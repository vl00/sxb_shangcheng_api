using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 更新db里的课程库存
    /// </summary>
    public class UpbackCourseStockCommand : IRequest<bool>
    {
        /// <inheritdoc cref="UpbackCourseStockCommand"/>
        public UpbackCourseStockCommand() { }

        public int? DoSec { get; set; }
    }

#nullable disable
}
