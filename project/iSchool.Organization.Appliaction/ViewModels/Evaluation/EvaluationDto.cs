using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
   
    /// <summary>
    /// 评测详情--返回实体
    /// </summary>
    public class EvaluationDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId  { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid Id { get; set; }
       
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 图片（json格式）
        /// </summary>
        public string Pictures { get; set; }

        /// <summary>视频</summary> 
        public string Video { get; set; }
        /// <summary>视频封面</summary> 
        public string VideoCover { get; set; }

        /// <summary>
        /// 是否官方
        /// </summary>
        public bool IsOfficial { get; set; }

        /// <summary>
        /// 标题或内容相同
        /// </summary>
        public bool IsSame_TitleOrContent { get; set; } = false;

        public double Row { get; set; } = 4;

        /// <summary>修改评测次数</summary>
        public int ModifyCount { get; set; } = 0;

        /// <summary>
        /// 是否展示审批通过按钮
        /// 1、已审批通过，则不展示--false
        /// 2、品牌下奖励机会数>0 && 审批状态!=审批通过--true
        /// </summary>
        public bool IsShowPassBtn { get; set; } = false;

        /// <summary>
        /// 审核状态
        /// </summary>
        public int? AuditStatus { get; set; }

        public EvaluationReward _EvaluationReward { get; set; }

        /// <summary>支付时间</summary>
        public DateTime? OrderPayTime { get; set; }

        public bool? HasVideo { get; set; }
        public bool? IsPlainText { get; set; }

        /// <summary>
        /// 11月10元种草机会
        /// </summary>
        public int TenYuanTotalChance { get; set; }
        /// <summary>
        /// 11月剩余10元种草奖励机会
        /// </summary>
        public int TenYuanRemainChance { get; set; }
        /// <summary>
        /// 11月已用10元种草奖励机会
        /// </summary>
        public int TenYuanUsedChance { get; set; }
        /// <summary>
        /// 同一SPU种草奖励通过次数
        /// </summary>
        public int SpuRecordCount { get; set; }
    }

    /// <summary>
    /// 评测详情--返回实体
    /// </summary>
    public class EvaluationDB
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 图片（json格式）
        /// </summary>
        public string Pictures { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public int? AuditStatus { get; set; }

        public int? ModifyCount { get; set; }

        /// <summary>视频</summary> 
        public string Video { get; set; }
        /// <summary>视频封面</summary> 
        public string VideoCover { get; set; }

        public bool HasVideo { get; set; }
        public bool IsPlainText { get; set; }
    }


}
