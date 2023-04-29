using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 【课程短Id集查询】课程标签列表请求实体
    /// </summary>
    public class CoursesLablesById_ssQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程短Id集
        /// </summary>
        public List<string> Id_ss { get; set; }
    }
}
