using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    
    public class OrgRelatedEvaluationQueryHandler : IRequestHandler<OrgRelatedEvaluationQuery, LoadMoreResult<EvaluationItemDto>>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        ElvtMainPageSizeOption pageSizeOption;
        IMapper mapper;

        const int cache_min = 60 * 10;

        public OrgRelatedEvaluationQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IUserInfo me,
            IOptionsSnapshot<ElvtMainPageSizeOption> pageSizeOption,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.pageSizeOption = pageSizeOption.Value;
            this.mapper = mapper;
        }

        public async Task<LoadMoreResult<EvaluationItemDto>> Handle(OrgRelatedEvaluationQuery req, CancellationToken cancellation)
        {
            var rdk = CacheKeys.OrgRelatedEvaluation.FormatWith(req.OrgId);
            var pg = new LoadMoreResult<EvaluationItemDto>();
            pg.CurrPageIndex = req.PageIndex;
            EvaluationItemDto[] items = null;

            var (cc, c1, c0) = await GetTotal(req);
            pg.TotalPageCount = Math.Max(GetTotalPageCount(c1, pageSizeOption.Size1), GetTotalPageCount(c0, pageSizeOption.Size0));

            // 快速比较超出总页数
            if (pg.CurrPageIndex > pg.TotalPageCount)
            {
                items = new EvaluationItemDto[0];
                goto LB_END;
            }

            // first page try find from redis cache first
            if (pg.CurrPageIndex == 1)
            {
                items = await redis.GetAsync<EvaluationItemDto[]>(rdk);
                if (items != null) goto LB_END;
            }

            // pagesize 有图:无图 == 20:5            
            try
            {
                unitOfWork.ReadDbConnection.TryOpen();
                var t1 = GetItems(req, false, pageSizeOption.Size1);
                var t0 = GetItems(req, true, pageSizeOption.Size0);
                await Task.WhenAll(t1, t0);
                // 合并也要排序
                items = t1.Result.Union(t0.Result).OrderByDescending(x => x, new FuncComparer<EvaluationItemDto>((x1, x2) =>
                {
                    
                    if (x1.ViewCount != x2.ViewCount) return x1.ViewCount > x2.ViewCount ? 1 : -1;
                    if (x1.CreateTime != x2.CreateTime) return x1.CreateTime > x2.CreateTime ? 1 : -1;
                    return 0;
                }))
                .ToArray();
            }
            finally
            {
                unitOfWork.ReadDbConnection.Close();
            }

            // 查用户信息
            var uInfos = await mediator.Send(new UserSimpleInfoQuery
            {
                UserIds = items.Select(_ => _.AuthorId)
            });
            foreach (var u in uInfos)
            {
                foreach (var u0 in items.Where(_ => _.AuthorId == u.Id))
                {
                    u0.AuthorName = u.Nickname;
                    u0.AuthorHeadImg = u.HeadImgUrl;
                }
            }

            // fisrt page set to redis cache 
            if (pg.CurrPageIndex == 1)
            {
                await redis.SetAsync(rdk, items, cache_min);
            }

        LB_END:
            pg.CurrItems = items;
            return pg;
        }

        // 获取总item数
        async Task<(long cc, long c1, long c0)> GetTotal(OrgRelatedEvaluationQuery req)
        {
            var k = CacheKeys.OrgRelatedEvaluation_Total.FormatWith(req.OrgId);
            var total = await redis.HGetAllAsync(k);
            if (total?.Any() == true) return (Convert.ToInt64(total["cc"]), Convert.ToInt64(total["c1"]), Convert.ToInt64(total["c0"]));

            var pdy = new DynamicParameters();
            pdy.Add("@id", req.OrgId);
            pdy.Add("@status",Consts.EvltOkStatus);


            var where = $@" and (exists(select 1 from EvaluationBind b join [dbo].[Organization]  o on o.id=b.orgid and o.IsValid=1
                        	where b.IsValid=1 and b.evaluationid=evlt.id  and o.id=@id))  ";
            var sql = $@"
                        select count(1) as cc,
                        isnull(sum(case when evlt.isPlaintext=1 then 1 else 0 end),0) as c1,
                        isnull(sum(case when evlt.isPlaintext=1 then 0 else 1 end),0) as c0
                        from Evaluation evlt
                        where evlt.IsValid=1 and evlt.status=@status
                        {where}
                        ;";
           
            var dy = await unitOfWork.QueryFirstAsync(sql, pdy);
            var r = ((long)Convert.ToInt64(dy.cc), (long)Convert.ToInt64(dy.c1), (long)Convert.ToInt64(dy.c0));

            await redis.StartPipe()
                .HSet(k, "cc", r.Item1)
                .HSet(k, "c1", r.Item2)
                .HSet(k, "c0", r.Item3)
                .Expire(k, cache_min)
                .EndPipeAsync();

            return r;
        }

        // 获取当前有/无图item
        async Task<IEnumerable<EvaluationItemDto>> GetItems(OrgRelatedEvaluationQuery req, bool isPlaintext, int pagesize)
        {
            var sql = $@"
                        select evlt.*
                        from Evaluation evlt
                        where evlt.IsValid=1 and evlt.status=@status and evlt.isPlaintext=@isPlaintext {{0}}
                        order by evlt.viewcount desc,evlt.CreateTime desc
                        OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
                        ";

            var where = $@" and (exists(select 1 from EvaluationBind b join [dbo].[Organization]  o on o.id=b.orgid and o.IsValid=1
                        	where b.IsValid=1 and b.evaluationid=evlt.id  and o.id=@OrgId))  ";

            sql = sql.FormatWith(where);
           
            var dyp = new DynamicParameters(req)
                .Set("PageSize", pagesize)
                .Set("isPlaintext", isPlaintext)
                .Set("status", Consts.EvltOkStatus)
                .Set("OrgId", req.OrgId)
                ;
            var qs = await unitOfWork.QueryAsync<Evaluation>(sql, dyp);

            var items = qs.Select(evlt => mapper.Map<EvaluationItemDto>(evlt));
            return items;
        }

        static IEnumerable<int> GetMainSubj()
        {
            yield return (int)SubjectEnum.English;
            yield return (int)SubjectEnum.Chinese;
            yield return (int)SubjectEnum.Math;
        }

        static int GetTotalPageCount(long totalItemCount, int pageSize)
        {
            return (int)Math.Ceiling(totalItemCount / (double)pageSize);
        }
    }

}
