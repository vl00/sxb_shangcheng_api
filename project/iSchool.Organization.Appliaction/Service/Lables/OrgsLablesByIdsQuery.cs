using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Lables
{

    /// <summary>
    /// 【长ID集合】机构卡片列表
    /// </summary>
    public class OrgsLablesByIdsQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 机构长id集
        /// </summary>
        public List<Guid> LongIds { get; set; }

       
    }


    

}
