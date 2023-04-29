using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    
    /// <summary>
    /// 新增/编辑课程页面展示实体
    /// </summary>
    public class AddCoursesShowDto
    {
        #region 课程Info
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程副标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid? OrgId { get; set; }

        /// <summary>
        /// 课程价格
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 课程库存
        /// </summary>
        public int? Stock { get; set; }

        /// <summary>
        /// 上课时长(单位:分钟)
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// 上课方式
        /// </summary>
        public List<int> ListOldModes { get; set; }

        /// <summary>
        /// 科目
        /// </summary>
        public int? Subject { get; set; }

        /// <summary>
        /// 最小年龄
        /// </summary>
        public int? MinAge { get; set; }

        /// <summary>
        /// 最大年龄
        /// </summary>
        public int? MaxAge { get; set; }

        /// <summary>
        /// 课程Banner的Html
        /// </summary>
        [Obsolete]
        public string Banner { get; set; }

        /// <summary>课程Banner</summary>
        public string BannerUrls { get; set; }
        /// <summary>课程Banner缩略图</summary>
        public string BannerUrls_s { get; set; }

        /// <summary>
        /// 课程详情
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// 是否新增(true:新增;false:编辑;)
        /// </summary>
        public bool IsAdd { get; set; } = false;

        /// <summary>
        /// 上架时间
        /// </summary>
        public DateTime? LastOnShelfTime { get; set; }
        /// <summary>
        /// 下架时间
        /// </summary>
        public DateTime? LastOffShelfTime { get; set; }

        /// <summary>
        /// 购前须知集合(json)
        /// </summary>
        public List<CourseNotices> ListNotices { get; set; }
        public string ListNoticesJson { get; set; }

        /// <summary>
        /// 1==课程；2==好物
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 科目分类
        /// </summary>
        public List<string> ListOldSubjects { get; set; }

        /// <summary>
        /// 好物分类
        /// </summary>
        public List<string> ListOldGoodthingTypes { get; set; }

        /// <summary>商城分类(3级)</summary>
        public List<OrgService_bg.ResponseModels.BgMallFenleisLoadQueryResult> ListOldCommodityTypes { get; set; }

        /// <summary>
		/// 是否隐形上架
		/// </summary> 
		public bool? IsInvisibleOnline { get; set; }

        /// <summary>
        /// 是否爆款
        /// </summary> 
        public bool? IsExplosions { get; set; }

        /// <summary>
        /// 是否系统课程
        /// </summary>
        public bool? IsSystemCourse { get; set; }

        /// <summary>
        /// 课程视频
        /// </summary>
        public List<string> Videos { get; set; }

        /// <summary>
        /// 课程视频封面
        /// </summary>
        public List<string> VideoCovers { get; set; }

        public long No { get; set; }

        /// <summary>新人专享</summary>
        public bool NewUserExclusive { get; set; }

        /// <summary>限时优惠</summary>
        public bool LimitedTimeOffer { get; set; }

        /// <summary>置顶</summary>
        public bool SetTop { get; set; }

        /// <summary>spu限购数量</summary>
        public int? SpuLimitedBuyNum { get; set; }

        #endregion

        #region 枚举下拉框集合

        /// <summary>
        /// 上课时长枚举集合(Text-Value)
        /// </summary>
        public List<SelectListItem> ListDurations { get; set; } = EnumUtil.GetSelectItems<CourceDurationEnum>();

        /// <summary>
        /// 上课方式枚举集合(Text-Value)
        /// </summary>
        public List<SelectListItem> ListModes { get; set; } = EnumUtil.GetSelectItems<TeachModeEnum>();

        /// <summary>
        /// 科目分类枚举集合(Text-Value)
        /// </summary>
        public List<SelectListItem> ListSubjects { get; set; } = EnumUtil.GetSelectItems<SubjectEnum>();

        /// <summary>
        /// 好物分类
        /// </summary>
        public List<SelectListItem> ListGoods { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 机构分类集合((Text-Value))
        /// </summary>
        public List<SelectListItem> ListOrgTypes { get; set; } = EnumUtil.GetSelectItems<OrgCfyEnum>();

        #endregion

        /// <summary>
        /// 机构集合
        /// </summary>
        public List<SelectListItem> ListOrgs { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 供应商集合
        /// </summary>
        public List<SelectListItem> ListSuppliers { get; set; } = new List<SelectListItem>();

        #region 属性-选项
        /// <summary>属性-选项</summary>
        public List<PropertyAndItems> PropInfoList { get; set; }
        #endregion

        /// <summary>
        /// 课程分销信息
        /// </summary>
        public CourseDrpInfo DrpInfo { get; set; }

        /// <summary>
        /// 大课信息
        /// </summary>
        public List<BigCourse> BigCoursesList { get; set; }


        #region 运费
        /// <summary>运费s</summary>
        public FreightItemDto[] Freights { get; set; }
        #endregion
    }

    /// <summary>运费地区item dto</summary>
    public class FreightItemDto
    {
        /// <summary>运费地区类型</summary>
        public byte Type { get; set; }
        /// <summary>城市名称集合</summary>
        public string[] Names { get; set; } = null;
        /// <summary>城市集合</summary> 
        public int[] Citys { get; set; } = null;
        /// <summary>费用</summary> 
        public decimal Cost { get; set; } = 0m;
    }

    /// <summary>不发货地区item dto</summary>
    public class FreightBlackListDto
    {
        /// <summary>城市名称集合</summary>
        public string[] Names { get; set; } = null;
        /// <summary>城市集合</summary> 
        public int[] Citys { get; set; } = null;
    }
}
