using iSchool.Organization.Appliaction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 后台分类列表item dto
    /// </summary>
    public class BgMallFenleiItemDto : NameCodeDto<int>
    {
        /// <summary>排序</summary>
        public int Sort { get; set; }
        /// <summary>父code</summary>
        public int Pcode { get; set; } = 0;

        public int Depth { get; set; }
        /// <summary>图</summary>
        public string Img { get; set; }
    }
}
