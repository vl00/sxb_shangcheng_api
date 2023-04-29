using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 发评测后添加投票
    /// </summary>
    public class AddVoteAfterEvaluationAddedCommand : IRequest<bool>
    {
        /// <summary>
        /// 评测id
        /// </summary>
        public Guid EvltId { get; set; }
        /// <summary>
        /// 投票标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 投票补充内容
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// 投票项s内容
        /// </summary>
        public string[] Items { get; set; }
        /// <summary>
        /// 投票类型 保留字段 1单选 2多选
        /// </summary>
        public int Type { get; set; } = 1;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
