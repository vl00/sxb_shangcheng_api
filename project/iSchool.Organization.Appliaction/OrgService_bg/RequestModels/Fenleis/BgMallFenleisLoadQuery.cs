using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.RequestModels
{
    /// <summary>
    /// 加载常规分类页面
    /// </summary>
    public class BgMallFenleisLoadQuery : IRequest<BgMallFenleisLoadQueryResult>
    {
        /// <summary>
        /// 分类code, 可以不传
        /// </summary>
        public int? Code { get; set; }

        /// <summary>
        /// 1=只返回直接下级 <br/>
        /// 2=返回该节点的所有级联节点s和每个上下级其同级的其他项s.其中,每个下级都取第1个加载之后的下级.
        /// </summary>
        public int ExpandMode { get; set; }

        [Newtonsoft.Json.JsonIgnore] 
        public Guid UserId { get; set; }
    }
}
