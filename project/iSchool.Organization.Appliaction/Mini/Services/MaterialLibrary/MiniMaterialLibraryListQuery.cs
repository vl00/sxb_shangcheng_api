using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;

namespace iSchool.Organization.Appliaction.Mini.Services.Courses
{
    public class MiniMaterialLibraryListQuery : PageInfo, IRequest<ResponseResult>
    {
        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string SearchText { get; set; }
    }
}
