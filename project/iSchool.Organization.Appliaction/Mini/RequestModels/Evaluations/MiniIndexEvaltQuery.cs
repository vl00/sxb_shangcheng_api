using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{

    /// <summary>
    /// 首页精选课程分页
    /// </summary>
    public class MiniIndexEvaltQuery : IRequest<MiniIndexEvalts>
    {
        public MiniIndexEvaltQuery()
        {
        }

        public MiniIndexEvaltQuery(int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
        //[Min(1, ErrorMessage = "页码必须大于1")]
        public int PageIndex { get; set; } = 1;

        //[Min(1, ErrorMessage = "每页大小必须大于1")]
        public int PageSize { get; set; } = 10;
    }
}
