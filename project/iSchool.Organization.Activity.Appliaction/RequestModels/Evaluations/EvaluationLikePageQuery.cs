using iSchool.Organization.Activity.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Activity.Appliaction.RequestModels
{
#nullable enable

    public class EvaluationLikePageQuery : IRequest<EvaluationLikePageResult>
    {
        public ActivityInfo ActivityInfo { get; set; } = default!;

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

#nullable disable
}
