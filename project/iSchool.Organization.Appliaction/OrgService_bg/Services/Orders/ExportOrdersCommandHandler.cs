using AutoMapper;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{
    public class ExportOrdersCommandHandler : IRequestHandler<ExportOrdersCommand, string>
    {
        IMediator _mediator;
        IMapper _mapper;
        IConfiguration _config;
        OrgUnitOfWork _orgUnitOfWork;

        public ExportOrdersCommandHandler(IMediator mediator, IMapper mapper, IOrgUnitOfWork orgUnitOfWork,
            IConfiguration config)
        {
            this._mediator = mediator;
            this._mapper = mapper;
            this._config = config;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<string> Handle(ExportOrdersCommand cmd, CancellationToken cancellation)
        {            
            IEnumerable<(string r1c, Func<int, int, dynamic, object> wr)> Wcell()
            {
                yield return ("用户id", (row, col, data) => data.Userid?.ToString() ?? "");
                yield return ("订单编号", (row, col, data) => data.OrderNo);
                yield return ("订单时间", (row, col, data) => data.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                yield return ("订单状态", (row, col, data) => ((OrderStatusV2)data.Status).GetDesc());
                yield return ("课程名称", (row, col, data) => data.Ctn.title ?? data.CourseName1);
                yield return ("金额", (row, col, data) => data.TotalPayment);
                yield return ("机构名称", (row, col, data) => data.Ctn.orgName ?? data.OrgName1);
                yield return ("下单人(收货人)", (row, col, data) => data.RecvUsername);
                yield return ("电话", (row, col, data) => data.Mobile);
                yield return ("省", (row, col, data) => data.RecvProvince);
                yield return ("市", (row, col, data) => data.RecvCity);
                yield return ("区", (row, col, data) => data.RecvArea);
                yield return ("地址", (row, col, data) => data.Address);
                yield return ("年龄", (row, col, data) => data.Age);
                yield return ("属性", (row, col, data) => string.Join(" ", ((JToken)data.Ctn.propItemNames)?.ToObject<string[]>() ?? new string[0]));
            }

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Sheet1");
            int row = 1, col = 1;
            foreach (var (r1c, _) in Wcell())
            {
                sheet.Cells[row, col++].Value = r1c;
            }

            var items = await GetDatas(cmd);
            foreach (var item in items)
            {
                row++; col = 1;
                foreach (var (_, wr) in Wcell())
                {
                    sheet.Cells[row, col].Value = wr(row, col, item)?.ToString();
                    col++;
                }
            }

            var id = Guid.NewGuid().ToString("n");
            package.SaveAs(new FileInfo(Path.Combine(AppContext.BaseDirectory, _config["AppSettings:XlsxDir"], $"{id}.xlsx")));
            return id;
        }

        async Task<IEnumerable<dynamic>> GetDatas(ExportOrdersCommand cmd)
        {
            var sql = $@"
select 
c.title as CourseName1,org.name as OrgName1,
o.TotalPayment,
o.RecvUsername,o.Mobile,o.RecvProvince,o.RecvCity,o.RecvArea,o.Address,o.Age,p.Ctn as Ctn0,
o.Userid,o.code as OrderNo, o.CreateTime, p.Status
from [order] o
left join OrderDetial p on p.orderid=o.id --and p.producttype={ProductType.Course.ToInt()}
left join Course c on c.id=p.productid and c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()}
left join Organization org on org.id=c.orgid and org.IsValid=1 and org.status={OrganizationStatusEnum.Ok.ToInt()}
where o.IsValid=1 and o.type={OrderType.BuyCourseByWx.ToInt()}
{"and o.CreateTime>=@StartTime".If(cmd.StartTime != null)} {"and o.CreateTime<@EndTime".If(cmd.EndTime != null)}
order by o.CreateTime desc
";
            var datas = (await _orgUnitOfWork.DbConnection.QueryAsync(sql, new 
            {
                StartTime = cmd.StartTime != null ? cmd.StartTime.Value.Date : (DateTime?)null,
                EndTime = cmd.EndTime != null ? (cmd.EndTime.Value.AddDays(1).Date) : (DateTime?)null,
            })).AsArray();

            foreach (var data in datas)
            {
                var ctn0 = data.Ctn0?.ToString();
                if (string.IsNullOrEmpty(ctn0)) data.Ctn = new JObject();
                else data.Ctn = JObject.Parse(ctn0);
            }
            return datas;
        }

    }
}
