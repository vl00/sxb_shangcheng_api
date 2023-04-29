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

    /// <summary>
    /// 优秀案例(评测)
    /// </summary>
    public class ExcellentCasesEvltPageQuery : IRequest<ExcellentCasesEvltPageResult>
    {
        public ActivityInfo ActivityInfo { get; set; } = default!;
    }

#nullable disable
}
