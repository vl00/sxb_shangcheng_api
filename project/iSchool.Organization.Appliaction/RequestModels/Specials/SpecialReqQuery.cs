using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 单个专题页
    /// </summary>
    public class SpecialReqQuery : IRequest<ResponseModels.SpecialResEntity>
    {
        /// <summary>
        /// 专题短id
        /// </summary>
        public long No { get; set; }
        /// <summary>
        /// 排序类型 1=最热 2=最新
        /// </summary>
        public int OrderBy { get; set; } = 1;
    }
}
