using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 获取素材的list
    /// </summary>

    public class MeterialPgListQuery : IRequest<MeterialPgListQueryResponse>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string CourseName { get; set; }
        public string MeterialName { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
