using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 根据查询条件获取课程的请求Model
    /// </summary>
    public class CoursesByInfoQuery:IRequest<ResponseResult>
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfo PageInfo { get; set; }      

        /// <summary>
        /// 科目Id
        /// </summary>
        public int? SubjectId { get; set; }

        ///// <summary>
        ///// 品牌Id
        ///// </summary>
        //public int? BrandId { get; set; }

        /// <summary>
        /// 年龄段Id
        /// </summary>
        public int? AgeGroupId { get; set; }

        ///// <summary>
        ///// 认证（1：认证；0：未认证）
        ///// </summary>
        public int? isAuth { get; set; }

    }
}
