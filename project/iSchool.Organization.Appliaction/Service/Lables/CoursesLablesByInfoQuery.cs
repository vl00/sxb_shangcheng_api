using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 课程标签列表请求实体
    /// </summary>
    public class CoursesLablesByInfoQuery:IRequest<ResponseResult>
    {
       
        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 科目Id
        /// </summary>
        public int? SubjectId { get; set; }

        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfo PageInfo { get; set; }
    }
}
