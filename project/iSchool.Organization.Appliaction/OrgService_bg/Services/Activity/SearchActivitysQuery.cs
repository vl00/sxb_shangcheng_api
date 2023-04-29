using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 查询活动列表-请求参数实体类
    /// </summary>
    public class SearchActivitysQuery : IRequest<PagedList<ActivityItem>>
    {
        /// <summary>活动状态(1:上架中；2：已下架) </summary>
        public int? Status { get; set; }

        /// <summary>关联专题 </summary>
        public Guid? SpecialId { get; set; } = default;

        /// <summary>关联专题(多选) </summary>
        public string SpecialIds { get; set; } 

        /// <summary>活动名称 </summary>
        public string Title { get; set; }

        /// <summary>页码</summary>
        public int PageIndex { get; set; }

        /// <summary>页大小</summary>
        public int PageSize { get; set; }

        /// <summary>查询类型(0：返回json；1：返回视图)</summary>
        public int SearchType { get; set; } = 1;

        /// <summary>活动链接 </summary>
        public string ActivityUrl { get; set; }
    }
}
