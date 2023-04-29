using MediatR;
using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 用于后台服务自动确定取消退货申请
    /// 返回不成功的refund id
    /// </summary>
    public class RefundStatusInSendbackDoAutoExpiredCancelCmd : IRequest<Guid[]?>
    {
        public int Days { get; set; } = 7;
    }

#nullable disable
}
