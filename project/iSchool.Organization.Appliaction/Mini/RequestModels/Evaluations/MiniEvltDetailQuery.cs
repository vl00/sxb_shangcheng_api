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
    /// mini种草详情
    /// </summary>
    public class MiniEvltDetailQuery : IRequest<MiniEvltDetailDto>
    {
        public Guid Id { get; set; }
        public long No { get; set; }

        public bool AllowRecordPV { get; set; } = true;

        /// <summary>
        /// 1=ios会屏蔽网课 <br/>
        /// 0=ios不会屏蔽网课
        /// </summary>
        public int AllowIosNodisplay { get; set; } = 1;
    }
}
#nullable disable
