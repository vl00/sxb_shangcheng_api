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
namespace iSchool.Organization.Appliaction.PC.ResponseModels
{
    /*
    // 需要在swagger里显示的类型不能同名字（即使不同namespace）, 否则swagger直接报错!!!
    public class EvaluationIndexQueryResult : PcEvaluationIndexQueryResult { }
    //*/
}

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class PcEvaluationIndexQueryResult : IPageMeInfo
    {
        /// <summary>分页信息</summary>
        public PagedList<EvaluationItemDto> PageInfo { get; set; } = default!;
        /// <summary>科目栏</summary>
        public IEnumerable<(string Name, string Id)>? Subjs { get; set; }
        /// <summary>用户(我)信息</summary>
        public IUserInfo? Me { get; set; }
        /// <summary>专题s</summary>
        public IEnumerable<SimpleSpecialDto>? Specials { get; set; }
        /// <summary>机构简单信息.</summary>
        public PcOrgItemDto? OrgInfo { get; set; }
    }
}
#nullable disable
