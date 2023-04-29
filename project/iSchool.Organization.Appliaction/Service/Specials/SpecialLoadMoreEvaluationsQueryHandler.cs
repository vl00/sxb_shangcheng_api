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

namespace iSchool.Organization.Appliaction.Service
{
    public class SpecialLoadMoreEvaluationsQueryHandler : IRequestHandler<SpecialLoadMoreEvaluationsQuery, LoadMoreResult<EvaluationItemDto>>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        ElvtMainPageSizeOption pageSizeOption;
        IMapper mapper;

        const int cache_min = 60 * 10;

        public SpecialLoadMoreEvaluationsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IUserInfo me,
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

        public async Task<LoadMoreResult<EvaluationItemDto>> Handle(SpecialLoadMoreEvaluationsQuery req, CancellationToken cancellation)
        {            
            var rdk =req.SpecialType==SpecialTypeEnum.SmallSpecial.ToInt()? CacheKeys.Rdk_spclLs.FormatWith(req.Id, req.OrderBy):CacheKeys.Rdk_big_spclLs.FormatWith(req.Id, req.OrderBy,req.SmallId);
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
                var t1 = GetItems(req, false, pageSizeOption.Size1,req.SmallId);
                var t0 = GetItems(req, true, pageSizeOption.Size0, req.SmallId);
                await Task.WhenAll(t1, t0);
                // 合并也要排序
                items = t1.Result.Union(t0.Result).OrderByDescending(x => x, new FuncComparer<EvaluationItemDto>((x1, x2) =>
                {
                    //最热
                    if (req.OrderBy == 1)
                    {
                        if (x1.Stick && !x2.Stick) return 1;
                        if (!x1.Stick && x2.Stick) return -1;
                        if (x1.CommentCount != x2.CommentCount) return x1.CommentCount > x2.CommentCount ? 1 : -1;
                        if (x1.CreateTime != x2.CreateTime) return x1.CreateTime > x2.CreateTime ? 1 : -1;
                    }
                    else //if (req.OrderBy == 2) //最新
                    {
                        if (x1.CreateTime != x2.CreateTime) return x1.CreateTime > x2.CreateTime ? 1 : -1;
                    }                    
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
        async Task<(long cc, long c1, long c0)> GetTotal(SpecialLoadMoreEvaluationsQuery req)
        {            
            var k =req.SpecialType==SpecialTypeEnum.SmallSpecial.ToInt()? CacheKeys.Rdk_spclLsTotal.FormatWith(req.Id, req.OrderBy): CacheKeys.Rdk_big_spclLsTotal.FormatWith(req.Id, req.OrderBy, req.SmallId);
            var total = await redis.HGetAllAsync(k);
            if (total?.Any() == true) return (Convert.ToInt64(total["cc"]), Convert.ToInt64(total["c1"]), Convert.ToInt64(total["c0"]));

            var sql = "";
            var dp = new DynamicParameters().Set("Id", req.Id);

            if (req.SpecialType == SpecialTypeEnum.SmallSpecial.ToInt())//小专题，用原来的逻辑
            {
                sql = $@"
select count(1) as cc,
isnull(sum(case when evlt.isPlaintext=1 then 0 else 1 end),0) as c1,
isnull(sum(case when evlt.isPlaintext=1 then 1 else 0 end),0) as c0
from Evaluation evlt
left join SpecialBind sb on sb.evaluationid=evlt.id and sb.IsValid=1
left join Special s on s.id=sb.specialid and s.status={SpecialStatusEnum.Ok.ToInt()}
where s.IsValid=1 and evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()}
and s.id=@Id "
;
            }
            else if (req.SpecialType == SpecialTypeEnum.BigSpecial.ToInt())//大专题，
            {
                sql = $@"
select count(1) as cc,
isnull(sum(case when evlt.isPlaintext=1 then 0 else 1 end),0) as c1,
isnull(sum(case when evlt.isPlaintext=1 then 1 else 0 end),0) as c0
from Evaluation evlt
left join SpecialBind sb on sb.evaluationid=evlt.id and sb.IsValid=1
left join Special s on s.id=sb.specialid and s.status={SpecialStatusEnum.Ok.ToInt()}
left join SpecialSeries ss on s.id=ss.smallspecial and ss.IsValid=1
where s.IsValid=1 and evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()}
and ss.bigspecial=@Id {" and s.id=@smallId".If(req.SmallId != default)} "
;
                if (req.SmallId != default)
                    dp.Set("smallId", req.SmallId);
            }
  
           
            var dy = await unitOfWork.QueryFirstAsync(sql, dp);
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
        async Task<IEnumerable<EvaluationItemDto>> GetItems(SpecialLoadMoreEvaluationsQuery req, bool isPlaintext, int pagesize,Guid smallId)
        {
            var dyp = new DynamicParameters(req)
               .Set("PageSize", pagesize)
               .Set("isPlaintext", isPlaintext)
               ;
            var sql = "";
            if (req.SpecialType == SpecialTypeEnum.SmallSpecial.ToInt())
            {
                sql = $@"
select s.id as specialid,evlt.*
from Special s 
left join SpecialBind sb on s.id=sb.specialid 
left join Evaluation evlt on sb.evaluationid=evlt.id 
where sb.IsValid=1 and evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} and evlt.isPlaintext=@isPlaintext 
and s.id=@Id and s.status={SpecialStatusEnum.Ok.ToInt()}
{"order by evlt.stick desc,evlt.commentcount desc,evlt.viewcount desc,evlt.CreateTime desc".If(req.OrderBy == 1)}
{"order by evlt.CreateTime desc".If(req.OrderBy == 2)}
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
            }
            else if(req.SpecialType==SpecialTypeEnum.BigSpecial.ToInt())//大专题
            {
                sql = $@"
select s.id as specialid,evlt.*
from Special s 
left join SpecialBind sb on s.id=sb.specialid 
left join Evaluation evlt on sb.evaluationid=evlt.id 
left join SpecialSeries ss on s.id=ss.smallspecial and ss.IsValid=1
where sb.IsValid=1 and evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} and evlt.isPlaintext=@isPlaintext 
and ss.bigspecial=@Id {" and s.id=@smallId".If(req.SmallId != default)} and s.status={SpecialStatusEnum.Ok.ToInt()}
{"order by evlt.stick desc,evlt.commentcount desc,evlt.viewcount desc,evlt.CreateTime desc".If(req.OrderBy == 1)}
{"order by evlt.CreateTime desc".If(req.OrderBy == 2)}
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
                if (req.SmallId != default)
                    dyp.Set("smallId", smallId);
            }
           
            var qs = await unitOfWork.QueryAsync<Evaluation>(sql, dyp);

            var items = qs.Select(evlt => mapper.Map<EvaluationItemDto>(evlt));
            return items;
        }

        static int GetTotalPageCount(long totalItemCount, int pageSize)
        {
            return (int)Math.Ceiling(totalItemCount / (double)pageSize);
        }
    }
}
