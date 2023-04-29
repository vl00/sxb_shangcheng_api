using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 机构状态枚举
    /// </summary>
    public enum OrganizationStatusEnum
    {
        ///// <summary>
        ///// 用户刚创建,相当于待审核.
        ///// </summary>
        //[Description("刚创建")]
        //Inited = 0,
        /// <summary>
        /// 正常|审核成功|上架
        /// </summary>
        [Description("正常")]
        Ok = 1,
        /// <summary>
        /// 失败|审核失败|下架
        /// </summary>
        [Description("失败")]
        Fail = 0,
    }
}
