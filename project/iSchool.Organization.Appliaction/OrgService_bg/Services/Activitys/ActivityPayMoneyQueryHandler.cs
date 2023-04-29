using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
    public class ActivityPayMoneyQueryHandler : IRequestHandler<ActivityPayMoneyQuery, ActivityPayMoneyQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;

        public ActivityPayMoneyQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<ActivityPayMoneyQryResult> Handle(ActivityPayMoneyQuery query, CancellationToken cancellation)
        {
            var result = new ActivityPayMoneyQryResult();
            await default(ValueTask);

            var sql = $@"
select a.Budget from Activity a where a.IsValid=1 and a.id=@ActivityId
;
select sum([Money]) from ActivityEvalMoneyOrder where IsValid=1 and Activityid=@Activityid and [orderstatus]%3=0 
";
            var gr = await _orgUnitOfWork.DbConnection.QueryMultipleAsync(sql, query);
            result.Budget = (await gr.ReadFirstOrDefaultAsync<decimal?>()) ?? 0;
            result.PayOutMoney = (await gr.ReadFirstOrDefaultAsync<decimal?>() ?? 0);

            return result;
        }
    }
}
