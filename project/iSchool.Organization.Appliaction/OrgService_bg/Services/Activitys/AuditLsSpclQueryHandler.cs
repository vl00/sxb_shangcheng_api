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
    public class AuditLsSpclQueryHandler : IRequestHandler<AuditLsSpclQuery, AuditLsSpclQueryResult>
    {
        OrgUnitOfWork orgUnitOfWork;

        public AuditLsSpclQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            this.orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<AuditLsSpclQueryResult> Handle(AuditLsSpclQuery query, CancellationToken cancellation)
        {
            var result = new AuditLsSpclQueryResult();
            await default(ValueTask);

            var sql = $@"
select s.* from Activity a 
left join ActivityExtend ae on ae.activityid=a.id and ae.Type={ActivityExtendType.Special.ToInt()}
left join Special s on s.id=ae.contentid and s.status={SpecialStatusEnum.Ok.ToInt()} and s.type={SpecialTypeEnum.SmallSpecial.ToInt()}
where a.IsValid=1 and isnull(a.Status,{ActivityStatus.Ok.ToInt()})={ActivityStatus.Ok.ToInt()} and a.id=@ActivityId
and s.IsValid=1 
";
            var spcls = await orgUnitOfWork.DbConnection.QueryAsync<Special>(sql, query);
            result.Spcls = spcls.Select(_ => (_.Id, _.Title)).ToArray();

            return result;
        }
    }
}
