using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ResponseModels
{
    public class BgMallFenleiDragDropCmdResult
    {
        /// <summary>
        /// 改变了的排序 (code, sort)
        /// </summary>
        public IEnumerable<(int, int)> NewSorts { get; set; }
    }
}
