using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 课程分销-推广奖励信息
    /// </summary>
    public class GetCourseDrpFxInfoDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = default!;

        /// <summary>课程商品与奖励</summary>
        public IEnumerable<CourseDrpFxRewardLsItemDto>? RewardList { get; set; }
        /// <summary>收货后佣金冻结天数</summary>
        public int ReceivingAfterDays { get; set; } = 3;
        /// <summary>升级顾问的最低消费金额</summary>
        public decimal Condition1ConsumedMoneys { get; set; }

        [Obsolete("1.5-")]
        public BigCourseDrpFxInfoDto? BigCourseInfo { get; set; }

        /// <summary>多个大课.没数据为null</summary>
        public IEnumerable<BigCourseDrpFxInfoDto>? BigCourseInfos { get; set; }

        /// <summary>
        /// (当前用户)是否高级顾问
        /// </summary>
        public bool IsHighHead { get; set; } = false;
        /// <summary>
        /// 奖励积分信息
        /// </summary>
        public IntegralInfo IntegralInfo { get; set; }
    }
    public class IntegralInfo
    {
        public int Min { get; set; }
        public int Max { get; set; }

    }


    public class CourseDrpFxRewardLsItemDto
    {
        /// <summary>商品id</summary>
        public Guid Id { get; set; }
        /// <summary>商品名字</summary>
        public string Name { get; set; } = default!;
        /// <summary>商品价格</summary>
        public decimal Price { get; set; }

        /// <summary>返现类型(数值)</summary> 
		public byte CashbackType { get; set; }
        /// <summary>返现类型(中文)</summary> 
        public string? CashbackTypeDesc { get; set; }
        /// <summary>返现数值</summary> 
        public decimal CashbackValue { get; set; }

        /// <summary>返现金额(比例需要算出)</summary> 
        public decimal CashbackMoney { get; set; }

        /// <summary>
        /// 社群管理工资==高级顾问间接工资<br/>
        /// 只有高级顾问进页面才有,其他用户进来为0, 前端应该隐藏, 数值=推广奖励*全民营销后台设置的比例
        /// </summary>
        public decimal MgrMoney { get; set; } = 0m;
    }

    /// <summary>
    /// 大课分销信息
    /// </summary>
    public class BigCourseDrpFxInfoDto
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }

        /// <summary>大课标题|名称</summary> 
        public string Title { get; set; } = default!;
		/// <summary>大课价格</summary> 
		public decimal Price { get; set; }
		/// <summary>用于前端推广奖励时间-开始时间</summary> 
		public DateTime? StartTime { get; set; }
		/// <summary>用于前端推广奖励时间-结束时间</summary> 
		public DateTime? EndTime { get; set; }
		/// <summary>达成条件</summary> 
		public string? Condition { get; set; }

        /// <summary>
        /// 返现类型(数值)<br/>
        /// 1=<inheritdoc cref="CashbackTypeEnum.Percent"/><br/>
        /// 2=<inheritdoc cref="CashbackTypeEnum.Yuan"/><br/>
        /// </summary> 
        public byte CashbackType { get; set; }
        /// <summary>返现类型(中文)</summary> 
        public string? CashbackTypeDesc { get; set; }
        /// <summary>返现数值</summary> 
        public decimal? CashbackValue { get; set; }

		/// <summary>说明</summary> 
		public string? Desc { get; set; }

        /// <summary>返现金额(比例需要算出)</summary> 
        public decimal CashbackMoney { get; set; }

    }

#nullable disable
}
