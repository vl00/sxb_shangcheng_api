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
    /// 查询专题列表-请求参数实体类
    /// </summary>
    public class SearchSpecialsQuery:IRequest<PagedList<SpecialItem>>
    {
        /// <summary>
        /// 专题名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 分享标题
        /// </summary>
        public string ShareTitle { get; set; }

        /// <summary>
        /// 分享副标题
        /// </summary>
        public string ShareSubTitle { get; set; }

        /// <summary>
        /// 专题状态
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 查询类型(0：返回json；1：返回视图)
        /// </summary>
        public int SearchType { get; set; } = 1;

        /// <summary>
        /// 专题baseurl
        /// </summary>
        public string SpecialBaseUrl { get; set; }
    }
}
