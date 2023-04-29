using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
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

namespace iSchool.Organization.Appliaction.Services
{
    public class MpMallOperateAreaQueryHandler : IRequestHandler<MpMallOperateAreaQuery, MpMallOperateAreaQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public MpMallOperateAreaQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<MpMallOperateAreaQryResult> Handle(MpMallOperateAreaQuery query, CancellationToken cancellation)
        {
            var result = new MpMallOperateAreaQryResult();
            await default(ValueTask);

            // 限时闪购s
            {
                var ls = await _redis.GetAsync<MpCourseDataDto[]>(CacheKeys.MpMallOperateArea_LimitedTimeOffers);
                if (ls == null)
                {
                    var sql = @"
SELECT top 4 o.authentication,c.id,c.no,isnull(c.banner_s,c.banner)as banner,c.Title,c.price,c.origprice,c.stock,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive,c.LimitedTimeOffer
from Course c left join [dbo].[Organization] o on o.id=c.orgid and o.IsValid=1  	
where c.IsValid=1 and c.status=1 and o.status=1 and o.authentication=1  and c.IsInvisibleOnline=0 and c.LimitedTimeOffer=1 and c.LastOffShelfTime>GETDATE()  
ORDER BY c.LastOffShelfTime 
";
                    ls = (await _orgUnitOfWork.QueryAsync<bool, Course, MpCourseDataDto>(sql, splitOn: "id", param: new { },
                        map: (authentication, course) =>
                        {
                            var dto = new MpCourseDataDto();
                            dto.Id = course.Id;
                            dto.Id_s = UrlShortIdUtil.Long2Base32(course.No);
                            dto.Banner = course.Banner?.ToObject<List<string>>();
                            dto.Authentication = authentication;
                            dto.Title = course.Title;
                            dto.Price = course.Price ?? 0;
                            dto.OrigPrice = course.Origprice;
                            dto.Stock = course.Stock ?? 0;
                            dto.LastOffShelfTime = course.LastOffShelfTime;
                            dto.NewUserExclusive = course.NewUserExclusive;
                            dto.CanNewUserReward = course.CanNewUserReward;
                            dto.LimitedTimeOffer = course.LimitedTimeOffer;
                            dto.Tags = OrderHelper.GetTagsFromCourse(course);
                            return dto;
                        })
                    ).AsArray();

                    await _redis.SetAsync(CacheKeys.MpMallOperateArea_LimitedTimeOffers, ls, 60 * 60 * 2);
                }
                result.LimitedTimeOffers = ls;
            }
            // 新人专享s
            {
                var ls = await _redis.GetAsync<MpCourseDataDto[]>(CacheKeys.MpMallOperateArea_NewUserExclusives);
                if (ls == null)
                {
                    var sql = @"
SELECT top 4 o.authentication,c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive,c.LimitedTimeOffer
from Course c left join [dbo].[Organization] o on o.id=c.orgid and o.IsValid=1  	
where c.NewUserExclusive=1 and c.IsInvisibleOnline=0 and c.IsValid=1 and c.status=1 and o.status=1 and o.authentication=1 
ORDER BY c.LastOffShelfTime 
";
                    ls = (await _orgUnitOfWork.QueryAsync<bool, Course, MpCourseDataDto>(sql, splitOn: "id", param: new { },
                        map: (authentication, course) =>
                        {
                            var dto = new MpCourseDataDto();
                            dto.Id = course.Id;
                            dto.Id_s = UrlShortIdUtil.Long2Base32(course.No);
                            dto.Banner = course.Banner?.ToObject<List<string>>();
                            dto.Authentication = authentication;
                            dto.Title = course.Title;
                            dto.Price = course.Price ?? 0;
                            dto.OrigPrice = course.Origprice;
                            dto.Stock = course.Stock ?? 0;
                            dto.LastOffShelfTime = course.LastOffShelfTime;
                            dto.NewUserExclusive = course.NewUserExclusive;
                            dto.CanNewUserReward = course.CanNewUserReward;
                            dto.LimitedTimeOffer = course.LimitedTimeOffer;
                            dto.Tags = OrderHelper.GetTagsFromCourse(course);
                            return dto;
                        })
                    ).AsArray();

                    await _redis.SetAsync(CacheKeys.MpMallOperateArea_NewUserExclusives, ls, 60 * 60 * 2);
                }
                result.NewUserExclusives = ls;
            }
            // 热销榜单s
            {
                var ls = await _redis.GetAsync<MpCourseDataDto[]>(CacheKeys.MpMallOperateArea_HotSells);
                if (ls == null)
                {
                    var sql = @"
SELECT top 4 o.authentication,c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,isnull(c.Sellcount,0) as Sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive,c.LimitedTimeOffer
from Course c left join [dbo].[Organization] o on o.id=c.orgid and o.IsValid=1  	
where c.IsValid=1 and c.status=1 and c.IsExplosions=1 and o.status=1 and o.authentication=1 and c.IsInvisibleOnline=0 and c.LimitedTimeOffer=0 and c.NewUserExclusive=0 
and c.IsExplosions=1 order by c.SetTop desc, c.LastOnShelfTime desc,isnull(c.Sellcount,0) desc

";
                    ls = (await _orgUnitOfWork.QueryAsync<bool, Course, MpCourseDataDto>(sql, splitOn: "id", param: new { },
                        map: (authentication, course) =>
                        {
                            var dto = new MpCourseDataDto();
                            dto.Id = course.Id;
                            dto.Id_s = UrlShortIdUtil.Long2Base32(course.No);
                            dto.Banner = course.Banner?.ToObject<List<string>>();
                            dto.Authentication = authentication;
                            dto.Title = course.Title;
                            dto.Price = course.Price ?? 0;
                            dto.OrigPrice = course.Origprice;
                            dto.Stock = course.Stock ?? 0;
                            dto.LastOffShelfTime = course.LastOffShelfTime;
                            dto.NewUserExclusive = course.NewUserExclusive;
                            dto.CanNewUserReward = course.CanNewUserReward;
                            dto.LimitedTimeOffer = course.LimitedTimeOffer;
                            dto.Tags = OrderHelper.GetTagsFromCourse(course);
                            return dto;
                        })
                    ).AsArray();

                    await _redis.SetAsync(CacheKeys.MpMallOperateArea_HotSells, ls, 60 * 60 * 2);
                }
                result.HotSells = ls;
            }
            // 本周上新s
            {
                var ls = await _redis.GetAsync<MpCourseDataDto[]>(CacheKeys.MpMallOperateArea_NewOnWeeks);
                if (ls == null)
                {
                    var sql = @"
SELECT top 4 o.authentication,c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,isnull(c.Sellcount,0) as Sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive,c.LimitedTimeOffer
from Course c left join [dbo].[Organization] o on o.id=c.orgid and o.IsValid=1  	
where c.IsValid=1 and c.status=1  and o.status=1 and o.authentication=1 and c.LimitedTimeOffer=0 and c.NewUserExclusive=0  
order by DATEDIFF(s,GETDATE(),ISNULL(c.LastOnShelfTime,c.CreateTime)) desc
";
                    ls = (await _orgUnitOfWork.QueryAsync<bool, Course, MpCourseDataDto>(sql, splitOn: "id", param: new { },
                        map: (authentication, course) =>
                        {
                            var dto = new MpCourseDataDto();
                            dto.Id = course.Id;
                            dto.Id_s = UrlShortIdUtil.Long2Base32(course.No);
                            dto.Banner = course.Banner?.ToObject<List<string>>();
                            dto.Authentication = authentication;
                            dto.Title = course.Title;
                            dto.Price = course.Price ?? 0;
                            dto.OrigPrice = course.Origprice;
                            dto.Stock = course.Stock ?? 0;
                            dto.LastOffShelfTime = course.LastOffShelfTime;
                            dto.NewUserExclusive = course.NewUserExclusive;
                            dto.CanNewUserReward = course.CanNewUserReward;
                            dto.LimitedTimeOffer = course.LimitedTimeOffer;
                            dto.Tags = OrderHelper.GetTagsFromCourse(course);
                            return dto;
                        })
                    ).AsArray();

                    await _redis.SetAsync(CacheKeys.MpMallOperateArea_NewOnWeeks, ls, 60 * 60 * 2);
                }
                result.NewOnWeeks = ls;
            }

            return result;
        }

    }
}
