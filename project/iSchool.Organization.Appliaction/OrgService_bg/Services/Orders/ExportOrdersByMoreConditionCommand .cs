using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{
    public class ExportOrdersByMoreConditionCommand : IRequest<string>
    {
        #region MyRegion
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
        /// 下单人电话
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 收货人电话
        /// </summary>
        public string RecvMobile { get; set; }

        /// <summary>
        /// 上课电话
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
        /// 订单(发货)状态
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }


        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        #endregion
    }
}
