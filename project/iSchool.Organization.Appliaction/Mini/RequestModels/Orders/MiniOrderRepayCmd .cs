using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable
    /// <summary>
    /// mini 订单重新支付
    /// </summary>
    public class MiniOrderRepayCmd : IRequest<Res2Result<CourseWxCreateOrderCmdResult_v4>>
    {
        /// <summary>
        /// (预)订单ID
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>openid</summary>
        public string? OpenId { get; set; } = null!;
        /// <summary>
        /// 0=h5jsapi <br/>
        /// 1=小程序 <br/>
        /// 2=h5支付 <br/>
        /// </summary>
        public int IsWechatMiniProgram { get; set; } = 0;
        /// <summary>
        /// appid主要用于小程序支付, 其他情况不管或传null
        /// </summary>
        public string? AppId { get; set; }

        /// <summary>
        /// 新的小程序支付时不用判断当前userid==order.Userid
        /// </summary>
        [JsonIgnore]
        public bool IsNewMpPay { get; set; } = false;
    }


#nullable disable
}
