using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class PcSpecialIndexQueryResult : IPageMeInfo
    {
        /// <summary>当前专题信息</summary>
        public SimpleSpecialDto? CurrSpecial { get; set; }
        /// <summary>分页信息</summary>
        public PagedList<EvaluationItemDto> PageInfo { get; set; } = default!;
        /// <summary>用户(我)信息</summary>
        public IUserInfo? Me { get; set; }
        /// <summary>专题s</summary>
        public IEnumerable<SimpleSpecialDto>? Specials { get; set; }
    }
}
#nullable disable
