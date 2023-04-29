using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 学校--机构列表请求model
    /// </summary>
    public class OrgsByIDsQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 机构ID集合
        /// </summary>
        public List<Guid> OrgIds { get; set; }
    }


    

}
