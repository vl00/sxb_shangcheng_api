using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 课程详情返回实体类
    /// </summary>
    public class CourseDetailsResponse : ISeoTDKInfo
    {
        /// <summary>课程Id</summary>
        public Guid Id { get; set; }
        /// <summary>课程短Id</summary>
        public string Id_s { get; set; }

        /// <summary>
        /// 是否允许积分兑换
        /// </summary>
        public bool? IsPointExchange { get; set; }

        /// <summary>
        /// 产品名字
        /// </summary>
        public string CName { get; set; }

        /// <summary>
        /// 课程类型 1=课程 2=好物
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 课程banner图片地址
        /// </summary>
        public List<string> Banner { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>科目</summary>
        public int Subject { get; set; }
        public string SubjectDesc { get; set; }

        /// <summary>科目s</summary>
        public int[] Subjects { get; set; }
        public string[] SubjectDescs { get; set; }

        #region 当前用户是否收藏
        /// <summary>
        /// 当前用户收藏课程状态(默认值false， true:已收藏；false:未收藏)
        /// </summary>
        public bool IsCurrentUserCollection { get; set; } = false;
        #endregion

        #region 关联机构信息

        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrgId { get; set; }

        /// <summary>
        /// 机构短Id
        /// </summary>
        public string OrgNoId { get; set; }

        /// <summary>
        /// 机构logo
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 认证（true：认证；false：未认证）
        /// </summary>
        public bool Authentication { get; set; }

        /// <summary>
        /// 机构描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 机构子描述
        /// </summary>
        public string SubDesc { get; set; }

        #endregion


        #region 相关评测
        /// <summary>
        /// 优质评测
        /// </summary>
        public EvaluationInfo EvaluationInfo { get; set; }

        ///// <summary>
        ///// 评测内容
        ///// </summary>
        //public string Content { get; set; }

        ///// <summary>
        ///// 评测图片
        ///// </summary>
        //public string Pictures { get; set; }

        ///// <summary>
        ///// 评测认证（true：认证；false：未认证）
        ///// </summary>
        //public bool EAuthentication { get; set; }

        #endregion

        /// <summary>
        ///课程详情
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// 当前登录用户的手机号码
        /// </summary>
        public string LoginUserMobile { get; set; }

        /// <summary>课程价格</summary>
        public decimal Price { get; set; }
        /// <summary>原始价格</summary> 
        public decimal? Origprice { get; set; }


        /// <summary>
        /// 积分SKU里最小原价
        /// </summary>
        public decimal? PointsMinOrigprice { get; set; }

        /// <summary>
        /// seo  decription
        /// </summary>
        public string Tdk_d { get; set; } = default!;

        /// <summary>
        /// 当前用户是否分销顾问.<br/>
        /// true = 是 <br/>
        /// false = 不是或没数据<br/>
        /// </summary>
        public bool IsHeadFxUser { get; set; }

        public List<string> Video { get; set; }
        public List<string> VideoCovers { get; set; }

        /// <summary>是否隐形上架</summary> 
        public bool IsInvisibleOnline { get; set; }
        /// <summary>
        /// 是否有新人立返
        /// </summary>
        public bool CanNewUserReward { get; set; }
        /// <summary>
        /// 新人立返金额
        /// </summary>
        public decimal NewUserRewardAmount { get; set; }

        /// <summary>是否新人专享</summary> 
        public bool NewUserExclusive { get; set; } = false;
        /// <summary>是否限时优惠</summary> 
        public bool LimitedTimeOffer { get; set; } = false;
        /// <summary>下架时间(戳).可null</summary> 
        [JsonConverter(typeof(DateTimeToTimestampJsonConverter))]
        public DateTime? LastOffShelfTime { get; set; }
        /// <summary>
        /// 是否rw微信群拉新买隐形上架好物<br/>
        /// 为true时`不让添加到购物车`
        /// </summary>
        public bool IsRwInviteActivity { get; set; }

        /// <summary>
        /// 当前用户的购物车数量, 未登录用户返回null
        /// </summary>
        public int? CartCount { get; set; }

        /// <summary>运费地区</summary>
        public List<CFreightM> FreightList { get; set; }
        /// <summary>不发货地区</summary>
        public List<NameCodeDto<int>> FreightBlackList { get; set; } = null;
        /// <summary>
        /// 销量
        /// </summary>
        public int SellCount { get; set; }



    }


    public class CFreightM
    {
        public string Area { get; set; }
        public decimal Cost { get; set; }
        public List<string> CityName { get; set; }
        public List<string> CityCode { get; set; }

    }

    /// <summary>
    /// 数据库课程详情
    /// </summary>
    public class CourseDetailsDB
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }
        public long No { get; set; }

        /// <summary>
        /// 是否允许积分兑换
        /// </summary>
        public bool? IsPointExchange { get; set; }
        /// <summary>
        /// 课程名字
        /// </summary>
        public string CName { get; set; }

        /// <summary>
        /// 课程banner图片地址
        /// </summary>
        public string Banner { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程副标题
        /// </summary>
        public string Subtitle { get; set; }

        public int? Subject { get; set; }

        #region 关联机构信息
        //private string orgNoId;
        /// <summary>
        /// 机构短Id
        /// </summary>
        public string OrgNoId { get; set; }
        //{
        //    get { return orgNoId; }
        //    set { try { orgNoId = UrlShortIdUtil.Long2Base32(Convert.ToInt64(value)); } catch { orgNoId = value; } }
        //}

        public Guid OrgId { get; set; }

        /// <summary>
        /// 机构logo
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 认证（true：认证；false：未认证）
        /// </summary>
        public bool Authentication { get; set; }

        /// <summary>
        /// 机构描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 机构子描述
        /// </summary>
        public string SubDesc { get; set; }

        #endregion


        #region 相关评测
        /// <summary>
        /// 优质评测
        /// </summary>
        public EvaluationInfo EvaluationInfo { get; set; }

        ///// <summary>
        ///// 评测内容
        ///// </summary>
        //public string Content { get; set; }

        ///// <summary>
        ///// 评测图片
        ///// </summary>
        //public string Pictures { get; set; }

        ///// <summary>
        ///// 评测认证（true：认证；false：未认证）
        ///// </summary>
        //public bool EAuthentication { get; set; }

        #endregion

        /// <summary>
        ///课程详情
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        public decimal? Origprice { get; set; }

        /// <summary>
        /// 积分SKU里最小原价
        /// </summary>
        public decimal? PointsMinOrigprice { get; set; }

        public int Type { get; set; }

        public string Subjects { get; set; }
        public string Videos { get; set; }
        public string VideoCovers { get; set; }

        public bool? IsInvisibleOnline { get; set; }
        public bool CanNewUserReward { get; set; }
        public decimal NewUserRewardValue { get; set; }
        public decimal NewUserRewardType { get; set; }

        public bool NewUserExclusive { get; set; }
        public bool LimitedTimeOffer { get; set; }
        public DateTime? LastOffShelfTime { get; set; }

        public string BlackList { get; set; }
        public int SellCount { get; set; }
    }

    /// <summary>
    /// 机构信息
    /// </summary>
    public class OrganizationInfo
    {
        /// <summary>
        /// 图片
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 认证（true：认证；false：未认证）
        /// </summary>
        public bool Authentication { get; set; }
    }

    /// <summary>
    /// 评测信息
    /// </summary>
    public class EvaluationInfo
    {
        private string evltNoId;
        /// <summary>
        /// 评测短Id
        /// </summary>
        public string EvltNoId { get; set; }
        //{
        //    get { return evltNoId; }
        //    set { try { evltNoId = UrlShortIdUtil.Long2Base32(Convert.ToInt64(value)); } catch { evltNoId = value; } }
        //}

        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvaluationId { get; set; }

        /// <summary>
        /// 评测内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 原图
        /// </summary>
        public List<string> Pictures { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public List<string> Thumbnails { get; set; }


    }

    #region 中转Model
    /// <summary>
    /// 评测信息DB
    /// </summary>
    public class EvaluationInfoDB
    {
        //private string evltNoId;
        /// <summary>
        /// 评测短Id
        /// </summary>
        public string EvltNoId { get; set; }
        //{
        //    get { return evltNoId; }
        //    set { try { evltNoId = UrlShortIdUtil.Long2Base32(Convert.ToInt64(value)); } catch { evltNoId = value; } }
        //}

        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvaluationId { get; set; }

        /// <summary>
        /// 评测内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 原图
        /// </summary>
        public string Pictures { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnails { get; set; }


    }

    /// <summary>
    /// 用于辅助获取符合条件的评测Id
    /// </summary>
    public class EvaluationInfoByIdDB
    {

        /// <summary>
        /// 评测短Id
        /// </summary>
        public long EvltNoId { get; set; }

        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvaluationId { get; set; }

        /// <summary>
        /// 置顶(是否加精)
        /// </summary>
        public bool Stick { get; set; }

        /// <summary>
        /// 浏览数
        /// </summary>
        public int ViewCount { get; set; }

    }

    #endregion
}
