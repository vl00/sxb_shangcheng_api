using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;

namespace iSchool.Organization.Appliaction.Mini.Services.Courses
{
    public class MiniActivityCourseListQuery : PageInfo, IRequest<ResponseResult>
    {
        /// <summary>
        /// 活动类型 0 新人专享  1限时优惠
        /// </summary>
        public int ActivityType { get; set; }
    }
}
