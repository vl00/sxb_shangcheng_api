using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class RefundApplyCmdResult
    {
        /// <summary>退款单id</summary>
        public Guid Id { get; set; }
        /// <summary>退款单号</summary>
        public string Code { get; set; } = default!;
    }

#nullable disable
}
