using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class MiniMyEvaluationsQuery : IRequest<MiniMyEvaluationsDto>
    {
        /// <summary>
        /// 0.默认排序  1.按点赞  2.按分享
        /// </summary>
        public int OrderBy { get; set; }

        /// <summary>
        /// 页码
        /// </summary>        
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 10;
    }

}
