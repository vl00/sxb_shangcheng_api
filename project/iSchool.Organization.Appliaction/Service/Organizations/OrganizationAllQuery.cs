using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Organization
{

    public class OrganizationAllQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfo PageInfo { get; set; }        

        /// <summary>
        /// 品牌|课程名称(模糊查询)
        /// </summary>
        public string CourseOrOrgName { get; set; }        

        /// <summary>
        /// 机构类型
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 认证
        /// </summary>
        public bool? Authentication { get; set; }

        


    }


    

}
