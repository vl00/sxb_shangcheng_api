using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Activity.Appliaction.RequestModels;
using iSchool.Organization.Activity.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Activity.Appliaction.Service
{
    public class ExcellentCasesEvltPageQueryHandler : IRequestHandler<ExcellentCasesEvltPageQuery, ExcellentCasesEvltPageResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IConfiguration config;
        IMapper mapper;

        public ExcellentCasesEvltPageQueryHandler(IOrgUnitOfWork unitOfWork, IMapper mapper,
            IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IUserInfo me)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.config = config;
            this.mapper = mapper;
        }

        public async Task<ExcellentCasesEvltPageResult> Handle(ExcellentCasesEvltPageQuery query, CancellationToken cancellation)
        {
            var result = new ExcellentCasesEvltPageResult();

            var ainfo = await mediator.Send(new ActivitySimpleInfoQuery { Id = query.ActivityInfo.ActivityId });
            if (ainfo == null) throw new CustomResponseException("活动无效");
            result.ActivityData = mapper.Map<ActivityDataDto>(ainfo);
            if (result.ActivityData.Astatus != 0)
            {
                goto lb_end;
            }

            var ls = await redis.GetAsync<EvaluationItemDto[]>(CacheKeys.Hd1_excc);
            if (ls == null)
            {
                var sql = $@"
select a.activityid,e.id,e.no as id_s,e.title,e.stick,e.userid as AuthorId,e.status,e.IsPlaintext,e.cover,e.CreateTime
--,e.likes,e.Commentcount,e.Collectioncount,e.Viewcount
from ActivityExtend a
left join Evaluation e on a.contentid=e.id
where a.type={ActivityExtendType.Evaluation.ToInt()} and a.[group]={(1)} and a.activityid='{Consts.Activity1_Guid}'
and e.isvalid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()}
order by a.sort asc
";
                ls = (await unitOfWork.DbConnection.QueryAsync<EvaluationItemDto>(sql)).AsArray();
                foreach (var item in ls)
                {
                    item.Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(item.Id_s));
                }

                await redis.SetAsync(CacheKeys.Hd1_excc, ls, 60 * 60 * 24 * 1);
            }

            var rr = await mediator.Send(new UserSimpleInfoQuery { UserIds = ls.Select(_ => _.AuthorId).Distinct() });
            foreach (var item in ls)
            {
                var r = rr.FirstOrDefault(_ => _.Id == item.AuthorId);
                if (r == null) continue;
                item.AuthorName = r.Nickname;
                item.AuthorHeadImg = r.HeadImgUrl ?? config["AppSettings:UserDefaultHeadImg"];
            }

            var rr1 = await mediator.Send(new EvltLikesQuery { EvltIds = ls.Select(_ => _.Id).Distinct().ToArray() });
            foreach (var item in ls)
            {
                if (!rr1.Items.TryGetValue(item.Id, out var r)) continue;
                item.LikeCount = r.Likecount;
                item.IsLikeByMe = r.IsLikeByMe;
            }

            result.Items = ls;
            result.Pcode = query.ActivityInfo.IsHasPromo ? query.ActivityInfo.Promocode : query.ActivityInfo.Acode;
            result.Banner = config["AppSettings:hd1:banner_excc"];

            lb_end:
            return result;
        }

        
    }
}
