using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 机构列表
    /// </summary>
    public class PcOrgIndexQuery : IRequest<PcOrgIndexQueryResult>
    {
        /// <summary>页码</summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>页大小</summary>
        public int PageSize { get; set; }
        /// <summary>品牌类型</summary>
        public int? Type { get; set; }
        /// <summary>品牌认证(展示所有认证的品牌)</summary>
        public bool? Authentication { get; set; }

        //public string? CourseOrOrgName { get; set; } //品牌|课程名称(模糊查询)
    }
}
#nullable disable
