using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg
{
 
    public class SaveActivityCommand : IRequest<ResponseResult>
    {
        /// <summary>活动Id</summary>
        public Guid Id { get; set; }

        /// <summary>活动名称 </summary>
        public string Title { get; set; }

        /// <summary>小专题集合 </summary>
        public string ListSpecials { get; set; }

        /// <summary>单篇奖金 </summary>
        public decimal Price { get; set; }

        #region 第N篇额外奖金
        /// <summary>第N篇集合</summary>
        public List<int> NExtraBonusNum { get; set; }

        /// <summary>第N篇额外奖金</summary>
        public List<decimal> NExtraBonusPrice { get; set; }
        #endregion

        /// <summary>每日上限</summary>
        public int? Limit { get; set; } = null;

        /// <summary>活动预算</summary>
        public decimal Budget { get; set; }

        /// <summary>继续/停止活动(1:继续活动；2:停止活动；默认值是2)</summary>
        public int StopOrKeepActivity { get; set; } = 2;

        /// <summary>审核通过，用户N天内不允许修改(界面传入值<1,则入库为null) </summary>
        public int? NDaysNotAllowChange { get; set; } = null;

        /// <summary>开始时间 </summary>
        public DateTime StartTime { get; set; }

        /// <summary>结束时间 </summary>
        public DateTime EndTime { get; set; }

        /// <summary>活动Logo </summary>
        public string Logo { get; set; }

        /// <summary>是否新增(true:新增;false:编辑;) </summary>
        public bool IsAdd { get; set; } = false;

        /// <summary>操作者</summary>
        public Guid UserId { get; set; }

        /// <summary>活动码(TODO，调全民营销的api) </summary>
        public string ACode { get; set; } = "TODO";

    }
}
