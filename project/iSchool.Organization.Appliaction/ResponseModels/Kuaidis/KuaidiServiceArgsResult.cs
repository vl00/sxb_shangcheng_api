using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 
    /// </summary>
    public class KuaidiServiceArgsResult
    {
        public object? Result { get; set; }

        public T GetResult<T>() => (T)this.Result!;
    }

    /// <summary>
    /// 快递公司编码dto
    /// </summary>
    public class KdCompanyCodeDto
    {
        /// <summary>快递鸟编码.貌似这个比较标准?</summary>
        public string Code { get; set; } = default!;
        /// <summary>快递100编码.百度也是用这个</summary>
        public string Code100 { get; set; } = default!;
        /// <summary>快递公司名</summary>
        public string Com { get; set; } = default!;
        /// <summary>快递公司别名</summary>
        public string[]? ComAlias { get; set; }
    }

#nullable disable
}
