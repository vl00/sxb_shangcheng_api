using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 
    /// </summary>
    public class ScanLckCommand : IRequest<string>
    {
        /// <summary>redis key 模糊模式</summary>
        public string K { get; set; } = default!; //= @"org:lck:*:*";
        /// <summary>
        /// 从<see ref="K"/>中匹配出ttl组的正则<br/>
        /// 正则需要指定名称组 (?&lt;ttl&gt;) 和 (?&lt;exp&gt;)
        /// </summary>
        public string Rgx { get; set; } = default!; //= @"^([^:]+:lck:(?<ttl>\d+)-(?<exp>\d+):).+";

        public int? Sec { get; set; }
    }

#nullable disable
}
