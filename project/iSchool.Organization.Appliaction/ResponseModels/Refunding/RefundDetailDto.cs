using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class RefundDetailDto
    {
        /// <summary>退款单id</summary>
        public Guid Id { get; set; }
        /// <summary>退款单号</summary>
        public string Code { get; set; } = default!;

        /// <summary></summary>
        public CourseOrderProdItemDto Item { get; set; } = default!;

        /// <summary>退款类型</summary>
        public int RefundType { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal RefundMoney { get; set; } //=> Item?.PricesAll ?? 0;

        /// <summary>申请时间</summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间 -- 退款成功时间|审核不通过时间
        /// </summary>
        public DateTime UpTime { get; set; }

        /// <summary>refundType=1时使用此字段</summary>
        public RefundDetailDto_Rty1? Rty1 { get; set; }
        /// <summary>refundType=2时使用此字段</summary>
        public RefundDetailDto_Rty2? Rty2 { get; set; }
        /// <summary>refundType=3时使用此字段</summary>
        public RefundDetailDto_Rty3? Rty3 { get; set; }
        /// <summary>refundType=4时使用此字段</summary>
        public RefundDetailDto_Rty4? Rty4 { get; set; }

        /// <summary>二维码</summary>
        public string? Qrcode { get; set; }
    }

    public class RefundDetailDto_Rty3
    {
        /// <summary>
        /// 5.退款成功 
        /// </summary>
        public int Status { get; set; } = (int)RefundStatusEnum.RefundSuccess;
    }
    public class RefundDetailDto_Rty4
    {
        /// <summary>
        /// 5.退款成功 
        /// </summary>
        public int Status { get; set; } = (int)RefundStatusEnum.RefundSuccess;
    }

    public class RefundDetailDto_Rty1
    {
        /// <summary>退款理由(数值)</summary>
        public int Cause { get; set; }
        /// <summary>退款理由(文字)</summary>
        public string CauseDesc { get; set; } = default!;
        /// <summary>
        /// 1. 提交申请  2.平台审核(发货) 3.平台审核(未发货)   4.平台退款  5.退款成功  6.审核失败 <br/>
        /// 20.(用户主动)取消申请  21.因过期而取消申请
        /// </summary>
        public int Status { get; set; }
        /// <summary>审核不通过|取消原因</summary>
        public string? NotOkReason { get; set; }
    }

    public class RefundDetailDto_Rty2
    { 
        /// <summary>退款理由(数值)</summary>
        public int Cause { get; set; }
        /// <summary>退款理由(文字)</summary>
        public string CauseDesc { get; set; } = default!;
        /// <summary>
        /// 11.提交申请   12.平台审核   13.审核失败   14.寄回商品  15平台收货  16.验货失败   17.退款成功 <br/>
        /// 20.(用户主动)取消申请  21.因过期而取消申请
        /// </summary>
        public int Status { get; set; }
        /// <summary>审核不通过|取消原因</summary>
        public string? NotOkReason { get; set; }

        /// <summary>寄回地址dto</summary>
        public RecvAddressDto? AddressDto { get; set; }

        /// <summary>最新物流记录.可null.</summary>
        public string? LastExpressDesc { get; set; }
        /// <summary>最新物流时间.可null.</summary>
        public DateTime? LastExpressTime { get; set; }
        /// <summary>快递公司.可null.</summary>
        public string? ExpressCompanyName { get; set; }
        /// <summary>快递单号.可null.</summary>
        public string? ExpressNu { get; set; }
    }

#nullable disable
}
