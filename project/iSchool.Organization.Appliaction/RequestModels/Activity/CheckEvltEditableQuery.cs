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
    /// 检查(活动)评测能否编辑
    /// </summary>
    public class CheckEvltEditableQuery : IRequest<CheckEvltEditableQueryResult>
    {
        public Guid EvltId { get; set; }

        public ActivityEvaluationBind? Aeb { get; set; }
    }    

#nullable disable
}
