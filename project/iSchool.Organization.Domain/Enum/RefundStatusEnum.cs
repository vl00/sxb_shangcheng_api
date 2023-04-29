using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{


    //1. 提交申请  2.平台审核(发货) 3.平台审核(未发货)   4.平台退款  5.退款成功  6.审核失败
    //11.提交申请   12.平台审核   13.审核失败   14.寄回商品  15平台收货  16.验货失败   17.退款成功
    public enum RefundStatusEnum
    {
        /// <summary>
        /// 提交申请
        /// </summary>
        RefundApply = 1,

        /// <summary>
        /// 平台审核(发货)
        /// </summary>
        RefundAudit1 = 2,

        /// <summary>
        /// 平台审核(未发货)
        /// </summary>
        RefundAudit2 = 3,

        /// <summary>
        /// 平台退款
        /// </summary>
        Refunding = 4,

        /// <summary>
        /// 退款成功
        /// </summary>
        RefundSuccess = 5,

        /// <summary>
        /// 审核失败
        /// </summary>
        RefundAuditFailed = 6,



        /// <summary>
        /// 提交申请
        /// </summary>
        ReturndApply = 11,

        /// <summary>
        /// 平台审核
        /// </summary>
        ReturnAudit = 12,

        /// <summary>
        /// 审核失败
        /// </summary>
        ReturnAuditFailed = 13,

        /// <summary>
        /// 寄回商品
        /// </summary>
        SendBack = 14,
        /// <summary>
        /// 平台收货
        /// </summary>
        Receiving = 15,

        /// <summary>
        /// 验货失败
        /// </summary>
        InspectionFailed = 16,
        /// <summary>
        /// 退款成功
        /// </summary>
        ReturnSuccess = 17,


        /// <summary>
        /// (用户主动)取消申请
        /// </summary>
        Cancel = 20,
        /// <summary>
        /// 因过期而取消申请
        /// </summary>
        CancelByExpired = 21,
    }
}
