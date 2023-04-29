using iSchool.Organization.Appliaction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable    

    public class CourseGoodsSettleInfoQryResult
    {        
        /// <inheritdoc cref="CourseOrderProdItemDto"/>
        public CourseOrderProdItemDto CourseDto { get; set; } = default!;

        public string? Qrcode { get; set; }

        /// <inheritdoc cref="RecvAddressDto"/>
        public RecvAddressDto? AddressDto { get; set; }

    }

    public class CourseMultiGoodsSettleInfosQryResult
    {        
        /// <inheritdoc cref="CourseOrderProdItemDto"/>
        public CourseOrderProdItemDto[] CourseDtos { get; set; } = default!;

        public string? Qrcode { get; set; }

        /// <inheritdoc cref="RecvAddressDto"/>
        public RecvAddressDto? AddressDto { get; set; }

        /// <summary>
        /// 用户商城积分余额
        /// </summary>
        public int UserPoints { get; set; }
    }

#nullable disable
}
