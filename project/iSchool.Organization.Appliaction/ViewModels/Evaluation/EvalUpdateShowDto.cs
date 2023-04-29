using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;


namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 评测编辑页面展示的实体
    /// </summary>
    public class EvalUpdateShowDto
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid Id { get; set; }

        
        /// <summary>
        /// 专题绑定表Id
        /// </summary>
        public Guid? SpecialBindId { get; set; }

        /// <summary>
        /// 专题Id
        /// </summary>
        public Guid? Specialid { get; set; }

        /// <summary>
        /// 评测绑定记录集合
        /// </summary>
        public List<EvaluationBind> ListEvltBind { get; set; }

        /// <summary>
        /// 评测标题
        /// </summary>
        public string ETitle { get; set; }

        #region old
        ///// <summary>
        ///// 机构Id
        ///// </summary>
        //public Guid? Orgid { get; set; }

        ///// <summary>
        ///// 评测绑定表Id
        ///// </summary>
        //public Guid EvaluationBindId { get; set; }

        ///// <summary>
        ///// 课程Id
        ///// </summary>
        //public Guid? Courseid { get; set; }

        ///// <summary>
        ///// 已有课程
        ///// </summary>
        //public ShowDBCourseModel ShowDBCourse { get; set; }


        ///// <summary>
        ///// (true:已有课程;false:自定义课程;)
        ///// </summary>
        //public bool ExistingorCustom { get; set; }

        ///// <summary>
        ///// 上课时长
        ///// </summary>
        //public int? Duration { get; set; }        

        ///// <summary>
        ///// 上课方式(存储json  用openjson查询)
        ///// </summary>
        //public string Mode { get; set; }

        ///// <summary>
        ///// 科目分类
        ///// </summary>
        //public int? Subject { get; set; }

        ///// <summary>
        ///// 年龄段
        ///// </summary>
        //public int? Age { get; set; }

        ///// <summary>
        ///// 最小年龄
        ///// </summary>
        //public int? MinAge { get; set; }

        ///// <summary>
        ///// 最大年龄
        ///// </summary>
        //public int? MaxAge { get; set; }

        ///// <summary>
        ///// 自定义课程名称
        ///// </summary>
        //public string CourseName { get; set; } 
        #endregion

        /// <summary>
        /// 是否加精
        /// </summary>
        public bool Stick { get; set; }

        /// <summary>
        /// 评测类型(1:自由模式;2:专业模式;)
        /// </summary>
        public int EvltType { get; set; }

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
        /// 年龄段枚举集合(Text-Value)
        /// </summary>
        public List<SelectListItem> ListAges { get; set; } = EnumUtil.GetSelectItems<AgeGroup>();

        #endregion

        #region 数据库数据源下拉框集合

        /// <summary>
        /// 专题集合
        /// </summary>
        public List<SelectListItem> ListSpecials { get; set; }

        /// <summary>
        /// 机构集合
        /// </summary>
        public List<SelectListItem> ListOrgs { get; set; }

        /// <summary>
        /// 课程集合
        /// </summary>
        public List<SelectListItem> ListCourses { get; set; }

        #endregion

        #region 评测项

        /// <summary>
        /// 评测项集合
        /// </summary>
        public List<EvaluationItem> ListEvaluationItems { get; set; }

        #endregion

        #region 评论集合

        /// <summary>
        /// 评论集合
        /// </summary>
        public PagedList<EvltCommentItem> ListEvltComments { get; set; }

        //public IPagedList<EvltCommentItem> MyProperty { get; set; }

        #endregion

        #region 图片HTML集合
        /// <summary>
        /// 图片HTML集合
        /// </summary>
        public Dictionary<int,string> DicUrlHtml { get; set; }
        #endregion

        ///// <summary>
        ///// 正文
        ///// </summary>
        //public string Content { get; set; }

        //TODO
        //评测类型：1:自由模式 2:专业模式；图片和正文再根据评测类型和评测Id获取
        //--图片[dbo].[EvaluationItem] TODO
        //--正文[dbo].[EvaluationItem] TODO
        //--评论[dbo].[EvaluationComment] TODO
    }
}
