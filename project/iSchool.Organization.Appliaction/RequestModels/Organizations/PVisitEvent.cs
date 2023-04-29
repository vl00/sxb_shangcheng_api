using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 用于当已登录用户访问某个content时
    /// </summary>
    public class PVisitEvent : INotification
    {
        public PVisitCttTypeEnum CttType { get; set; } = PVisitCttTypeEnum.Evaluation;
        public Guid CttId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime Now { get; set; } = DateTime.Now;
    }
}
