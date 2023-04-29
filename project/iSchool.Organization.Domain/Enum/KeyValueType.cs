using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// KeyValue表的Type枚举
    /// </summary>
    public enum KeyValueType
    {
        /// <summary>
        /// 机构分类
        /// </summary>
        [Description("机构分类")]
        OrgType = 0,

        /// <summary>
        /// 科目分类
        /// </summary>
        [Description("科目分类")]
        SubjectType = 1
    }
}
