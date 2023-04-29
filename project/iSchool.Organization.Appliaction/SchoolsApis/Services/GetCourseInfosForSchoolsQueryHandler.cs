using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.SchoolsApis
{
    public class GetCourseInfosForSchoolsQueryHandler : IRequestHandler<GetCourseInfosForSchoolsQuery, GetCourseInfosForSchoolsQryResult>
    {
        IConfiguration _config;
        CSRedisClient _redis;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;

        public GetCourseInfosForSchoolsQueryHandler(IConfiguration config, CSRedisClient redis, IOrgUnitOfWork orgUnitOfWork,
            IMediator mediator)
        {
            this._config = config;
            this._redis = redis;
            this._mediator = mediator;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<GetCourseInfosForSchoolsQryResult> Handle(GetCourseInfosForSchoolsQuery query, CancellationToken cancellation)
        {
            var result = new GetCourseInfosForSchoolsQryResult();

            var length = (query.Ids?.Length ?? 0) + (query.SIds?.Length ?? 0);
            if (length == 0)
            {
                result.Courses = new List<PcCourseItemDto3>();
                return result;
            }

            var courses = new List<PcCourseItemDto3>();


            var errSids = new List<string>();
            var errIds = new List<Guid>();
            //获取课程信息
            await GetCourses(query, courses, errSids, errIds);


            // urls            
            foreach (var item in courses)
            {
                item.MUrl = $"{_config["BaseUrls:org-m"]}/course/detail/{item.Id_s}";
                item.PcUrl = $"{_config["BaseUrls:org-pc"]}/course/detail/{item.Id_s}";

                // mpqrcode
                if (query.Mp)
                {
                    try
                    {
                        var cmd1 = _config.GetSection("AppSettings:CreateMpQrcode:course-detail").Get<CreateMpQrcodeCmd>();
                        cmd1.Scene = cmd1.Scene.FormatWith(item.Id_s);
                        item.MpQrcode = (await _mediator.Send(cmd1)).MpQrcode;
                    }
                    catch { /* ignore */ }
                }

                // h5ToMpUrl
                try
                {
                    var cakey = CacheKeys.H5ToMpUrl.FormatWith(_config["AppSettings:CreateMpQrcode:course-detail:Page"], $"id={item.Id_s}");
                    item.H5ToMpUrl = await _redis.GetAsync(cakey);
                }
                catch { /* ignore */ }
                item.H5ToMpUrl = item.H5ToMpUrl.IsNullOrEmpty() ? null : item.H5ToMpUrl;
            }

            result.ErrIds = errIds;
            result.ErrSids = errSids;
            result.Courses = courses;
            return result;
        }



        /// <summary>
        /// 获取课程信息
        /// </summary>
        /// <returns></returns>
        private async Task GetCourses(GetCourseInfosForSchoolsQuery query, List<PcCourseItemDto3> courses, List<string> errSids, List<Guid> errIds)
        {
            for (var i = 0; i < (query.Ids?.Length ?? 0); i++)
            {
                try
                {
                    var course = await _mediator.Send(new CourseBaseInfoQuery { CourseId = query.Ids[i], AllowNotValid = true });
                    var org = await _mediator.Send(new OrgzBaseInfoQuery { OrgId = course.Orgid, AllowNotValid = true });
                    var dto = new PcCourseItemDto3
                    {
                        Id = course.Id,
                        Id_s = UrlShortIdUtil.Long2Base32(course.No),
                        Title = course.Title,
                        Subtitle = course.Subtitle,
                        Banner = course.Banner.IsNullOrEmpty() ? null : course.Banner?.ToObject<string[]>()?.FirstOrDefault(),
                        Price = course.Price,
                        OrigPrice = course.Origprice,
                        Sellcount = course.Sellcount ?? 0,
                        IsExplosions = course.IsExplosions ?? false,
                        Authentication = org.Authentication,
                        OrgName = org.Name,
                        IsOnline = org.IsValid && course.IsValid && org.Status == OrganizationStatusEnum.Ok.ToInt() && course.Status == CourseStatusEnum.Ok.ToInt(),
                    };
                    courses.Add(dto);
                    dto.Tags = OrderHelper.GetTagsFromCourse(course);
                }
                catch
                {
                    errIds.Add(query.Ids[i]);
                }
            }
            for (var i = 0; i < (query.SIds?.Length ?? 0); i++)
            {
                try
                {
                    var course = await _mediator.Send(new CourseBaseInfoQuery { No = UrlShortIdUtil.Base322Long(query.SIds[i]), AllowNotValid = true });
                    var org = await _mediator.Send(new OrgzBaseInfoQuery { OrgId = course.Orgid, AllowNotValid = true });
                    var dto = new PcCourseItemDto3
                    {
                        Id = course.Id,
                        Id_s = UrlShortIdUtil.Long2Base32(course.No),
                        Title = course.Title,
                        Subtitle = course.Subtitle,
                        Banner = course.Banner.IsNullOrEmpty() ? null : course.Banner?.ToObject<string[]>()?.FirstOrDefault(),
                        Price = course.Price,
                        OrigPrice = course.Origprice,
                        Sellcount = course.Sellcount ?? 0,
                        IsExplosions = course.IsExplosions ?? false,
                        Authentication = org.Authentication,
                        OrgName = org.Name,
                        IsOnline = org.IsValid && course.IsValid && org.Status == OrganizationStatusEnum.Ok.ToInt() && course.Status == CourseStatusEnum.Ok.ToInt(),
                    };
                    courses.Add(dto);
                    dto.Tags = new List<string>();
                    if (course.Minage != null && course.Maxage != null)
                    {
                        dto.Tags.Add($"{course.Minage}-{course.Maxage}岁");
                    }
                    else if (course.Minage != null && course.Maxage == null)
                    {
                        dto.Tags.Add($"大于{course.Minage}岁");
                    }
                    else if (course.Maxage != null && course.Minage == null)
                    {
                        dto.Tags.Add($"小于{course.Maxage}岁");
                    }
                    if (course.Subjects?.ToObject<string[]>() is string[] subjs && subjs.Length > 0)
                    {
                        dto.Tags.Add(EnumUtil.GetDesc(subjs[0].ToEnum<SubjectEnum>()));
                    }
                    if (course.Price <= 10)
                    {
                        dto.Tags.Add("低价体验");
                    }
                }
                catch
                {
                    errSids.Add(query.SIds[i]);
                }
            }
        }
    }
}
