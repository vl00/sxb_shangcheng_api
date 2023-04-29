using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Activity.Appliaction.ResponseModels.Jobs
{
    /// <summary>
    /// 活动测评定期提醒实体
    /// </summary>
    public class ActivityEvltNoticeDto
    {
        /// <summary>
        /// 活动标题
        /// </summary>
        public string ActivityTitle { get; set; }

        /// <summary>
        /// 测评标题
        /// </summary>
        public string EvltTitle { get; set; }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户测评点赞数
        /// </summary>
        public int Likes { get; set; }

        /// <summary>
        /// 点赞排名
        /// </summary>
        public int Ranking { get; set; }

        /// <summary>
        /// 测评No
        /// </summary>
        public int EvaluationNo { get; set; }

        /// <summary>
        /// 测评Id
        /// </summary>
        public Guid EvaluationId { get; set; }
    }
}
