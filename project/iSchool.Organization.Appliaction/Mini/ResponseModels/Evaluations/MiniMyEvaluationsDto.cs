using iSchool.Infrastructure.Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 我的种草
    /// </summary>
    public class MiniMyEvaluationsDto
    {
        public PagedList<MiniEvaluationItemDto> PageInfo { get; set; } = default!;
    }
}
