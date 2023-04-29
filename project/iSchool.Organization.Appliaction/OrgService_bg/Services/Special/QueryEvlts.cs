using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 查询所有上架评测
    /// </summary>
    public class QueryEvlts : IRequest<PagedList<SpecialEvlts>>
    {
        /// <summary>
        /// 当前专题Id
        /// </summary>
        public Guid SpecialId { get; set; }

        /// <summary>科目</summary>
        public int? Subject { get; set; }

        /// <summary>评测标题</summary>
        public string Title { get; set; }

        /// <summary>作者Id</summary>
        public Guid? UserId { get; set; }

        /// <summary>页码</summary>
        public int PageIndex { get; set; }

        /// <summary>页大小</summary>
        public int PageSize { get; set; }

        /// <summary>查询类型(0：返回json；1：返回视图)</summary>
        public int SearchType { get; set; } = 1;

        /// <summary>记录评测关联专题或取消关联</summary>
        public string oldCheckedList { get; set; }
    }
}
