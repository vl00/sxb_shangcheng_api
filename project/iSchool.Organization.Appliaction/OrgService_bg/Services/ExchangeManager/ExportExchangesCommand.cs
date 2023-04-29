using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{
    /// <summary>
    /// 导出兑换记录
    /// </summary>
    public class ExportExchangesCommand : IRequest<string>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }
    }
}
