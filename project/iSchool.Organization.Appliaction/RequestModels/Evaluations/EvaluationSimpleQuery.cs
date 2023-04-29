using iSchool.Organization.Appliaction.ResponseModels.Evaluations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Evaluations
{
    public class EvaluationSimpleQuery : IRequest<EvaluationSimpleDto>
    {
        public Guid Id { get; set; }
    }
}
