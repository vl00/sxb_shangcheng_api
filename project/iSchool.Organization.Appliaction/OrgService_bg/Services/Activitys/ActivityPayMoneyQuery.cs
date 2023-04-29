using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
#nullable enable

    public class ActivityPayMoneyQuery : IRequest<ActivityPayMoneyQryResult>
    {
        public Guid ActivityId { get; set; }
    }

    public class ActivityPayMoneyQryResult
    {
        /// <summary>总预算</summary>
        public decimal Budget { get; set; }
        /// <summary>支出金额</summary>
        public decimal PayOutMoney { get; set; }
    }

#nullable disable
}
