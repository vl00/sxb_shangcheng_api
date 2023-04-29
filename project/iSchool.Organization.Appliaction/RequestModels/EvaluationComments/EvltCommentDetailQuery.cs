using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 评测详情
    /// </summary>
    public class EvltCommentDetailQuery : IRequest<ResponseModels.EvltCommentDetailDto>
    {
       
        public Guid EvltCmtId { get; set; }
    }
}
