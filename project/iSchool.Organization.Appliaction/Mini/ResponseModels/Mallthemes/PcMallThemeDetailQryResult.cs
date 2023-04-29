using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class PcMallThemeDetailQryResult
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

        /// <summary>pc背景图</summary>
        public string Background { get; set; } = default!;

        /// <summary>当主题不足3个时,不显示more按钮</summary> 
        public bool IsThemesLessThan3 { get; set; } = false;
        /// <summary>本期主题短id</summary> 
        public string? CurrTid_s { get; set; }
        /// <summary>本期主题logo</summary> 
        public string? CurrTlogo { get; set; }
        /// <summary>(本)上期主题短id</summary> 
        public string? PrevCurrTid_s { get; set; }
        /// <summary>(本)上期主题logo</summary> 
        public string? PrevCurrTlogo { get; set; }

        /// <summary>专题s</summary> 
        public PcMallThemeDetailQryResult_Special[] Specials { get; set; } = default!;
    }

    public class PcMallThemeDetailQryResult_Special
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

        /// <summary>pc专题图</summary> 
        public string? ConceptPicture { get; set; }

        /// <summary>分享文案</summary>
        public string? ShareText { get; set; }

        /// <summary>专题下商品s</summary> 
        public IEnumerable<MpCourseDataDto> Courses { get; set; } = default!;
    }

#nullable disable
}
