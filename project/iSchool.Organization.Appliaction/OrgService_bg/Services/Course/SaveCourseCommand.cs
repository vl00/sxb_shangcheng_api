using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    public class SaveCourseCommand : IRequest<ResponseResult>
    {

        #region 课程信息
        /// <summary>
        /// 课程Id 
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>课程名称 </summary>
        public string Name { get; set; }

        /// <summary>课程标题</summary>
        public string Title { get; set; }

        /// <summary>课程副标题</summary>
        public string SubTitle { get; set; }

        /// <summary>机构Id</summary>
        public Guid OrgId { get; set; }

        /// <summary>编辑时存储旧的机构Id</summary>
        public Guid? OldOrgId { get; set; }

        /// <summary>课程价格</summary>
        public decimal Price { get; set; }

        /// <summary>课程库存</summary>
        public int Stock { get; set; }

        /// <summary>上课时长(单位:分钟)</summary>
        public int? Duration { get; set; }

        /// <summary>上课方式(json格式)</summary>
        public string Mode { get; set; }

        /// <summary>
        /// 类型1=课程2=好物
        /// </summary>
        public int Type { get; set; }

        /// <summary>科目分类(json格式)</summary>
        public string Subjects { get; set; }

        /// <summary>好物分类(json格式)</summary>
        public string GoodthingTypes { get; set; }

        /// <summary>商品分类(第3级的code)(json格式)</summary>
        public string CommodityTypes { get; set; }

        /// <summary>科目</summary>
        public int? Subject { get; set; }

        /// <summary>编辑时存储旧的科目Id</summary>
        public int? OldSubject { get; set; }

        /// <summary>最小年龄</summary>
        public int? MinAge { get; set; }

        /// <summary>最大年龄</summary>
        public int? MaxAge { get; set; }

        /// <summary>课程Banner(json格式)</summary>
        public string Banner { get; set; }
        /// <summary>课程Banner缩略图(json格式)</summary>
        public string Banner_s { get; set; }

        /// <summary>课程详情</summary>
        public string Detail { get; set; }

        /// <summary>自动上架时间（新增时才有）</summary>
        public DateTime? LastOnShelfTime { get; set; }

        /// <summary>自动下架时间（新增有；当编辑设置佣金锁定期具体日期也有）</summary>
        public DateTime? LastOffShelfTime { get; set; }

        /// <summary>操做者</summary>
        [Newtonsoft.Json.JsonIgnore]
        public Guid UserId { get; set; }

        /// <summary>是否新增(true:新增；false:编辑) </summary>
        public bool IsAdd { get; set; }

        /// <summary>是否系统课程</summary>
        public bool? IsSystemCourse { get; set; }

        /// <summary>
        /// 课程视频(json)
        /// </summary>
        public string Videos { get; set; }

        /// <summary>
        /// 课程视频封面(json)
        /// </summary>
        public string VideoCovers { get; set; }

        /// <summary>
        /// 购前须知集合
        /// </summary>
        public List<CourseNotices> ListNotices { get; set; }

        /// <summary>
        /// 是否隐形上架
        /// </summary> 
        public bool? IsInvisibleOnline { get; set; }

        /// <summary>
        /// 是否爆款
        /// </summary> 
        public bool? IsExplosions { get; set; }

        public long? No { get; set; }

        /// <summary>新人专享</summary>
        public bool NewUserExclusive { get; set; }

        /// <summary>限时优惠</summary>
        public bool LimitedTimeOffer { get; set; }

        /// <summary>置顶</summary>
        public bool? SetTop { get; set; }

        /// <summary>spu限购数量</summary>
        public int? SpuLimitedBuyNum { get; set; }

        #endregion

        #region 大课集合

        /// <summary>
        /// 大课集合
        /// </summary>
        public List<BigCourse> BigCourseList { get; set; }
        #endregion

        #region 大课信息 旧
        ///// <summary>是否更新(true:新增一条大课记录，软删除旧记录；false:删除) </summary>
        //public bool IsUpdate { get; set; } = false;

        ///// <summary>大课名称</summary>
        //public string BigCourseTitle { get; set; }

        ///// <summary>达成条件</summary>
        //public string Condition { get; set; }

        ///// <summary>大课价格</summary>
        //public decimal BigCoursePrice { get; set; }

        ///// <summary>返现类型</summary>
        //public int bigCashbackType { get; set; }

        ///// <summary>返现比例</summary>
        //public decimal bigCashbackValue { get; set; }

        ///// <summary>开始时间</summary>
        //public DateTime? BigStartTime { get; set; }

        ///// <summary>结束时间</summary>
        //public DateTime? BigEndTime { get; set; }

        ///// <summary>条件说明</summary>
        //public string ConditionDesc { get; set; } = "";

        #endregion

        #region 分销规则



        /// <summary>分销自购返现类型（推广佣金）</summary>
        public int CashbackType { get; set; }

        /// <summary>分销自购返现数值（推广佣金）</summary>
        public decimal? CashbackValue { get; set; }

        /// <summary>佣金锁定期类型</summary>
        public int NolimitType { get; set; }

        /// <summary>佣金锁定期-具体日期后解冻</summary>
        public DateTime? NolimitAfterDate { get; set; }

        /// <summary>佣金锁定期-购买后x日后解冻</summary>
        public int? NolimitAfterBuyInDays { get; set; }

        /// <summary>(编辑时)自动上架时间</summary>
        public DateTime? AutoOnShelfDate_Edit { get; set; }
        /// <summary>(编辑时)自动下架时间</summary>
        public DateTime? AutoOffShelfDate_Edit { get; set; }


        /// <summary>平级佣金类型</summary>
        public int PJCashbackType { get; set; }

        /// <summary>平级佣金数值</summary>
        public decimal? PJCashbackValue { get; set; }

        /// <summary>
		/// 工资系数计算（1：是；0：否）
		/// </summary>
		public bool IsBonusRate { get; set; } = true;

        /// <summary>
        /// 上线独享数值
        /// </summary>
        public decimal? HeadFxUserExclusiveValue { get; set; }

        /// <summary>
        /// 上线独享类型(1：%；2：元)
        /// </summary>
        public int HeadFxUserExclusiveType { get; set; }

        /// <summary>
        /// 上线独享收益
        /// (上线独享类型=1，上线独享收益 = 课程金额 * 上线独享数值 * 0.01；上线独享类型=2，上线独享收益=上线独享数值;)
        /// </summary>
        public decimal? HeadFxUserExclusiveMoney { get; set; }

        /// <summary>新人奖励值</summary> 
		public decimal? NewUserRewardValue { get; set; }
        /// <summary>新人奖励类型</summary> 
        public int? NewUserRewardType { get; set; }
        /// <summary>
        ///锁定时间（确认收货后几天）
        /// </summary>
        public int? ReceivingAfterDays { get; set; }

        #endregion

        #region 属性-价格-库存-限购数量-显示
        /// <summary>商品待更新实体集合</summary>
        public List<UpdateGoodsInfo> UpdateGoodsInfos { get; set; }
        #endregion

        #region 运费 + 不发货地区
        /// <summary>运费s</summary>
        public FreightItemDto[] Freights { get; set; }

        /// <summary>不发货地区</summary>
        public int[] FreightBacklist { get; set; }
        #endregion

        #region 更新sku兑换积分
        /// <summary>是否支持积分兑换</summary>
        public bool EnablePointExchange { get; set; }
        /// <summary>更新sku兑换积分</summary>
        public List<UpdateSkuPointExchangeItem> UpdateSkuPointExchanges { get; set; }
        #endregion 
    }


}
