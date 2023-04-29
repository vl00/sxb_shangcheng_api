using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 快递接口结果来源
    /// </summary>
    public enum KuaidiApiResultSrcTypeEnum
    {
        /// <summary>百度</summary>
        [Description("百度")]
        Baidu = 1,
        /// <summary>腾讯云-17972-全国物流快递查询</summary>
        [Description("腾讯云-17972-全国物流快递查询")]
        Txc17972 = 2,
        /// <summary>腾讯云-20590-快递鸟</summary>
        [Description("腾讯云-20590-快递鸟")]
        TxcKdniao = 3,
    }
}
