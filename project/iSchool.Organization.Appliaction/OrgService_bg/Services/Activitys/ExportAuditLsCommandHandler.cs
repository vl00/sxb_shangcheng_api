using AutoMapper;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
    public class ExportAuditLsCommandHandler : IRequestHandler<ExportAuditLsCommand, string>
    {
        IMediator _mediator;
        IMapper _mapper;
        IConfiguration _config;

        public ExportAuditLsCommandHandler(IMediator mediator, IMapper mapper, IConfiguration config)
        {
            this._mediator = mediator;
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<string> Handle(ExportAuditLsCommand cmd, CancellationToken cancellation)
        {
            var rr = await _mediator.Send(_mapper.Map<AuditLsPagerQuery>(cmd));
            var items = rr.PageInfo.CurrentPageItems;

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Sheet1");
            int row = 1, col = 1;
            {
                sheet.Cells[row, col++].Value = "序号";
                sheet.Cells[row, col++].Value = "评测名称";
                sheet.Cells[row, col++].Value = "发表时间";
                sheet.Cells[row, col++].Value = "类型";
                sheet.Cells[row, col++].Value = "单篇奖金";
                sheet.Cells[row, col++].Value = "额外奖金";
                sheet.Cells[row, col++].Value = "用户ID";
                sheet.Cells[row, col++].Value = "用户名";
                sheet.Cells[row, col++].Value = "手机号";
                sheet.Cells[row, col++].Value = "该手机号绑定账号数量";
                sheet.Column(col).Width = 25;
                sheet.Cells[row, col++].Value = "领取红包数量";
                sheet.Cells[row, col++].Value = "审核时间";
                sheet.Cells[row, col++].Value = "修改时间";
                sheet.Cells[row, col++].Value = "关联活动";
                sheet.Cells[row, col++].Value = "关联专题";
                sheet.Cells[row, col++].Value = "内容状态";
            }
            var i = 0;
            foreach (var item in items)
            {
                row++; col = 1;
                sheet.Row(row).Height = 15;
                // 序号
                sheet.Cells[row, col++].Value = $"{++i}";
                // 评测名称
                sheet.Cells[row, col++].Value = $"{item.Title}";
                // 发表时间
                sheet.Cells[row, col++].Value = $"{item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")}";
                // 类型
                sheet.Cells[row, col++].Value = $"{EnumUtil.GetDesc((ActiEvltSubmitType)item.SubmitType)}";
                // 单篇奖金
                sheet.Cells[row, col++].Value = $"{item.PriceForOneEvaluation ?? 0}";
                // 额外奖金
                sheet.Cells[row, col++].Value = item.PriceForExtraBonus == null ? "-" : $"{item.PriceForExtraBonus.Value.Item2.Sum()}";
                // 用户ID
                sheet.Cells[row, col++].Value = $"{item.UserId}";
                // 用户名
                sheet.Cells[row, col++].Value = $"{item.UserName}";
                // 手机号
                sheet.Cells[row, col++].Value = $"{item.Mobile}";
                // 该手机号绑定账号数量
                sheet.Cells[row, col++].Value = $"{item.UmacCount}";
                // 领取红包数量
                sheet.Cells[row, col++].Value = $"{item.UGetRedpCount}";
                // 审核时间
                sheet.Cells[row, col++].Value = $"{(item.AuditTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "")}";
                // 修改时间
                sheet.Cells[row, col++].Value = $"{(item.Mtime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "")}";
                // 关联活动
                sheet.Cells[row, col++].Value = $"{(item.ActiTitle ?? "")}";
                // 关联专题
                sheet.Cells[row, col++].Value = $"{(item.SpclTitle ?? "")}";
                // 内容状态
                sheet.Cells[row, col++].Value = $"{EnumUtil.GetDesc((ActiEvltAuditStatus)item.AebStatus)}";
            }

            var id = Guid.NewGuid().ToString("n");
            package.SaveAs(new FileInfo(Path.Combine(AppContext.BaseDirectory, _config["AppSettings:XlsxDir"], $"{id}.xlsx")));
            return id;
        }
    }
}
