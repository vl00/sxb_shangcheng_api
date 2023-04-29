using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 分享评测cmd
    /// </summary>
    public class ShareEvltCommand : IRequest<ShareLinkDto>
    { 
        /// <summary>
        /// 评测id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 渠道
        /// </summary>
        public string Cnl { get; set; }
    }
}
