using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.RequestModels
{
    public class BgModifyHotMallFenleiCmd : IRequest<object>
    {
        /// <summary>第几个.从1开始算起</summary>
        public int Index { get; set; }
        /// <summary>第3级的code</summary>
        public int D3 { get; set; }
        /// <summary>
        /// true=删除,删除时可以不用传d3
        /// false=编辑
        /// </summary>
        public bool IsDeleted { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Guid UserId { get; set; }
    }
}
