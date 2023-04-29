using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class MpMallThemeDetailQryResult
    {
        /// <summary>主题id</summary> 
        public Guid Tid { get; set; }
        /// <summary>主题短id</summary> 
        public string Tid_s { get; set; } = default!;
        /// <summary>主题名称</summary> 
		public string Tname { get; set; } = default!;
        /// <summary>主题logo</summary> 
		public string? Tlogo { get; set; }
        /// <summary>主题会场图 banner</summary> 
		public string? Tbanner { get; set; }

        /// <summary>专题s</summary> 
        public MpMallThemeDetailQryResult_Special[] Specials { get; set; } = default!;
        /// <summary>当主题不足3个时,不显示more按钮</summary> 
        public bool IsThemesLessThan3 { get; set; } = false;

        /// <summary>专题id</summary> 
        public Guid Spid { get; set; }
        /// <summary>专题短id</summary> 
        public string Spid_s { get; set; } = default!;
        /// <summary>专题名称</summary> 
        public string Spname { get; set; } = default!;
        /// <summary>专题 banner</summary> 
		public string? Spbanner { get; set; }

        /// <summary>当前专题分享文案</summary>
        public string? SpShareText { get; set; }

        /// <summary>m背景图</summary>
        public string Background { get; set; } = default!;

        /// <summary>概念图片-锚点s</summary> 
        public MpMallThemeDetailQryResult_Concept[] Concepts = default!;

        /// <summary>专题下商品s</summary> 
        public IEnumerable<MpCourseDataDto> Courses { get; set; } = default!;
    }

    public class MpMallThemeDetailQryResult_Special
    {
        /// <summary>专题id</summary> 
        public Guid Spid { get; set; }
        /// <summary>专题短id</summary> 
        public string Spid_s { get; set; } = default!;
        /// <summary>专题名称</summary> 
        public string Spname { get; set; } = default!;
        /// <summary>专题banner</summary> 
        public string? Spbanner { get; set; } = default!;
        /// <summary>专题banner缩略图</summary> 
        public string? Spbanner_s { get; set; } = default!;

        /// <summary>分享文案</summary>
        public string? ShareText { get; set; }
    }

    public class MpMallThemeDetailQryResult_Concept
    {
        public string Shape { get; set; } = default!;
        public string Coords { get; set; } = default!;
        /// <summary>商品spu id</summary> 
        public Guid CourseId { get; set; }
        /// <summary>商品spu 短id</summary> 
        public string CourseId_s { get; set; } = default!;

        public string Href { get; set; } = default!;
    }

#nullable disable
}
