using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
#nullable enable

    /// <summary>
    /// 活动信息
    /// </summary>
    [Obsolete]
    public class ActivityInfo
    {
        /// <summary>原code</summary>
        public string? OriginCode { get; set; }
        /// <summary>活动id</summary>
        public Guid ActivityId { get; set; }
        /// <summary>resolved活动码</summary>
        public string? Acode { get; set; } = default!;
        /// <summary>resolved推广码</summary>
        public string? Promocode { get; set; }
        /// <summary>resolved推广编号</summary>
        public string? PromoNo { get; set; }

        /// <summary>是否包含推广信息</summary>
        public bool IsHasPromo => !string.IsNullOrEmpty(PromoNo);
    }

#nullable disable
}
