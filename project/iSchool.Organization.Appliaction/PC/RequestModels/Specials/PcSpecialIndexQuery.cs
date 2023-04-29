using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 单个专题页
    /// </summary>
    public class PcSpecialIndexQuery : IRequest<PcSpecialIndexQueryResult>
    {
        /// <summary>专题短id</summary>
        public long No { get; set; }
        /// <summary>排序类型 1=最热 2=最新</summary>
        public int OrderBy { get; set; } = 1;
        /// <summary>页码</summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>页大小</summary>
        public int PageSize { get; set; }
    }
}
#nullable disable
