using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 查询活动评测最新bind记录
    /// </summary>
    public class ActivityEvltLatestQuery : IRequest<ActivityEvaluationBind?>
    {
        /// <inheritdoc cref="ActivityEvltLatestQuery"/>
        public ActivityEvltLatestQuery() { }

        /// <summary>评测id</summary>
        public Guid EvltId { get; set; }
    }

#nullable disable
}
