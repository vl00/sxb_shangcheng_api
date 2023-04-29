using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{

    /// <summary>
    /// 新增评测-页面展示的实体
    /// </summary>    
    public class AddEvaluationShowDto
    {
        /// <summary>
        /// 新增机构Id
        /// </summary>
        public Guid Id { get; set; }

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
        public List<SelectListItem> ListSpecials { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 机构集合
        /// </summary>
        public List<SelectListItem> ListOrgs { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 课程集合
        /// </summary>
        public List<SelectListItem> ListCourses { get; set; } = new List<SelectListItem>();

        #endregion      
    }
}
