using iSchool.Infrastructure.Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{

    /// <summary>
    /// 首页宝妈精选列表
    /// </summary>
    public class MiniIndexEvalts
    {

        public PagedList<MiniEvaluationItemDto> PageInfo { get; set; } = default!;

    }
}
