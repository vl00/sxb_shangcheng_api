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
    /// pc首页列表
    /// </summary>
    public class PcEvaluationIndexQuery : IRequest<PcEvaluationIndexQueryResult>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 多选用“,”隔开<br/>
        /// 每项不是数字就不查科目
        /// </summary>
        public string? Subj { get; set; }
        /// <summary>
        /// 是否只要精华.<br/>
        /// 1=只要精华<br/>
        /// 0=全部
        /// </summary>
        public int Stick { get; set; } = 0;
        public string? Age { get; set; }
        /// <summary>机构no</summary>
        public long? OrgNo { get; set; }

        /// <summary>
        /// 表示科目栏是否显示全部.<br/>
        /// 例如从某个详情页里的相关评测跳转过来.
        /// 0(或默认)=不显示 <br/>
        /// 1=显示
        /// </summary>
        public int R { get; set; } = 0;
    }
}
#nullable disable
