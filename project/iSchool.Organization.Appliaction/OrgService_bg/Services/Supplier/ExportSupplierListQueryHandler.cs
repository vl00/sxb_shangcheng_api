using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ViewModels.Supplier;
using iSchool.Organization.Domain;
using MediatR;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Supplier
{
    public class ExportSupplierListQueryHandler : IRequestHandler<ExportSupplierListQuery, List<SupplierItem>>
    {

        OrgUnitOfWork _orgUnitOfWork;
        public ExportSupplierListQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<List<SupplierItem>> Handle(ExportSupplierListQuery request, CancellationToken cancellationToken)
        {
            var dy = new DynamicParameters();
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

            string sql = $@" select ROW_NUMBER() over(order by CreateTime desc) as rownum,* from 
                                 (
                                    SELECT s.id,s.name,s.BankCardNo,s.BankAddress,s.CompanyName ,case when s.IsPrivate = '1' then '是' else '否' end IsPrivate,
                                    ReturnAddress =  STUFF((SELECT  ';' + JSON_VALUE(ReturnAddress, '$.Receiver') + ','+  JSON_VALUE(ReturnAddress, '$.Phone') + ','+  JSON_VALUE(ReturnAddress, '$.Addr')
                                    from SupplierAddress where IsVaild = 1 and SupplierId = s.id ORDER BY Sort FOR XML PATH('')), 1, 1, '' ),
                                    (SELECT count(1) from SupplierAddress where IsVaild = 1 and SupplierId = s.id) ReturnAddressCount,
                                    orgname = STUFF((SELECT ';' + o.name
                                    FROM Organization o
                                    LEFT JOIN SupplierBrand bind on bind.OrgId = o.id and bind.IsValid = 1
                                    WHERE bind.SupplierId = s.id FOR XML PATH('')), 1, 1, '')  ,
                                    case s.BillingType when 0 then '日结' when 1 then '周结' when 2 then '月结' end  as BillingType,
                                    s.CreateTime
                                    FROM Supplier s
                                    LEFT JOIN SupplierBrand bind on s.id = bind.SupplierId and bind.IsValid = 1
                                    LEFT JOIN Organization o on bind.OrgId = o.id
                                    where s.IsValid = 1 {where}
                                    GROUP BY s.id,s.name,s.BankCardNo,s.BankAddress,s.CompanyName,s.IsPrivate,s.BillingType,s.CreateTime
                                )t1
                        ;";
            var items = _orgUnitOfWork.DbConnection.Query<SupplierItem>(sql, dy).ToList();



            //IEnumerable<(string r1c, Func<int, int, dynamic, object> wr)> Wcell()
            //{
            //    yield return ("序号", (row, col, data) => data.RowNum?.ToString() ?? "");
            //    yield return ("供应商名称", (row, col, data) => data.Name);
            //    yield return ("供应商对公账号", (row, col, data) => data.BankCardNo);
            //    yield return ("开户行", (row, col, data) => data.BankAddress);
            //    yield return ("供应商对公账户名称（公司名称）", (row, col, data) => data.CompanyName);
            //    yield return ("是否私人", (row, col, data) => data.IsPrivate);
            //    yield return ("供应商退货地址", (row, col, data) => data.ReturnAddress);
            //    yield return ("相关品牌", (row, col, data) => data.OrgName);
            //}
            //var stream = new System.IO.MemoryStream();

            //using var package = new ExcelPackage(stream);
            //{
            //    var sheet = package.Workbook.Worksheets.Add("Sheet1");
            //    int row = 1, col = 1;
            //    foreach (var (r1c, _) in Wcell())
            //    {
            //        sheet.Cells[row, col++].Value = r1c;
            //    }
            //    foreach (var item in items)
            //    {
            //        row++; col = 1;
            //        foreach (var (_, wr) in Wcell())
            //        {
            //            sheet.Cells[row, col].Value = wr(row, col, item)?.ToString();
            //            col++;
            //        }
            //    }
            //    package.Save();
            //}
            return Task.FromResult(items);
        }
    }
}
