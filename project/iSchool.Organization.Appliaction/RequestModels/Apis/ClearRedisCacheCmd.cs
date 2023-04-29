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
    /// del redis cache
    /// </summary>
    public class ClearRedisCacheCmd : IRequest
    {
        /// <summary>normal keys or their patterns</summary>
        public IEnumerable<string> Keys { get; set; } = null!;
        /// <summary>normal (keys and hkey-filed) or their patterns</summary>
        public IEnumerable<(string, string?)> Keys2 { get; set; } = null!;

        public int ExecSec = 60;

        public int WaitSec { get; set; } = 3;
    }

#nullable disable
}
