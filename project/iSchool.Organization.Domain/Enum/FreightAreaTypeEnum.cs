using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 运费地区类型
    /// </summary>
    public enum FreightAreaTypeEnum
    {
        ///// <summary>未知</summary>
        //[Description("未知")]
        //None = -1,

        /// <summary>江浙沪</summary>
        [Description("江浙沪")]
        JZS = 1,
        /// <summary>偏远地区</summary>
        [Description("偏远地区")]
        RemoteAreas = 2,
        /// <summary>其他地区</summary>
        [Description("其他地区")]
        Other = 3,
        /// <summary>自定义</summary>
        [Description("自定义")]
        Custom = 10,
    }
}
