using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    /// <summary>
    /// 查询首页数据
    /// </summary>
    public class MiniIndexDataQueryHandler : IRequestHandler<MiniIndexDataQuery, MiniIndexDataDto>
    {

        private readonly OrgUnitOfWork _unitOfWork;
        private readonly CSRedisClient _redis;


        public MiniIndexDataQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redis)
        {
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redis = redis;
        }



        public Task<MiniIndexDataDto> Handle(MiniIndexDataQuery request, CancellationToken cancellationToken)
        {
            var exists = _redis.Exists(CacheKeys.ExcellentCourses);
            var result = new MiniIndexDataDto();
            if (exists)
            {
                //如果存在则读缓存
                result.Courses = _redis.Get<List<MiniCourseItemDto>>(CacheKeys.ExcellentCourses);
            }
            else
            {
                var sqlWhere = $"course.IsValid=1 AND course.status=1 and course.type={CourseTypeEnum.Course.ToInt()} and course.IsInvisibleOnline=0 and org.authentication=1 ";
                //过滤掉新人专享和限时折扣
                sqlWhere += " and course.LimitedTimeOffer=0 and course.NewUserExclusive=0 ";
                //否则读库，再写缓存
                var sql = @$"SELECT  TOP {request.CoursePageSize} course.*,org.name AS orgname,org.authentication,org.logo
                            FROM dbo.Course AS course
                            LEFT JOIN dbo.Organization AS org ON  org.id=course.orgid
                            WHERE {sqlWhere}
                             ORDER BY course.IsExplosions desc,course.ModifyDateTime DESC,course.sellcount DESC";


                var list = _unitOfWork.Query<Course, string, bool, string, MiniCourseItemDto>(sql,
                      (course, orgname, authentication, logo) =>
                      {
                          var banners = course.Banner_s ?? course.Banner;
                          var dto = new MiniCourseItemDto()
                          {
                              Id = course.Id,
                              Id_s = UrlShortIdUtil.Long2Base32(course.No),
                              Name = course.Name,
                              Banner = string.IsNullOrEmpty(banners) ? new List<string>() :
                                    JsonSerializationHelper.JSONToObject<List<string>>(banners),
                              Title = course.Title,
                              Authentication = authentication,
                              Price = course.Price,
                              OrigPrice = course.Origprice,
                              OrgName = orgname,
                              Logo = logo,
                              Tags = new List<string>(),
                              LastOffShelfTime = course.LastOffShelfTime,
                          };

                          //年龄标签
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

                          //科目标签
                          //if (course.Subject != null)
                          //    dto.Tags.Add(EnumUtil.GetDesc((SubjectEnum)course.Subject.Value));


                  

                          //低价体验
                          if (course.Price != null && course.Price <= 10)
                              dto.Tags.Add("低价体验");
                          if (course.NewUserExclusive)
                              dto.Tags.Add("新人专享");
                          if (course.CanNewUserReward)
                              dto.Tags.Add("新人立返");
                          if (course.LimitedTimeOffer)
                              dto.Tags.Add("限时补贴");

                          return dto;
                      }, splitOn: "orgname,authentication,logo").ToList();

                _redis.Set(CacheKeys.ExcellentCourses, list, 30 * 60);
                result.Courses = list;
            }
            return Task.FromResult(result);
        }
    }
}
