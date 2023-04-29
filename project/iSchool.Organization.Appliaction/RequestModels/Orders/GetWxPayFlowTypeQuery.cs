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
    /// 决定wx支付走的方式(是原支付还是新的跳转支付)
    /// </summary>
    public class GetWxPayFlowTypeQuery : IRequest<GetWxPayFlowTypeQryResult>
    { 
    }

#nullable disable
}
