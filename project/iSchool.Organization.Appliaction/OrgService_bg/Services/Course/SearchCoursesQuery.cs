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
    /// 后台管理--课程列表
    /// </summary>
    public class SearchCoursesQuery : IRequest<PagedList<CoursesItem>>
    {
        /// <summary>
        /// 机构
        /// </summary>
        public Guid? OrgId { get; set; }

        /// <summary>
        /// 供应商ID
        /// </summary>
        public Guid? SupplierId { get; set; }

        /// <summary>
        /// 科目
        /// </summary>
        public int? SubjectId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 商品分类
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 课程标题
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
