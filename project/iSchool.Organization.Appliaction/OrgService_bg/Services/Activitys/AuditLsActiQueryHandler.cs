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
    public class AuditLsActiQueryHandler : IRequestHandler<AuditLsActiQuery, AuditLsActiQueryResult>
    {
        OrgUnitOfWork orgUnitOfWork;

        public AuditLsActiQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            this.orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<AuditLsActiQueryResult> Handle(AuditLsActiQuery query, CancellationToken cancellation)
        {
            var result = new AuditLsActiQueryResult();
            await default(ValueTask);

            var sql = $@"
select a.* from Activity a where a.IsValid=1 and isnull(a.Status,{ActivityStatus.Ok.ToInt()})={ActivityStatus.Ok.ToInt()}
{"and a.type=@Type".If(query.Type != null)}
";
            sql = $@"
select count(1) from ({sql}) T
;
{sql}
order by a.CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var gr = await orgUnitOfWork.DbConnection.QueryMultipleAsync(sql, query);
            var cc = await gr.ReadFirstAsync<int>();
            var items = (await gr.ReadAsync<Activity>()).ToArray();
            result.Activitys = items.ToPagedList(query.PageSize, query.PageIndex, cc);

            return result;
        }
    }
}
