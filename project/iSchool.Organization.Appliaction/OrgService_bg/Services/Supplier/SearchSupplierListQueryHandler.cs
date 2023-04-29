using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ViewModels.Supplier;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Supplier
{
    public class SearchSupplierListQueryHandler : IRequestHandler<SearchSupplierListQuery, PagedList<SupplierItem>>
    {

        OrgUnitOfWork _orgUnitOfWork;
        public SearchSupplierListQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<PagedList<SupplierItem>> Handle(SearchSupplierListQuery request, CancellationToken cancellationToken)
        {
            var dy = new DynamicParameters();
            dy.Add("@SkipCount", (request.PageIndex - 1) * request.PageSize);

            string where = "";

            //供应商名称模糊查询
            if (!string.IsNullOrEmpty(request.Name))
            {
                where += $@"   and s.Name like @Name ";
                dy.Add("@Name", $"%{request.Name}%");
            }
            //供应商对公账户模糊查询
            if (!string.IsNullOrEmpty(request.BankCardNo))
            {
                where += $@"   and s.BankCardNo like @BankCardNo ";
                dy.Add("@BankCardNo", $"%{request.BankCardNo}%");
            }
            //是否私人
            if (request.IsPrivate != null)
            {
                where += @"   and s.IsPrivate=@IsPrivate  ";
                dy.Add("@IsPrivate", request.IsPrivate);
            }

            //品牌
            if (request.OrganizationIds != null && request.OrganizationIds.Any())
            {
                where += @"   and bind.OrgId IN @OrganizationIds  ";
                dy.Add("@OrganizationIds", request.OrganizationIds);
            }

            //结算方式
            if (request.BillingType != null)
            {
                where += @"   and s.BillingType=@BillingType  ";
                dy.Add("@BillingType", request.BillingType);
            }

            string sql = $@" 
                            select top {request.PageSize} * from 
                            (
                                 select ROW_NUMBER() over(order by CreateTime desc) as rownum,* from 
                                 (
                                    SELECT s.id,s.name,s.BankCardNo,s.BankAddress,s.CompanyName,s.IsPrivate,
                                    ReturnAddress = (SELECT JSON_VALUE(ReturnAddress, '$.Receiver') + ','+  JSON_VALUE(ReturnAddress, '$.Phone') + ','+  JSON_VALUE(ReturnAddress, '$.Addr') addr
                                    from SupplierAddress where IsVaild = 1 and SupplierId = s.id ORDER BY Sort FOR JSON AUTO ),
                                    (SELECT count(1) from SupplierAddress where IsVaild = 1 and SupplierId = s.id) ReturnAddressCount,
                                    orgname = (SELECT o.name
                                    FROM Organization o
                                    LEFT JOIN SupplierBrand bind on bind.OrgId = o.id and bind.IsValid = 1
                                    WHERE bind.SupplierId = s.id FOR JSON AUTO) ,
                                    case s.BillingType when 0 then '日结' when 1 then '周结' when 2 then '月结' end as BillingType,
                                    s.CreateTime
                                    FROM Supplier s
                                    LEFT JOIN SupplierBrand bind on s.id = bind.SupplierId and bind.IsValid = 1
                                    LEFT JOIN Organization o on bind.OrgId = o.id
                                    where s.IsValid = 1 {where}
                                    GROUP BY s.id,s.name,s.BankCardNo,s.BankAddress,s.CompanyName,s.IsPrivate,s.BillingType,s.CreateTime
                                )t1
                            )TT
                            Where rownum>@SkipCount order by rownum 
                        ;";
            string countSql = $@" SELECT count(DISTINCT s.id)
                                    FROM Supplier s
                                    LEFT JOIN SupplierBrand bind on s.id = bind.SupplierId and bind.IsValid = 1
                                    LEFT JOIN Organization o on bind.OrgId = o.id
                                    where s.IsValid = 1   {where}                            
                             ;";
            var totalItemCount = _orgUnitOfWork.DbConnection.Query<int>(countSql, dy).FirstOrDefault();
            var items = _orgUnitOfWork.DbConnection.Query<SupplierItem>(sql, dy).ToList();

            var data = items.ToPagedList(request.PageSize, request.PageIndex, totalItemCount);

            return Task.FromResult(data);
        }
    }
}
