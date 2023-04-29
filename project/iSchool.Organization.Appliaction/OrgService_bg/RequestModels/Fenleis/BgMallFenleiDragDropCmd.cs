using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.RequestModels
{
    public class BgMallFenleiDragDropCmd : IRequest<BgMallFenleiDragDropCmdResult>
    {
        /// <summary>源code</summary>
        public int Scode { get; set; }
        /// <summary>源排序号</summary>
        public int Ssort { get; set; }

        /// <summary>目标code</summary>
        public int Tcode { get; set; }
        /// <summary>
        /// 目标方向 1=目标前面 2=目标后面
        /// </summary>
        public int Tdirection { get; set; }
        /// <summary>目标排序号</summary>
        public int Tsort { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Guid UserId { get; set; }
    }
}
