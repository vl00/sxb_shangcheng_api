using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ViewModels.Supplier;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services.Supplier
{
    public class SupplierInfoByIdQueryHandler : IRequestHandler<SupplierInfoByIdQuery, SupplierInfo>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public SupplierInfoByIdQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }
        public Task<SupplierInfo> Handle(SupplierInfoByIdQuery request, CancellationToken cancellationToken)
        {
            string sql = $@" SELECT s.Id,s.Name,s.BankCardNo,s.BankAddress,s.IsPrivate,s.CompanyName,
                        ReturnAddress = (SELECT id, JSON_VALUE(ReturnAddress, '$.Receiver') Receiver,  JSON_VALUE(ReturnAddress, '$.Phone') Phone, JSON_VALUE(ReturnAddress, '$.Addr') Addr
                        from SupplierAddress where IsVaild = 1 and SupplierId = s.id ORDER BY Sort FOR JSON AUTO ),
                        s.Freight,s.BillingType 
                        FROM [Organization].[dbo].[Supplier] s  where s.IsValid=1 and s.id=@id; ";
            var data = _orgUnitOfWork.DbConnection.Query<Domain.Supplier>(sql, new DynamicParameters().Set("id", request.Id)).FirstOrDefault();

            string bindsql = $@" SELECT * FROM [Organization].[dbo].[SupplierBrand] where IsValid=1 and SupplierId=@id; ";
            var binddata = _orgUnitOfWork.DbConnection.Query<Domain.SupplierBrand>(bindsql, new DynamicParameters().Set("id", request.Id)).ToList();

            if (data == null)
            {
                return null;
            }

            SupplierInfo info = new SupplierInfo()
            {
                Id = data.Id,
                Name = data.Name,
                BankCardNo = data.BankCardNo,
                BankAddress = data.BankAddress,
                CompanyName = data.CompanyName,
                ReturnAddress = data.ReturnAddress,
                IsPrivate = data.IsPrivate,
                BillingType = data.BillingType,
                OrgIds = binddata.Select(q => q.OrgId).ToList()
            };

            return Task.FromResult(info);
        }
    }
}
