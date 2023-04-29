using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class PcSpecialIndexQueryHandler : IRequestHandler<PcSpecialIndexQuery, PcSpecialIndexQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public PcSpecialIndexQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IUserInfo me,            
            IConfiguration config,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<PcSpecialIndexQueryResult> Handle(PcSpecialIndexQuery query, CancellationToken cancellation)
        {
            var result = new PcSpecialIndexQueryResult();
            var spId_s = UrlShortIdUtil.Long2Base32(query.No);
            await default(ValueTask);

            //if (query.PageIndex == 1)
            {
                result.Specials = await mediator.Send(new SimpleSpecialQuery { });
                result.CurrSpecial = result.Specials?.FirstOrDefault(_ => _.Id_s == spId_s);
                result.Me = me.IsAuthenticated ? me : null;
            }
            
            result.PageInfo = await GetPagedList(query, result.CurrSpecial?.Id);

            return result;
        }

        async Task<PagedList<EvaluationItemDto>> GetPagedList(PcSpecialIndexQuery query, Guid? id)
        {
            IEnumerable<EvaluationItemDto> items = null;
            int cc = 0;
            await default(ValueTask);

            if (id == null)
            {
                var special = await mediator.Send(new GetSpecialInfoQuery { No = query.No });
                if (special == null) throw new CustomResponseException($"无效专题no={query.No}");
                id = special.Id;
            }

            var rdk = CacheKeys.PC_SpclLs.FormatWith(id, query.PageSize, query.OrderBy);
            if (query.PageIndex == 1)
            {
                var jtk = await redis.GetAsync<JToken>(rdk);
                if (jtk != null)
                {
                    cc = (int)jtk["totalItemCount"];
                    items = jtk["page1_items"].ToObject<EvaluationItemDto[]>();
                }
            }

            if (items == null)
            {
                var sql = $@"
select s.id as specialid,evlt.*
from Special s 
left join SpecialBind sb on s.id=sb.specialid 
left join Evaluation evlt on sb.evaluationid=evlt.id 
where sb.IsValid=1 and evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} --and evlt.isPlaintext=@isPlaintext 
and s.id=@Id and s.status={SpecialStatusEnum.Ok.ToInt()}
";
                sql = $@"
select count(1) from ({sql}) T
;
{sql}
{"order by evlt.stick desc,evlt.commentcount desc,evlt.viewcount desc,evlt.CreateTime desc".If(query.OrderBy == 1)}
{"order by evlt.CreateTime desc".If(query.OrderBy == 2)}
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";

                var dyp = new DynamicParameters(query)
                    .Set("@id", id)
                    ;
                var gr = await unitOfWork.QueryMultipleAsync(sql, dyp);
                cc = await gr.ReadFirstAsync<int>();
                items = (await gr.ReadAsync<Evaluation>()).Select(evlt => mapper.Map<EvaluationItemDto>(evlt)).AsArray();

                if (query.PageIndex == 1)
                {
                    await redis.SetAsync(rdk, new { totalItemCount = cc, page1_items = items }, 60 * 10);
                }
            }
            var pg = items.ToPagedList(query.PageSize, query.PageIndex, cc);

            // user info
            var rr = await mediator.Send(new UserSimpleInfoQuery { UserIds = items.Select(_ => _.AuthorId).Distinct() });
            foreach (var item in items)
            {
                var r = rr.FirstOrDefault(_ => _.Id == item.AuthorId);
                if (r == null) continue;
                item.AuthorName = r.Nickname;
                item.AuthorHeadImg = r.HeadImgUrl ?? config["AppSettings:UserDefaultHeadImg"];
            }

            // likes
            var rr1 = await mediator.Send(new EvltLikesQuery { EvltIds = items.Select(_ => _.Id).Distinct().ToArray() });
            foreach (var item in items)
            {
                if (!rr1.Items.TryGetValue(item.Id, out var r)) continue;
                item.LikeCount = r.Likecount;
                item.IsLikeByMe = r.IsLikeByMe;
            }

            return pg;
        }
    }
}
