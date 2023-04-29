using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class SKUEnableRangeQueryHandler : IRequestHandler<SKUEnableRangeQuery, IEnumerable<SKUItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public SKUEnableRangeQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }
        public async Task<IEnumerable<SKUItem>> Handle(SKUEnableRangeQuery request, CancellationToken cancellationToken)
        {
            string sql = @"
 
 SELECT * INTO #Course FROM Course
 WHERE title LIKE @courseName AND IsValid = 1
 ORDER BY Course.id
 OFFSET @offset ROWS 
 FETCH NEXT @limit ROWS ONLY
 SELECT CourseGoods.ID Id,CourseGoods.Courseid CourseId,Course.[title] CourseName,CoursePropertyItem.Id PropId,CoursePropertyItem.[Name] PropName,CoursePropertyItem.Sort FROM  #Course Course 
  JOIN  CourseGoods ON Course.id = CourseGoods.Courseid
  JOIN CourseGoodsPropItem ON CourseGoodsPropItem.GoodsId = CourseGoods.Id
  JOIN CoursePropertyItem ON CoursePropertyItem.Id = CourseGoodsPropItem.PropItemId
 WHERE CourseGoods.IsValid = 1 AND  CoursePropertyItem.IsValid = 1
";

            var res = await _orgUnitOfWork.QueryAsync<dynamic>(sql, new { courseName = $"%{request.Text}%", offset = request.Offset, limit = request.Limit });
            return Map2SKUEnableRange(res);
        }


        IEnumerable<SKUItem> Map2SKUEnableRange(IEnumerable<dynamic> res)
        {
            return res.GroupBy(rg => rg.Id).Select(rs =>
               {
                   var first = rs.First();
                   var properties = rs.Select(rss => new PropertyItem()
                   {
                       PropId = rss.PropId,
                       Name = rss.PropName,
                       Sort = rss.Sort
                   }).OrderBy(po => po.Sort).ToList();
                   var model = new SKUItem()
                   {
                       Id = rs.Key,
                       CourseId = first.CourseId,
                       CourseName = first.CourseName,
                       Properties = properties
                   };
                   return model;
               }
              );
        }
    }
}
