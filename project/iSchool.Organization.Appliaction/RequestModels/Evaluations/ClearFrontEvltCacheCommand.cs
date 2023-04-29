using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 前端清除单个评测缓存
    /// </summary>
    public class ClearFrontEvltCacheCommand : IRequest
    {
        /// <summary>评测id</summary>
        public Guid EvltId { get; set; }
        /// <summary>专题id</summary>
        public Guid? SpclId { get; set; }
    }

#nullable disable
}
