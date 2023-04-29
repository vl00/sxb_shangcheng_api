using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 用于当已登录用户访问某个评测时
    /// </summary>
    public class UserVisitEvaluationEvent : INotification
    {
        public Guid EvltId { get; set; }
        public Guid UserId { get; set; }
        public DateTime Now { get; set; }
    }
}
