using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 判断用户是否绑定手机
    /// </summary>
    public class CheckUserBindMobileCommand : IRequest<bool>
    {
        /// <summary>
        /// true=未绑定手机号时抛出异常 <br/>
        /// false=未绑定手机号时返回false
        /// </summary>
        public bool ThrowIfNoBind { get; set; } = false;
    }
}
