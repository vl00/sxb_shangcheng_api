using iSchool.Domain.Enum;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 根据查询条件获取好物的请求Model
    /// </summary>
    public class MiniGoodThingRecommendQuery : PageInfo, IRequest<ResponseResult>
    {
        /// <summary>
        /// 0 默认(精选) 1爆款 2同年龄
        /// </summary>
        public int Type { get; set; }


        /// <summary>
        /// CourseId
        /// </summary>
        public Guid? CourseId { get; set; } 
        /// <summary>
        /// 1课程 2好物
        /// </summary>
        public int CourseType { get; set; } = 2;

    }
}
