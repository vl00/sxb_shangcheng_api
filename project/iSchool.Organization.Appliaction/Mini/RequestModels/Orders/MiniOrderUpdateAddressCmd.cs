using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable
    /// <summary>
    /// mini 订单修改地址
    /// </summary>
    public class MiniOrderUpdateAddressCmd : IRequest<bool>
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        //[Required]
        //public Guid OrderId { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        [Required]
        public string AdvanceOrderNo { get; set; } = default!;
        /// <summary>地址.必须传</summary>
        [Required]
        public RecvAddressDto AddressDto { get; set; } = default!;
    }


#nullable disable
}
