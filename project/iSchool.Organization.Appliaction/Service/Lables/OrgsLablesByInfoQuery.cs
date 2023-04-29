using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Lables
{

    public class OrgsLablesByInfoQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 机构id
        /// </summary>
        public Guid OrgId { get; set; }

        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfo PageInfo { get; set; }        

        /// <summary>
        ///机构名称
        /// </summary>
        public string OrgName { get; set; }        

        /// <summary>
        /// 机构类型
        /// </summary>
        public int? Type { get; set; }
             

    }


    

}
