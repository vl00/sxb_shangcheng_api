using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 评测详情 -- base info
    /// </summary>
    public class GetEvltBaseInfoQuery : IRequest<EvltBaseInfoDto>
    {        
        public long No { get; set; }
        /// <summary>
        /// 优先处理id, 没就传default
        /// </summary>
        public Guid EvltId { get; set; }
    }
}
