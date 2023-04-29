using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{

    /// <summary>
    /// 首页数据查询
    /// </summary>
    public class MiniIndexDataQuery : IRequest<MiniIndexDataDto>
    {
        public int CoursePageSize { get; set; } = 10;
    }
}
