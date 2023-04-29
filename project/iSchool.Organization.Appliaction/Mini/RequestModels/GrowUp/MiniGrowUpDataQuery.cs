using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class MiniGrowUpDataQuery : IRequest<MiniGrowUpDataDto>
    {
        /// <summary>
        /// 每页大小(默认10)
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
