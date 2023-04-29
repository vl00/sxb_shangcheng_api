using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 机构列表
    /// </summary>
    public class OrgListDto
    {
        public List<OrgItem> list { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }

    public class OrgItem
    {
        #region 不展示
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrgId { get; set; }
        #endregion

        /// <summary>
        /// 机构短Id
        /// </summary>
        public string Id_s { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int RowNum { get; set; }       

        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 适合年龄
        /// </summary>
        public string AgeRange { get; set; }

        /// <summary>
        /// 最小年龄
        /// </summary>
        public int MinAge { get; set; }

        /// <summary>
        /// 最大年龄
        /// </summary>
        public int MaxAge { get; set; }

        /// <summary>
        /// 教学模式
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// 课程数
        /// </summary>
        public int CourseCount { get; set; }

        /// <summary>
        /// 是否合作（true：是；其他：否）
        /// </summary>
        public bool? Authentication { get; set; }

        /// <summary>
        /// 品牌类型
        /// </summary>
        public string Types { get; set; }

        /// <summary>
        /// 品牌分类
        /// </summary>
        public string OrgType { get; set; }

        /// <summary>
        /// 好物(json)
        /// </summary>
        public string GoodthingTypes { get; set; }

        /// <summary>
        /// 机构状态(1:上架;0:下架)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// logo
        /// </summary>
        public string Logo { get; set; }
    }

}
