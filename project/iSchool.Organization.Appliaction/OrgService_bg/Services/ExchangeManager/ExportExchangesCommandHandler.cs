using AutoMapper;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.OrgService_bg.Course;
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

namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{
    /// <summary>
    /// 导出兑换记录
    /// </summary>
    public class ExportExchangesCommandHandler : IRequestHandler<ExportExchangesCommand , string>
    {
        IMediator _mediator;
        IMapper _mapper;
        IConfiguration _config;
        OrgUnitOfWork _orgUnitOfWork;

        public ExportExchangesCommandHandler(IMediator mediator, IMapper mapper, IOrgUnitOfWork orgUnitOfWork,
            IConfiguration config)
        {
            this._mediator = mediator;
            this._mapper = mapper;
            this._config = config;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<string> Handle(ExportExchangesCommand  cmd, CancellationToken cancellation)
        {            
            IEnumerable<(string r1c, Func<int, int, dynamic, object> wr)> Wcell()
            {
                
                yield return ("发送时间", (row, col, data) => data.CreateTime);
                yield return ("兑换码", (row, col, data) => data.Code);
                yield return ("发送人", (row, col, data) => data.UserName);
                yield return ("订单号", (row, col, data) => data.OrderCode);
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

        async Task<IEnumerable<dynamic>> GetDatas(ExportExchangesCommand  cmd)
        {
            var request = new SearchExchangesQuery()
            {
                CourseId = cmd.CourseId,
                PageIndex = 1,
                PageSize = 999999999
            };         
            var datas = _mediator.Send(request).Result.CurrentPageItems.AsArray();
            return datas;
        }

    }
}
