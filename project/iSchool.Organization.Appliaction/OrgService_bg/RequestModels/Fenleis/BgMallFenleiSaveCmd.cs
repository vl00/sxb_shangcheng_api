using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.RequestModels
{
    public class BgMallFenleiSaveCmd : IRequest<BgMallFenleiSaveCmdResult>
    { 
        public string Name { get; set; }
        /// <summary>图</summary>
        public string Img { get; set; }

        /// <summary>
        /// 新增不传, 修改必传
        /// </summary>
        public int? Code { get; set; }
        /// <summary>
        /// 父code <br/>
        /// 新增必传, 修改不传
        /// </summary>
        public int? Pcode { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Guid UserId { get; set; }
    }
}
