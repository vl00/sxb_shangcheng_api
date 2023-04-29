using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>学校pc首页推荐机构</summary>
    public class PcGetSubjRecommendOrgsQuery : IRequest<PcOrgIndexQueryResult>
    {
        /// <summary>页码</summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>页大小</summary>
        public int PageSize { get; set; } = 12;
        /// <summary>品牌类型</summary>
        public int Type { get; set; } = 1;
    }
}
#nullable disable
