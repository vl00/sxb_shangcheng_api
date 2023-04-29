using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 用于检查用户每日活动上限和账号是否异常
    /// </summary>
    public class UserHd2ActiQuery : IRequest<UserHd2ActiQueryResult>
    {    
        /// <inheritdoc cref="UserHd2ActiQuery"/>
        public UserHd2ActiQuery() { }

        public Guid ActivityId { get; set; }
        public Guid UserId { get; set; }        
        public Guid[]? OtherUserIds { get; set; }
        public DateTime? Now { get; set; }
    }

#nullable disable
}
