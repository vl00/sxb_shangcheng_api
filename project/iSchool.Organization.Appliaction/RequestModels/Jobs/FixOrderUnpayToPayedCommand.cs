using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 用于修正当微信回调异常时导致课程订单状态未支付    
    /// </summary>
    public class FixCourseOrderUnpayToPayedCommand : IRequest
    {        
    }
}
