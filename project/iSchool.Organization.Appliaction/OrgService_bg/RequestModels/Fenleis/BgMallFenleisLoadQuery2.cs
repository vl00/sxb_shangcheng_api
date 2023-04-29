using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.RequestModels
{
    public class BgMallFenleisLoadQuery2 : IRequest<BgMallFenleisLoadQueryResult>
    {
        /// <summary>
        /// 分类code, 可以不传
        /// </summary>
        public int? Code { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Guid UserId { get; set; }
    }
}
