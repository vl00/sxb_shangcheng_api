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
    /// mini 结算时根据地址省市区查找商品的运费
    /// </summary>
    public class GetFreightsByRecvAddressQuery : IRequest<GetFreightsByRecvAddressQryResult>
    {
        /// <summary>省</summary> 
        [Required]
        public string Province { get; set; } = default!;
        /// <summary>市</summary> 
        public string? City { get; set; }
        /// <summary>区</summary> 
        public string? Area { get; set; }

        /// <summary>
        /// 商品spu的id<br/>
        /// 通常为结算info接口返回结果里 $.data.courseDtos[].id
        /// </summary>
        [Obsolete] //[Required]
        public Guid[]? CourseIds { get; set; }

        /// <summary>
        /// 商品sku的id<br/>
        /// 通常为结算info接口返回结果里 $.data.courseDtos[].goodsId
        /// </summary>
        [Required]
        public Guid[] SkuIds { get; set; } = default!;

        /// <summary>是否填充运费补全为0</summary>
        public bool AllowFillEmptyOrgsFreights { get; set; } = true;
    }

#nullable disable
}
