using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 查询机构列表-请求参数实体类
    /// </summary>
    public class SearchOrgListQuery:IRequest<OrgListDto>
    {
        /// <summary>
        /// 年龄段
        /// </summary>
        public int? AgeGroup { get; set; }

        /// <summary>
        /// 教学模式/上课方式
        /// </summary>
        public int? TeachMode { get; set; }

        /// <summary>
        /// 是否合作
        /// </summary>
        public bool? Authentication { get; set; }

        /// <summary>
        /// 机构名称（模糊查询）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }
    }
}
