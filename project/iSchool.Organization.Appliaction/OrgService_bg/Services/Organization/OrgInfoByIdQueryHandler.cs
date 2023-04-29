using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Domain;
using MediatR;

namespace iSchool.Organization.Appliaction.OrgService_bg.Organization
{
    /// <summary>
    /// 根据机构Id，获取机构信息
    /// </summary>
    public class OrgInfoByIdQueryHandler : IRequestHandler<OrgInfoByIdQuery, Domain.Organization>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public OrgInfoByIdQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<Domain.Organization> Handle(OrgInfoByIdQuery request, CancellationToken cancellationToken)
        {
            string sql = $@" SELECT * FROM [Organization].[dbo].[Organization] where IsValid=1 and id=@id; ";
            var data = _orgUnitOfWork.DbConnection.Query<Domain.Organization>(sql, new DynamicParameters().Set("id",request.Id)).FirstOrDefault();
            return Task.FromResult(data);
        }
    }
}
