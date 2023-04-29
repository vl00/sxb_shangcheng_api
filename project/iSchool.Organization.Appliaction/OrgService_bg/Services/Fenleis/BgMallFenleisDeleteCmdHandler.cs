using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services
{
    public class BgMallFenleisDeleteCmdHandler : IRequestHandler<BgMallFenleisDeleteCmd, object>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public BgMallFenleisDeleteCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<object> Handle(BgMallFenleisDeleteCmd cmd, CancellationToken cancellation)
        {
            var todels = new List<KeyValue>(16);
            var now = DateTime.Now;
            await default(ValueTask);

            var self = await GetKV(cmd.Code) ?? throw new CustomResponseException("分类不存在或被删除了");
            todels.Add(self);

            if (self.Depth < Consts.MallFenlei_MaxDepth)
            {
                await GetChildrenByPcode(todels, self.Key);
            }

            // 删除3级的关联数据
            var sql_ass = "";
            var ls_smLogUserOperations = new List<SmLogUserOperation>();
            if (todels.Where(_ => _.Depth == Consts.MallFenlei_MaxDepth).ToArray() is KeyValue[] kv3 && kv3.Length > 0)
            {
                if (kv3.Length > 1000) throw new CustomResponseException("该分类级联的所有3级分类数量超过1000,请各自分开删除");

                var code3s = kv3.Select(_ => _.Key).ToArray();

                // 删除热门分类
                sql_ass += $"update PopularClassify set IsValid=0,ModifyDateTime=getdate(),Modifier='{cmd.UserId}' where IsValid=1 and ClassifyKey in ({string.Join(',', code3s)});\n";

                // course表
                var ass_courses = await _orgUnitOfWork.DbConnection.QueryAsync<(Guid Id, string Title, string CommodityTypes, string New_CommodityTypes)>($@"
select c.id,c.title,c.CommodityTypes
,'['+isnull((select string_agg([value],',') from [dbo].[fn_cbStrT0](substring(c.CommodityTypes,2,len(c.CommodityTypes)-2),'{string.Join(',', code3s)}',',',-1)),'')+']'
from course c
where c.IsValid=1 and isjson(c.CommodityTypes)=1 and len(c.CommodityTypes)>2
and id in(select c.id from course c outer apply openjson(c.CommodityTypes)j
    where c.IsValid=1 and isjson(c.CommodityTypes)=1 and c.CommodityTypes<>'[]'
    and j.[value] in ({string.Join(',', code3s)}) 
) 
");
                foreach (var course in ass_courses)
                {
                    sql_ass += $"update [Course] set [CommodityTypes]='{course.New_CommodityTypes}' where Id='{course.Id}';\n";

                    ls_smLogUserOperations.Add(new SmLogUserOperation().SetUserId(cmd.UserId)
                        .SetClass(nameof(BgMallFenleisDeleteCmd))
                        .SetParams("New_CommodityTypes", course.New_CommodityTypes)
                        .SetParams("courseid", course.Id)
                        .SetDesc("商城分类被删除联动删除Course表相关分类")
                        .SetOldata("course", course)
                        .SetTime(DateTime.Now));
                }

                // Organization表
                var ass_orga = await _orgUnitOfWork.DbConnection.QueryAsync<(Guid Id, string Name, string BrandTypes, string New_BrandTypes)>($@"
select o.id,o.name,o.BrandTypes
,'['+isnull((select string_agg([value],',') from [dbo].[fn_cbStrT0](substring(o.BrandTypes,2,len(o.BrandTypes)-2),'{string.Join(',', code3s)}',',',-1)),'')+']'
from [dbo].[Organization] o where o.IsValid=1 and isjson(o.BrandTypes)=1 and len(o.BrandTypes)>2
and id in(select o.id from [Organization] o outer apply openjson(o.BrandTypes)j
    where o.IsValid=1 and isjson(o.BrandTypes)=1 and o.BrandTypes<>'[]'
    and j.[value] in ({string.Join(',', code3s)}) 
) 
");
                foreach (var org in ass_orga)
                {
                    sql_ass += $"update [Organization] set [BrandTypes]='{org.New_BrandTypes}' where Id='{org.Id}';\n";

                    ls_smLogUserOperations.Add(new SmLogUserOperation().SetUserId(cmd.UserId)
                        .SetClass(nameof(BgMallFenleisDeleteCmd))
                        .SetParams("New_BrandTypes", org.New_BrandTypes)
                        .SetParams("orgid", org.Id)
                        .SetDesc("商城分类被删除联动删除Organization表相关分类")
                        .SetOldata("organization", org)
                        .SetTime(DateTime.Now));
                }
            }

            // try do del
            for (var __ = todels.Any(); __; __ = !__)
            {
                var sql = $"update [KeyValue] set IsValid=0 where type=@ty16 and IsValid=1 and [key]=@code";
                var i = await _orgUnitOfWork.ExecuteAsync(sql, new { ty16 = Consts.Kvty_MallFenlei, code = todels[0].Key });
                if (i < 1) break;

                try
                {
                    _orgUnitOfWork.BeginTransaction();

                    foreach (var kvs in SplitArr(todels.Skip(1), 500))
                    {
                        sql = $"update [KeyValue] set IsValid=0 where type=@ty16 and IsValid=1 and [key] in @codes";
                        await _orgUnitOfWork.ExecuteAsync(sql, new { ty16 = Consts.Kvty_MallFenlei, codes = kvs.Select(_ => _.Key).ToArray() }, _orgUnitOfWork.DbTransaction);
                    }

                    if (!sql_ass.IsNullOrEmpty())
                    {
                        await _orgUnitOfWork.ExecuteAsync(sql_ass, new { }, _orgUnitOfWork.DbTransaction);
                    }

                    _orgUnitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    _orgUnitOfWork.SafeRollback();
                    throw new CustomResponseException("删除失败:" + ex.Message);
                }
            }


            // clear cache
            await _mediator.Send(new ClearRedisCacheCmd
            {
                Keys = new[]
                {
                    CacheKeys.MallFenlei_DelFontKeys,
                    "org:course:*",
                    "org:courses:*",
                    "org:organizations:*",
                    "org:organization:*",
                }
            });

            // add user log
            await _mediator.Publish(new SmLogUserOperation().SetUserId(cmd.UserId)
                .SetClass(nameof(BgMallFenleisDeleteCmd))
                .SetParams("_", cmd)                
                .SetOldata("keyvalue", todels)
                .SetTime(now));

            foreach (var smlog in ls_smLogUserOperations)
            {
                await _mediator.Publish(smlog);
            }

            return Unit.Value;
        }

        async Task<KeyValue> GetKV(int code)
        {
            var sql = $"select top 1 * from [keyvalue] where IsValid=1 and [type]=@ty16 and [key]=@code";
            var kv = await _orgUnitOfWork.QueryFirstOrDefaultAsync<KeyValue>(sql, new { code, ty16 = Consts.Kvty_MallFenlei });
            return kv;
        }

        async Task GetChildrenByPcode(List<KeyValue> todels, int pcode)
        {
            var sql = "select * from KeyValue kv where kv.isvalid=1 and kv.type=@ty16 and kv.parent=@pcode order by sort";
            var ls = await _orgUnitOfWork.QueryAsync<KeyValue>(sql, new { pcode, ty16 = Consts.Kvty_MallFenlei });
            
            foreach (var kv in ls)
            {
                todels.Add(kv);
                await GetChildrenByPcode(todels, kv.Key);
            }
        }

        static IEnumerable<T[]> SplitArr<T>(IEnumerable<T> collection, int c)
        {
            for (var arr = collection; arr.Any();)
            {
                yield return arr.Take(c).ToArray();
                arr = arr.Skip(c);
            }
        }
    }
}
