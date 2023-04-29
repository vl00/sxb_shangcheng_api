using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Appliaction.Service.PointsMall;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class MiniOrderCancelHandler : IRequestHandler<MiniOrderCancelCmd, bool>
    {
        private readonly IUserInfo me;
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        NLog.ILogger log;
        CSRedisClient redis;
        ICouponReceiveRepository _couponReceiveRepository;
        ILogger<MiniOrderCancelHandler> _logger;
        private readonly IPointsMallService _pointsMallService;
        public MiniOrderCancelHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, NLog.ILogger log, CSRedisClient redis,
            IUserInfo me
            , ICouponReceiveRepository couponReceiveRepository
            , ILogger<MiniOrderCancelHandler> logger
            , IPointsMallService pointsMallService)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.log = log;
            this.redis = redis;
            _couponReceiveRepository = couponReceiveRepository;
            _logger = logger;
            _pointsMallService = pointsMallService;
        }

        public async Task<bool> Handle(MiniOrderCancelCmd cmd, CancellationToken cancellation)
        {


            var advanceOrderId = cmd.OrderId;
            var orders = await _mediator.Send(new OrderDetailSimQuery { AdvanceOrderId = cmd.OrderId, IgnoreCheckExpired = true });
            if (orders == null || orders.Orders?.Length < 1)
            {
                throw new CustomResponseException("订单不存在.");
            }
            if (orders.Orders.Any(_ => _.OrderStatus != (int)OrderStatusV2.Unpaid))
            {
                throw new CustomResponseException("当前订单状态不允许取消.");
            }
            if (!cmd.IsFromExpired && orders.UserId != me.UserId)
            {
                throw new CustomResponseException("非法操作.");
            }

            for (var __ = !cmd.IsFromExpired; __; __ = !__)
            {
                // re回调实际已支付的单
                var b = await _mediator.Send(new TryRePayorderCommand { OrdersEntity = orders });
                if (b)
                {
                    throw new CustomResponseException("订单已支付.请刷新页面查看.");
                }
            }

            // up status to cancel
            {
                try
                {
                    _orgUnitOfWork.BeginTransaction();

                    var sql = $@"
update [order] set [status]={OrderStatusV2.Cancelled.ToInt()}, [ModifyDateTime]=getdate(),[Modifier]=@Modifier
    where [Id] in @OrderIds and [status]={OrderStatusV2.Unpaid.ToInt()}

update [OrderDetial] set [status]={OrderStatusV2.Cancelled.ToInt()} 
    where [orderid] in @OrderIds and [status]={OrderStatusV2.Unpaid.ToInt()}
";
                    var affectCount = await _orgUnitOfWork.DbConnection.ExecuteAsync(sql, new
                    {
                        OrderIds = orders.Orders.Select(_ => _.OrderId).Distinct(),
                        Modifier = cmd.IsFromExpired ? "00111111-1111-1111-1111-111111111100" : "11111111-1111-1111-1111-111111111110",
                    }, _orgUnitOfWork.DbTransaction);

                    if (affectCount <= 0)
                    {
                        _orgUnitOfWork.SafeRollback();
                        return false;
                    }
                    else _orgUnitOfWork.CommitChanges();
                }
                catch
                {
                    _orgUnitOfWork.SafeRollback();
                    return false;
                }
            }
            // 新人专享 - 未支付
            for (var __ = true; __; __ = !__)
            {
                var ks = orders.Orders.SelectMany(o => o.Prods.OfType<CourseOrderProdItemDto>().Select(_ => (o.UserId, o.AdvanceOrderId, o.AdvanceOrderNo, Prod: _)))
                    .Where(_ => _.Prod.NewUserExclusive == true)
                    .Select(_ => (CacheKeys.UnpaidOrderOfNewUserExclusive.FormatWith(_.Prod.ProdType, _.UserId), $"{_.AdvanceOrderNo}_{_.AdvanceOrderId:n}"))
                    .Distinct();
                if (!ks.Any()) break;

                foreach (var (k, v) in ks)
                {
                    try { await redis.LockExReleaseAsync(k, v); } catch { }
                }
            }

            // re back
            await Reback(orders.Orders.Select(_ => _.OrderId).Distinct().ToArray());

            //撤销对应的优惠券
            try { await _mediator.Send(new CancelCouponCommand() { OrderId = orders.AdvanceOrderId }); } catch (Exception ex){ _logger.LogError(ex, ""); }

            //回滚冻结积分
            try
            {
                Guid freezeId = await redis.GetAsync<Guid>(CacheKeys.FreezeIdCacheKey(advanceOrderId));
                if (freezeId != default(Guid))
                {
                    
                    await _pointsMallService.DeFreezePoints(freezeId, orders.UserId);
                    await redis.DelAsync(CacheKeys.FreezeIdCacheKey(advanceOrderId));
                }
            }
            catch(Exception ex){
                _logger.LogError(ex, "回滚冻结积分失败。advanceOrderId={advanceOrderId}", advanceOrderId);
            }

            //取消赠送的冻结积分
            foreach (var order in orders.Orders)
            {
                foreach (var courseOrderProdItemDto in order.Prods.OfType<CourseOrderProdItemDto>())
                {
                    try
                    {
                        var freezeId = await redis.GetAsync<Guid>(CacheKeys.PresentedFreezePointsCacheKey(courseOrderProdItemDto.OrderDetailId));
                        if (freezeId != default(Guid))
                        {
                            await _pointsMallService.DeductFreezePoints(freezeId, orders.UserId,6);
                            await redis.DelAsync(CacheKeys.PresentedFreezePointsCacheKey(courseOrderProdItemDto.OrderDetailId));
                        }
                      
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"扣除赠送冻结积分失败，orderDetailId = {courseOrderProdItemDto.OrderDetailId}");
                    }
                }

            }

            return true;
        }


        private async Task Reback(Guid[] order2Ids)
        {
            if (!order2Ids.Any()) return;

            // 过期回归库存
            for (var __ = true; __; __ = !__)
            {
                var sql = $@"
select productid,sum(number) from OrderDetial 
where status={OrderStatusV2.Cancelled.ToInt()} 
and orderid in @orders
group by productid
";
                var courses = await _orgUnitOfWork.DbConnection.QueryAsync<(Guid GoodsId, int Num)>(sql, new { orders = order2Ids });
                if (!courses.Any()) break;

                foreach (var (goodsId, num) in courses)
                {
                    try
                    {
                        await _mediator.Send(new CourseGoodsStockRequest
                        {
                            StockCmd = new GoodsStockCommand { Id = goodsId, Num = num * -1 }
                        });
                    }
                    catch { }
                }
            }

            // 归还rw积分
            for (var __ = true; __; __ = !__)
            {
                var sql = $@"
select unionID,type,sum(consumedScores) from(
select json_value(p.ctn,'$._RwInviteActivity.unionID')as unionID,json_value(p.ctn,'$._RwInviteActivity.courseExchange.type')as type,
try_convert(float,json_value(p.ctn,'$._RwInviteActivity.consumedScores'))as consumedScores
from OrderDetial p
where 1=1 and p.orderid in @orders and p.status={OrderStatusV2.Cancelled.ToInt()} 
)T where unionID is not null
group by unionID,type
";
                var ls = await _orgUnitOfWork.DbConnection.QueryAsync<(string UnionID, int Type, double ConsumedScores)>(sql, new { orders = order2Ids });
                if (!ls.Any()) break;

                foreach (var (unionID, type, scores) in ls)
                {
                    var args = new UserScoreOnRwInviteActivityArgs { UnionID = unionID }
                        .SetCourseExchangeType((CourseExchangeTypeEnum)type)
                        .PreConsume(-1 * scores);
                    try { await _mediator.Send(args); }
                    catch (Exception ex1)
                    {
                        var msg1 = GetLogMsg(args);
                        msg1.Properties["Error"] = $"订单过期后归还积分失败.err={ex1.Message}";
                        msg1.Properties["StackTrace"] = ex1.StackTrace;
                        msg1.Properties["ErrorCode"] = Consts.Err.OrderCreate_RwInviteActivity_ErrOnRollBack;
                        log.Error(msg1);
                    }
                }
            }
        }

        NLog.LogEventInfo GetLogMsg(object paramsObj = null)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
            msg.Properties["Caption"] = "取消订单";
            msg.Properties["UserId"] = me.UserId;
            msg.Properties["Level"] = "Error";
            if (paramsObj is string str) msg.Properties["Params"] = str;
            else if (paramsObj != null) msg.Properties["Params"] = (paramsObj).ToJsonString(camelCase: true);
            //msg.Properties["Error"] = $"检测敏感词意外失败.网络异常.err={ex.Message}";
            //msg.Properties["StackTrace"] = ex.StackTrace;
            //msg.Properties["ErrorCode"] = 3;
            return msg;
        }
    }
}
