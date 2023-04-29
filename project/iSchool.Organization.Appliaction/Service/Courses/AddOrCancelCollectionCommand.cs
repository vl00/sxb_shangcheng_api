using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 收藏课程或取消
    /// </summary>
    public class AddOrCancelCollectionCommand:IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        ///// <summary>
        ///// 收藏/取消（true:收藏；false:取消）
        ///// </summary>
        //public bool AddOrCancel { get; set; }
    }

    /// <summary>
    /// 收藏课程或取消
    /// </summary>
    public class AddOrCancelCollectionRequest 
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

    }
}
