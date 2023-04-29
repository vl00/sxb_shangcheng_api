using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 退款+退货理由
    /// </summary>
    public enum RefundCauseEnum
    {
        /// <summary>不想要了</summary>
        [Description("不想要了")]
        C01 = 1,
        /// <summary>商品信息拍错（属性/颜色等）</summary>
        [Description("商品信息拍错（属性/颜色等）")]
        C02 = 2,
        /// <summary>地址/电话信息填写错误</summary>
        [Description("地址/电话信息填写错误")]
        C03 = 3,
        /// <summary>拍多了</summary>
        [Description("拍多了")]
        C04 = 4,
        /// <summary>协商一致退款</summary>
        [Description("协商一致退款")]
        C05 = 5,
        /// <summary>缺货</summary>
        [Description("缺货")]
        C06 = 6,
        /// <summary>发货速度不满意</summary>
        [Description("发货速度不满意")]
        C07 = 7,
        /// <summary>其他</summary>
        [Description("其他")]
        C08 = 8,


        /// <summary>颜色/尺寸/参数不符</summary>
        [Description("颜色/尺寸/参数不符")]
        C11 = 11,
        /// <summary>商品瑕疵</summary>
        [Description("商品瑕疵")]
        C12 = 12,
        /// <summary>质量问题</summary>
        [Description("质量问题")]
        C13 = 13,
        /// <summary>少件/漏发</summary>
        [Description("少件/漏发")]
        C14 = 14,
        /// <summary>其他</summary>
        [Description("其他")]
        C15 = 15,
    }
}
