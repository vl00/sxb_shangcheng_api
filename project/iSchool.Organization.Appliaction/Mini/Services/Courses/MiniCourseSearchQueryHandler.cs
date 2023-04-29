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

namespace iSchool.Organization.Appliaction.Service
{
    public class MiniCourseSearchQueryHandler : IRequestHandler<MiniCourseSearchQuery, List<MiniCourseItemDto>>
    {
        private readonly OrgUnitOfWork _unitOfWork;

        public MiniCourseSearchQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<List<MiniCourseItemDto>> Handle(MiniCourseSearchQuery request, CancellationToken cancellationToken)
        {
            var sql = @$"SELECT   course.*,org.name AS orgname,org.authentication,org.logo
                            FROM dbo.Course AS course
                            LEFT JOIN dbo.Organization AS org ON  org.id=course.orgid
                            WHERE course.IsValid=1 AND course.status=1
							AND course.id IN @ids
                            ORDER BY CHARINDEX(','+ltrim(course.id)+',',@index)";


            var list = _unitOfWork.Query<Domain.Course, string, bool, string, MiniCourseItemDto>(sql,
                  (course, orgname, authentication, logo) =>
                  {
                      var banners = course.Banner_s ?? course.Banner;
                      var dto = new MiniCourseItemDto()
                      {
                          Id = course.Id,
                          Id_s = UrlShortIdUtil.Long2Base32(course.No),
                          Name = course.Name,
                          Banner = string.IsNullOrEmpty(banners) ? new List<string>() : JsonSerializationHelper.JSONToObject<List<string>>(banners),
                          Title = course.Title,
                          Authentication = authentication,
                          Price = course.Price,
                          OrigPrice = course.Origprice,
                          OrgName = orgname,
                          Logo = logo,
                          Tags = new List<string>()
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

                      return dto;
                  }, new
                  {
                      ids = request.Ids,
                      index = $",{string.Join(',', request.Ids)},"
                  }, splitOn: "orgname,authentication,logo").ToList();
            return Task.FromResult(list);
        }
    }
}
