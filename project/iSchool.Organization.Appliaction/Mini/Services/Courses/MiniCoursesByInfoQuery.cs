using iSchool.Domain.Enum;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 根据查询条件获取课程的请求Model
    /// </summary>
    public class MiniCoursesByInfoQuery : PageInfo, IRequest<ResponseResult>
    {
        /// <summary>
        /// 搜索的标题
        /// </summary>
        public string SearchText { get; set; }
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
 
        /// <summary>
        /// 排序
        /// </summary>

        public CourseFilterSortType Sort { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public CourseFilterCutomizeType Type { get; set; }
        /// <summary>
        /// 类型 1 课程 2 好物
        /// </summary>

        public int CourseType { get; set; } = 1;
        /// <summary>
        /// 好物的类别
        /// </summary>

        public int? GoodThingType { get; set; }

    }
}
