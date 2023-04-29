using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.EvaluationCrawler
{
    /// <summary>
    /// 抓取评测详情页-下拉框通用请求实体
    /// </summary>
    public class SelectItemsQuery:IRequest<List<SelectListItem>>
    {
        /// <summary>
        /// 主键
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// （1：机构；2：所有小专题；3：课程;4:某个大专题下的小专题;
        /// 5:未绑定活动的专题(指定活动绑定专题需返回) 6：所有上架的大专题;
        /// 7:未绑定大专题的所有小专题）
        /// 8:KeyValue表[ OtherCondition=KeyValue.type；type=1-课程科目分类、type=14-好物分类 ]
        /// 9:供应商
        /// 10:供应商的地址s
        /// </summary>
        public int Type { get; set; }

        /// <summary>指定活动Id</summary>
        public Guid ActivityId { get; set; } = default;

        /// <summary>
        /// 指定大专题下的小专题
        /// </summary>
        public Guid BigSpecialId { get; set; } = default;

        /// <summary>
        /// 附加条件(1-条件,1表示机构；)
        /// </summary>
        public string OtherCondition { get; set; }

        /// <summary>供应商id</summary>
        public Guid? SupplierId { get; set; }
    }
}
