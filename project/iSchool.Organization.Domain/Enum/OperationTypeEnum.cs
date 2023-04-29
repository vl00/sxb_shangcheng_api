using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public enum OperationTypeEnum
    {
        /// <summary>新增</summary>        
        Add = 1,

        /// <summary>更新</summary>
        Update = 2,

        /// <summary>删除</summary>        
        Del = 3,

        /// <summary>其他(非操作))|查询</summary>
        Query = 4,
    }
}
