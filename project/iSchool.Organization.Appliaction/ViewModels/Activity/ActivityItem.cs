using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 活动返回实体
    /// </summary>
    public class ActivityItem
    {
        /// <summary>活动Id</summary>
        public Guid Id { get; set; }

        /// <summary>序号</summary>
        public int RowNum { get; set; }

        /// <summary>活动名称(标题) </summary>
        public string Title { get; set; }

        /// <summary>关联专题 </summary>
        public string SpecialTitles { get; set; }

        /// <summary>单篇奖金 </summary>
        public decimal? Price { get; set; }

        /// <summary>活动开始时间 </summary>
        public string StartTime { get; set; }

        /// <summary>活动结束时间 </summary>
        public string EndTime { get; set; }

        /// <summary>支出金额 </summary>
        public decimal? ExpenditureAmount { get; set; }

        /// <summary>剩余金额</summary>
        public decimal? RemainingAmount { get; set; }

        /// <summary>活动预算</summary>
        public decimal? Budget { get; set; }

        /// <summary>活动状态（1:上架;2:下架;null:无状态） </summary>
        public int? Status { get; set; }

        /// <summary>活动产生评测数量 </summary>
        public int ActEvltCount { get; set; } = 0;

        /// <summary>活动链接 </summary>
        public string ActivityUrl { get; set; }

        /// <summary>活动码 </summary>
        public string ACode { get; set; }
    }
}
