using CSRedis;
using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class SimpleSpecialQueryHandler : IRequestHandler<SimpleSpecialQuery, List<SimpleSpecialDto>>
    {
        private readonly IRepository<Special> _specialRepository;
        private readonly CSRedisClient redis;
        private readonly OrgUnitOfWork _unitOfWork;
        IUserInfo me;
        IMediator mediator;

        public SimpleSpecialQueryHandler(IRepository<Special> specialRepository, CSRedisClient redisClient, 
            IUserInfo me, IMediator mediator,
            IOrgUnitOfWork unitOfWork)
        {
            _specialRepository = specialRepository;
            redis = redisClient;
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            this.mediator = mediator;
        }

        public async Task<List<SimpleSpecialDto>> Handle(SimpleSpecialQuery query, CancellationToken cancellation)
        {
            var hdInfo = query.Code.IsNullOrEmpty() ? null : await mediator.Send(new HdDataInfoQuery { Code = query.Code });
            if (hdInfo != null && hdInfo.GetFrStatus() != ActivityFrontStatus.Ok)
                throw new CustomResponseException($"活动{EnumUtil.GetDesc(hdInfo.GetFrStatus())}");

            var rdk = hdInfo == null ? CacheKeys.simplespecial : CacheKeys.simplespecial_acd.FormatWith(hdInfo.Id);
            var list = await redis.GetAsync<List<SimpleSpecialDto>>(rdk);
            if (list == null)
            {
                #region 会报错？？
                //var result = _specialRepository.GetAll(p => p.IsValid == true && p.Status == SpecialStatusEnum.Ok.ToInt()).AsArray();
                //list = result.Select(p => new SimpleSpecialDto 
                //{ 
                //    Id = p.Id, 
                //    Title = p.Title,
                //    Id_s = UrlShortIdUtil.Long2Base32(p.No),
                //    Banner = p.Banner,
                //}).ToList();
                #endregion

                var sql = $@"
select s.id,s.no,s.title,s.banner,ae.activityid,a.acode,a.type,a.status,a.starttime,a.endtime
from [Special] s left join ActivityExtend ae on ae.type={ActivityExtendType.Special.ToInt()} and ae.contentid=s.id and ae.IsValid=1
left join Activity a on a.IsValid=1 and isnull(a.status,{ActivityStatus.Ok.ToInt()})={ActivityStatus.Ok.ToInt()} and a.id=ae.activityid
where s.IsValid=1 and s.status={SpecialStatusEnum.Ok.ToInt()} and s.type={SpecialTypeEnum.SmallSpecial.ToInt()}
{"and a.acode=@Acode".If(hdInfo != null)}
order by s.sort
";
                list = (await _unitOfWork.QueryAsync<Special, Guid?, Activity, SimpleSpecialDto>(sql,
                    splitOn: "activityid,acode",
                    param: new { hdInfo?.Acode },
                    map: (spcl, activityid, activity) =>
                    {
                        if (activity != null)
                        {
                            activity.Id = activityid ?? default;
                            activity.IsValid = true;
                        }
                        var dto = new SimpleSpecialDto
                        {
                            Id = spcl.Id,
                            Title = spcl.Title,
                            Id_s = UrlShortIdUtil.Long2Base32(spcl.No),
                            Banner = spcl.Banner,
                        };
                        if (activity == null)
                        {
                            dto.Acode = null;
                            dto.Atype = null;
                        }
                        else if (HdDataInfoDto.GetFrStatus(activity) is ActivityFrontStatus status)
                        {
                            var a = status == ActivityFrontStatus.Ok ? (activity.Acode, activity.Type) : 
                                status == ActivityFrontStatus.Expired || status == ActivityFrontStatus.Fail ? (default(string), (int?)null) :
                                ("", null);

                            dto.Acode = a.Item1;
                            dto.Atype = a.Item2;
                        }  
                        return dto;
                    })).AsList();

                await redis.SetAsync(rdk, list, TimeSpan.FromDays(1));
            }
            return list.Where(_ => _.Acode != "").AsList();
        }
    }
}
