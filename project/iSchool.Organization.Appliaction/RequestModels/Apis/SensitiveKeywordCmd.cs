using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 敏感词query
    /// </summary>
    public class SensitiveKeywordCmd : IRequest<SensitiveKeywordCmdResult>
    {
        public string? Txt { get; set; }

        public string[]? Txts { get; set; }
    }

#nullable disable
}
