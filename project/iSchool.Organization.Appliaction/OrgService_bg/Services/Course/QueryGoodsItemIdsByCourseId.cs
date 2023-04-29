using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 获取 商品-选项Id集合的列表
    /// </summary>
    public class QueryGoodsItemIdsByCourseId : IRequest<List<GoodsItemIds>>
    {
        /// <summary>课程Id </summary>
        public Guid CourseId { get; set; }
    }
}
