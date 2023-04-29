using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 订单的快递详情
    /// </summary>
    public class OrderKuaidiDetailDto
    {
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>订单号</summary>
        public string OrderNo { get; set; } = default!;

        /// <summary>收货地址dto</summary>
        public RecvAddressDto RecvAddressDto { get; set; } = default!;

        /// <summary>运单号</summary>
        public string Nu { get; set; } = default!;
        /// <summary>
        /// 成功时,快递轨迹s<br/>
        /// 错误时为null.
        /// </summary>
        public IEnumerable<KuaidiNuDataItemDto>? Items { get; set; }
        /// <summary>true=已收货</summary>
        public bool IsCompleted { get; set; } = false;
        /// <summary>快递公司名</summary>
        public string? CompanyName { get; set; }
        /// <summary>快递公司code</summary>
        public string? CompanyCode { get; set; }

        /// <summary>小助手qrcode</summary>
        public string? HelperQrcodeUrl { get; set; }
    }






    public class KuaidiDetailDto
    {
        /// <summary>收货地址dto</summary>
        public RecvAddressDto RecvAddressDto { get; set; } = default!;

        /// <summary>运单号</summary>
        public string Nu { get; set; } = default!;

        /// <summary>
        /// 成功时,快递轨迹s<br/>
        /// 错误时为null.
        /// </summary>
        public IEnumerable<KuaidiNuDataItemDto>? Items { get; set; }
        /// <summary>true=已收货</summary>
        public bool IsCompleted { get; set; } = false;
        /// <summary>快递公司名</summary>
        public string? CompanyName { get; set; }
        /// <summary>快递公司code</summary>
        public string? CompanyCode { get; set; }

        /// <summary>小助手qrcode</summary>
        public string? HelperQrcodeUrl { get; set; }
    }


    public class KuaidiItemDto
    {

    }

#nullable disable
}
