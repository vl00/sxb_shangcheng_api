using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// mini种草圈
    /// </summary>
    public class MiniEvltGlassIndexQuery : IRequest<MiniEvltGrassIndexQryResult>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        /// <summary>(综合)排序</summary>
        public int Orderby { get; set; }
        /// <summary>科目</summary>
        //public int Subj { get; set; }
        /// <summary>商品分类id</summary>
        public string CatogoryId { get; set; }
        /// <summary>品牌</summary>
        public Guid? Brand { get; set; }
        /// <summary>内容形式</summary>
        public int Ctt { get; set; }

        //public string? SearchTxt { get; set; }

        /// <summary>
        /// 课程id. <br/>
        /// 大家的种草是具体某个课程进去的那个课程下面的种草内容, 不需要排除自己
        /// </summary>
        public Guid? CourseId { get; set; }

        /// <summary>
        /// 1=ios会屏蔽网课 <br/>
        /// 0=ios不会屏蔽网课
        /// </summary>
        public int AllowIosNodisplay { get; set; } = 1;
    }
}
#nullable disable
