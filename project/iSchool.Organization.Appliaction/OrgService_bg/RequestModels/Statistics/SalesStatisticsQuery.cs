using iSchool.Organization.Appliaction.ResponseModels.Statistics;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class SalesStatisticsQuery : IRequest<SalesStatisticsResponse>
    {
        /// <summary>
        /// 查询几天的数据
        /// </summary>
        public int Day { get; set; }
    }
}
