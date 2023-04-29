using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 后台管理--购买留资列表
    /// </summary>
    public class SearchCoursesOrderQuery : IRequest<PagedList<CoursesOrderItem>>
    {
        /// <summary>
        /// 机构
        /// </summary>
        public Guid? OrgId { get; set; }

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid? CourseId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrdCode { get; set; }

        /// <summary>
        /// 下单人手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 科目
        /// </summary>
        public int? SubjectId { get; set; }

        /// <summary>
        /// 机构类型
        /// </summary>
        public int? OrgTypeId { get; set; }

        /// <summary>
        /// 订单(发货)状态
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 查询类型(0：返回json；1：返回视图)
        /// </summary>
        public int SearchType { get; set; } = 1;
    }
}
