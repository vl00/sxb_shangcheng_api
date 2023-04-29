using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 新增/编辑机构展示Model
    /// </summary>
    public class OrgAddOrEditShowDto
    {
        /// <summary>
        /// 是否新增
        /// </summary>
        public bool IsAdd { get; set; } = false;

        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 副标题1
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 副标题2
        /// </summary>
        public string SubDesc { get; set; }

        /// <summary>
        /// LOGO
        /// </summary>
        public string LOGO { get; set; }

        /// <summary>
        /// 机构分类
        /// </summary>
        public string Types { get; set; }

        /// <summary>
        /// 好物分类
        /// </summary>
        public string GoodthingTypes { get; set; }

        /// <summary>
        /// 机构分类下拉框数据源
        /// </summary>
        public List<SelectListItem> ListSelectTypes { get; set; }

        /// <summary>
        /// 好物分类下拉框数据源
        /// </summary>
        public List<SelectListItem> ListSelectGoodthingTypes { get; set; }


        ///// <summary>
        ///// 使用年龄段
        ///// </summary>
        //public List<SelectListItem> OldAges { get; set; }

        ///// <summary>
        ///// 使用年龄段下拉框数据源
        ///// </summary>
        //public List<SelectListItem> ListSelectAges { get; set; }

        /// <summary>
        /// 最小年龄
        /// </summary>
        public int? MinAge { get; set; }

        /// <summary>
        /// 最大年龄
        /// </summary>
        public int? MaxAge { get; set; }

        /// <summary>
        /// 教学模式
        /// </summary>
        public string Modes { get; set; }

        /// <summary>
        /// 教学模式下拉框数据源
        /// </summary>
        public List<SelectListItem> ListSelectModes { get; set; }

        /// <summary>
        /// 机构简介
        /// </summary>
        public string Intro { get; set; }

        /// <summary>
        /// 品牌分类
        /// </summary>
        public List<OrgService_bg.ResponseModels.BgMallFenleisLoadQueryResult> BrandTypes { get; set; }



    }
}
