using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    public class WxPayRequest : IRequest<WxPayResponse>
    {
        /// <inheritdoc cref="ApiWxAddPayOrderRequest"/>
        public ApiWxAddPayOrderRequest? AddPayOrderRequest { get; set; }
        /// <inheritdoc cref="WxPayCallbackNotifyMessage"/>
        public WxPayCallbackNotifyMessage? WxPayCallback { get; set; }
    }

#nullable disable
}
