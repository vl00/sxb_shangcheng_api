using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Modles
{
    /// <summary>
    /// 下拉框数据源实体
    /// </summary>
    public class OrgSelectItemsKeyValues
    {
        /// <summary>
        /// 键
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Sort { get; set; }
    }
}
