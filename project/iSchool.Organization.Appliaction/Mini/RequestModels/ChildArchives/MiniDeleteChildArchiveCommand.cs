using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class MiniDeleteChildArchiveCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 孩子档案id
        /// </summary>
        public Guid Id { get; set; }
    }
}
