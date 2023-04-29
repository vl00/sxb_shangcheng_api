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

    public class RefundApplyQry : IRequest<RefundApplyQryResult>
    {
        /// <summary>订单详情OrderDetial id</summary>
        public Guid OrderDetailId { get; set; }

        //public int RefundType { get; set; }

        public bool IsInLck { get; set; } = false;
    }

#nullable disable
}
