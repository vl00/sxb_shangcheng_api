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
    /// 后台管理--课程订单列表
    /// </summary>
    public class SearchOrdersQuery : IRequest<PagedList<CoursesOrderItem>>
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
        /// 微信支付商户单号
        /// </summary>
        public string PayOrderNo { get; set; }

        /// <summary>
        /// 下单人手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 收货人手机号
        /// </summary>
        public string RecvMobile { get; set; }

        /// <summary>
        ///上课电话
        /// </summary>
        public string BeginClassMobile { get; set; }

        /// <summary>
        /// 科目
        /// </summary>
        public int? SubjectId { get; set; }

        /// <summary>
        /// 机构类型
        /// </summary>
        public int? OrgTypeId { get; set; }

        /// <summary>
        /// 供应商id
        /// </summary>
        public Guid? SupplierId { get; set; }
        /// <summary>
        /// 订单(发货)状态
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 商品分类(1==课程；2==好物)
        /// </summary>
        public int? CourseType { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 查询类型(0：返回json；1：返回视图，999:导出)
        /// </summary>
        public int SearchType { get; set; } = 1;
    }
}
