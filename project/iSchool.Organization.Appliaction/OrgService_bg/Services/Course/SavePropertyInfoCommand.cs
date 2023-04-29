using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 保存属性、选项、商品的信息及关系
    /// </summary>
    public class SavePropertyInfoCommand : IRequest<ResponseResult>
    {
        /// <summary>课程Id</summary>
        public Guid CourseId { get; set; }

        /// <summary>属性相关信息集合</summary>
        public List<PropertyAndItems> PropertyInfos { get; set; }

        ///// <summary>是否是新增课程（true:是；false:否）</summary>
        //public bool IsNewCourse { get; set; } = false;
    }
}
