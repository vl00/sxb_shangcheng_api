using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// mp主题详情页
    /// </summary>
    public class MpMallThemeDetailQuery : IRequest<MpMallThemeDetailQryResult>
    {
        /// <summary>专题id</summary>
        public string? Spid { get; set; }
        /// <summary>主题id</summary>
        public string? Tid { get; set; }
    }

    /// <summary>
    /// pc主题详情页
    /// </summary>
    public class PcMallThemeDetailQuery : IRequest<PcMallThemeDetailQryResult>
    {
        /// <summary>主题id</summary>
        public string? Tid { get; set; }
    }

#nullable disable
}
