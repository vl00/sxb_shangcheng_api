using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class CourseGoodsStockReqResHandler : GoodsStockReqResHandler<CourseGoodsStockRequest, CourseGoodsStockResponse>,
        IRequestHandler<CourseGoodsStockRequest, CourseGoodsStockResponse>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;

        private static TodoX<Guid> _todoX;        

        public CourseGoodsStockReqResHandler(IOrgUnitOfWork orgUnitOfWork, IHostApplicationLifetime lifetime,
            CSRedisClient redis, IServiceProvider services) 
            : base(redis, services)
        {
            _orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;

            if (_todoX == null && Interlocked.CompareExchange(ref _todoX, 
                new TodoX<Guid>(5, null, SyncCourseGoodsStockToTbCourse, AsyncUtils.ServiceScopeFactory), null) == null)
            {
                lifetime.ApplicationStopping.Register(() => _todoX.OnCancel().Wait());
            }
        }

        protected override string OnGetCacheKey(Guid id)
        {
            return CacheKeys.CourseGoodsStock.FormatWith(id);
        }

        protected override async Task<int?> OnLoadGoodsStock(Guid id)
        {
            var sql = " select stock from [CourseGoods] where id=@id ";
            var stock = await _orgUnitOfWork.DbConnection.ExecuteScalarAsync<int?>(sql, new { id });
            return stock;
        }

        protected override async Task<bool> OnSaveGoodsStock(Guid id, int stock1, int? stock0)
        {
            var sql = $@"
update [CourseGoods] set stock=@stock1 where id=@id and IsValid=1 {"and [stock]=@stock0".If(stock0 != null)}
";
            var i = await _orgUnitOfWork.DbConnection.ExecuteAsync(sql, new { id, stock0, stock1 });
            var b = i > 0;
            if (b)
            {
                _todoX.AddItem(id);
            }
            return b;
        }

        protected override async Task OnBgResetGoodsStock(Guid id, int stockcount)
        {
            var sql = @"--,[count]=@stockcount
update [CourseGoods] set stock=@stockcount where id=@id 
";
            await _orgUnitOfWork.DbConnection.ExecuteAsync(sql, new { id, stockcount });

            _todoX.AddItem(id);
        }

        #region Obsolete
//        [Obsolete]
//        static async Task SyncCourseGoodsStockToTbCourse(IServiceProvider services, object state)
//        {
//            var goodsId = (Guid)state;
//            var _orgUnitOfWork = services.GetService<IOrgUnitOfWork>() as OrgUnitOfWork;

//            var sql = @"
//declare @courseId uniqueidentifier 
//declare @stock int

//select top 1 @courseId=courseId from [CourseGoods] where id=@goodsId ;
//select @stock=sum(stock) from [CourseGoods] where courseId=@courseId and IsValid=1 and show=1

//update [Course] set stock=@stock where id=@courseId 
//";
//            await _orgUnitOfWork.ExecuteAsync(sql, new { goodsId });
//        }
        #endregion

        static async Task SyncCourseGoodsStockToTbCourse(IEnumerable<Guid> goodsIds, object args)
        {
            using var spscope = ((IServiceScopeFactory)args)!.CreateScope();
            var services = spscope.ServiceProvider;
            var _orgUnitOfWork = services.GetService<IOrgUnitOfWork>() as OrgUnitOfWork;
            await default(ValueTask);

            var sql = @"
select g.courseId,sum(g.stock) as stocks,sum(g.[count]) as counts 
from [CourseGoods] g join (
    select CourseId from [CourseGoods] where IsValid=1 and show=1 and Id in @goodsIds 
    group by CourseId 
) c on c.CourseId=g.CourseId
where g.IsValid=1 and g.show=1 
group by g.CourseId
";
            var ls = await _orgUnitOfWork.DbConnection.QueryAsync<(Guid CourseId, int Stocks, int Counts)>(sql, new { goodsIds });
            if (!ls.Any()) return;

            sql = @"--,[count]=@Counts
update [Course] set stock=@Stocks where id=@CourseId 
";
            await _orgUnitOfWork.ExecuteAsync(sql, ls.Select(x => new { x.CourseId, x.Stocks, x.Counts }));
        }



        internal class TodoX<T>
        {
            private readonly TimeSpan _timeWin;
            private readonly int _size;
            private readonly Func<IEnumerable<T>, object, Task> _todo;
            private readonly object _args;
            private readonly Func<bool> _spinCondition;

            private readonly List<T> _list;
            private Task _ir;
            private int _ic;

            public TodoX(int size, TimeSpan? timeWin, Func<IEnumerable<T>, object, Task> func, object args)
            {
                _size = size < 2 ? 2 : size;
                _timeWin = timeWin == null || timeWin.Value < TimeSpan.FromSeconds(2) ? TimeSpan.FromSeconds(2) : timeWin.Value;
                _todo = func;
                _args = args;
                _list = new List<T>(_size);
                _spinCondition = this.TryGet_ic;
            }

            public void AddItem(T item)
            {
                lock (_list)
                {
                    _list.Add(item);
                    OnAddedWithLocked();
                }
            }

            public void AddItems(IEnumerable<T> items)
            {
                lock (_list)
                {
                    _list.AddRange(items);
                    OnAddedWithLocked();
                }
            }

            private void OnAddedWithLocked()
            {
                if (_list.Count < _size)
                {
                    SetTimeoutTodo();
                }
                else
                {
                    if (!this.TryGet_ic()) return;
                    Interlocked.Exchange(ref _ir, null);
                    _ = this.OnTodo();
                }
            }

            private void SetTimeoutTodo()
            {
                var t = Task.Delay(_timeWin);
                Interlocked.Exchange(ref _ir, t);
                t.ContinueWith(OnTimeout, this);
            }

            private static void OnTimeout(Task task, object o)
            {
                var _this = (TodoX<T>)o!;
                if (Interlocked.CompareExchange(ref _this._ir, null, task) != task) return;
                if (!_this.TryGet_ic()) return;
                _ = _this.OnTodo();
            }

            private async Task OnTodo()
            {
                Exception ex0 = null;
                T[] arr = null;
                var need_NextTodo = false;
                do
                {
                    if (arr == null)
                    {
                        lock (_list)
                        {
                            if (_todo != null) arr = _list.ToArray();
                            else
                            {
                                _list.Clear();
                                break;
                            }
                        }
                    }
                    try
                    {
                        var t = _todo!.Invoke(arr, _args);
                        if (t != null && !t.IsCompletedSuccessfully) await t.ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        ex0 = ex;
                    }
                    lock (_list)
                    {
                        _list.RemoveRange(0, arr.Length);
                        if (ex0 != null) break;
                        var c = _list.Count;
                        if (c < _size)
                        {
                            if (c > 0) need_NextTodo = true;
                            break;
                        }
                        else arr = _list.ToArray();
                    }
                }
                while (true);
                Interlocked.CompareExchange(ref _ic, 0, 1);
                if (need_NextTodo) SetTimeoutTodo();
                if (ex0 != null) throw ex0;
            }

            public Task OnCancel()
            {
                if (!TryGet_ic()) SpinWait.SpinUntil(_spinCondition); 
                return OnTodo();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryGet_ic() => Interlocked.CompareExchange(ref _ic, 1, 0) == 0;
        }
    }
}
