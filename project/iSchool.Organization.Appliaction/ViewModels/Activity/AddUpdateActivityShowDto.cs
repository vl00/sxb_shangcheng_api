using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 新增/编辑活动页面展示实体
    /// </summary>
    public class AddUpdateActivityShowDto
    {
        /// <summary>活动Id</summary>
        public Guid Id { get; set; }
                
        /// <summary>活动名称 </summary>
        public string Title { get; set; }

        /// <summary>小专题集合 </summary>
        public List<SelectListItem> ListSpecials { get; set; }

        /// <summary>已选小专题Id集合 </summary>
        public List<Guid> ListOldSpecials { get; set; }

        /// <summary>单篇奖金 </summary>
        public decimal? Price { get; set; }

        /// <summary>第N篇额外奖金</summary>
        public Dictionary<int,decimal> NExtraBonus { get; set; }

        /// <summary>每日上限</summary>
        public int? Limit { get; set; }

        /// <summary>活动预算</summary>
        public decimal? Budget { get; set; }

        /// <summary>继续/停止活动(1:继续活动；2:停止活动；默认值是2)</summary>
        public int StopOrKeepActivity { get; set; } = 2;

        /// <summary>审核通过，用户N天内不允许修改(界面传入值<1,则入库为null) </summary>
        public int? NDaysNotAllowChange { get; set; } =null;

        /// <summary>开始时间 </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>结束时间 </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>活动Logo </summary>
        public string Logo { get; set; }

        /// <summary>是否新增(true:新增;false:编辑;) </summary>
        public bool IsAdd { get; set; } = false;
              
    }
}
