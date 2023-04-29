using iSchool.Domain.Enum;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 根据查询条件获取课程的请求Model, 多选查询条件
    /// </summary>
    public class MiniCoursesByInfoMutiFilterQuery : PageInfo, IRequest<ResponseResult>
    {
        /// <summary>
        /// 搜索的标题
        /// </summary>
        public string SearchText { get; set; }
        /// <summary>
        /// 商品分类Ids
        /// </summary>
        public List<int> CatogroyIds { get; set; } = null;



        /// <summary>
        /// 年龄段Id
        /// </summary>
        public List<int> AgeGroupId { get; set; } = null;

        /// <summary>
        /// 排序
        /// </summary>

        public CourseFilterSortType Sort { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public List<int> Types { get; set; } = null;
        /// <summary>
        /// 类型 1 课程 2 好物
        /// </summary>

        public int CourseType { get; set; } = 0;
        /// <summary>
        /// 价格区间
        /// </summary>
        public decimal PriceMin { get; set; }
        public decimal PriceMax { get; set; }


    }
}
