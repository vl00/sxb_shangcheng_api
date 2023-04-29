using iSchool.Infrastructure;
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
    /// <summary>
    /// 批量查询pc机构信息各统计数目
    /// </summary>
    public class PcGetOrgsCountsQueryResult : Dictionary<Guid, (int CourceCount, int EvaluationCount, int GoodsCount)>
    {        
    }
}
#nullable disable
