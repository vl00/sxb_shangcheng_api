using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Imports
{
    public class BatchImportSendGoodsCmdHandler : IRequestHandler<BatchImportSendGoodsCmd, BatchImportSendGoodsCmdResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public BatchImportSendGoodsCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<BatchImportSendGoodsCmdResult> Handle(BatchImportSendGoodsCmd cmd, CancellationToken cancellation)
        {
            var result = new BatchImportSendGoodsCmdResult();
            IDisposable disposable = null;
            await default(ValueTask);

            ExcelWorksheet sheet = null;
            try
            {
                var package = new ExcelPackage(cmd.ExcelStream);
                disposable = package;
                sheet = package.Workbook.Worksheets["Sheet1"];
            }
            catch { }
            if (sheet == null)
            {
                result.Errs.Add("文档错误.找不到Sheet1.请使用正确的模板进行导入");
                return result;
            }
            using var _disposable_ = disposable;

            var iCode = FindXlsxRowValue(sheet, 1, "子订单号").Col;
            var iCount = FindXlsxRowValue(sheet, 1, "数量(拆分发货)").Col;
            var iPropNames = FindXlsxRowValue(sheet, 1, "具体购买").Col;
            var iNu = FindXlsxRowValue(sheet, 1, "快递单号").Col;
            var iCom = FindXlsxRowValue(sheet, 1, "快递公司-编号").Col;         
            var iOrderDetailId = FindXlsxRowValue(sheet, 1, "OrderDetailId").Col;
            if ((-1).In(iCode, iCount, iNu, iCom, iOrderDetailId))
            {
                result.Errs.Add("文档错误.找不到对应的字段.请使用正确的模板进行导入");
                goto LB_end;
            }

            var lsOrderDetails = new List<Domain.OrderDetial>();
            var lsAddCmds = new List<AddOrderLogisticsCommand>();
            for (var i = 2; i <= sheet.Dimension.Rows; i++)
            {
                if (CheckIfEmptyRow(sheet, i))
                {
                    continue;
                }
                var err = "";
                var orderNo = sheet.Cells[i, iCode].Value?.ToString()?.Trim();
                var count = int.TryParse(sheet.Cells[i, iCount].Value?.ToString()?.Trim(), out var _count) ? _count : 0;
                var propNames = sheet.Cells[i, iPropNames].Value?.ToString()?.Trim().Split('|', StringSplitOptions.RemoveEmptyEntries);
                var nu = sheet.Cells[i, iNu].Value?.ToString()?.Trim();
                var com = GetCom(sheet.Cells[i, iCom].Value?.ToString()?.Trim());
                var orderDetailId = Guid.TryParse(sheet.Cells[i, iOrderDetailId].Value?.ToString()?.Trim(), out var _odid) ? _odid : default;
                if (orderNo.IsNullOrEmpty())
                {
                    err += "订单号不能为空";
                }
                if (count <= 0)
                {
                    err += "\n数量不能小于1";
                }
                //if (propNames == null || propNames.Length == 0)
                //{
                //    err += "\n具体购买不能为空";
                //}
                if (nu.IsNullOrEmpty())
                {
                    err += "\n快递单号不能为空";
                }
                if (com.IsNullOrEmpty())
                {
                    err += "\n快递公司编号错误";
                }
                if (orderDetailId == default)
                {
                    err += "\norderDetailId不能为空";
                }
                if (!err.IsNullOrEmpty())
                {
                    result.Errs.Add($"第{i}行: {err}\n");
                    continue;
                }
                // check kd nu com
                {
                    var kdcom = (await _mediator.Send(KuaidiServiceArgs.CheckNu(nu, com))).GetResult<KdCompanyCodeDto>();
                    if (kdcom == null)
                    {
                        result.Errs.Add($"第{i}行: 快递单号或快递公司错误, 或快递单号与快递公司不匹配。");
                        continue;
                    }
                }
                try
                {
                    var sql = "select * from [order] o where o.IsValid=1 and o.type>=2 and o.code=@orderNo";
                    var order = await _orgUnitOfWork.QueryFirstOrDefaultAsync<Domain.Order>(sql, new { orderNo });
                    if (order == null)
                    {
                        result.Errs.Add($"第{i}行: 找不到该订单'{orderNo}'");
                        continue;
                    }
                    sql = "select * from [OrderDetial] d where d.orderid=@Id ";
                    var orderDetails = await _orgUnitOfWork.QueryAsync<Domain.OrderDetial>(sql, new { order.Id });
                    //var orderDetail = orderDetails.FirstOrDefault(od =>
                    //{
                    //    string[] arr = null;
                    //    try { arr = JObject.Parse(od.Ctn)["propItemNames"].ToObject<string[]>(); } catch { }
                    //    if (arr == null) return false;
                    //    return propNames.Any(a => a.In(arr));
                    //});
                    var orderDetail = orderDetails.FirstOrDefault(od => od.Id == orderDetailId);
                    if (orderDetail == null)
                    {
                        result.Errs.Add($"第{i}行: 找不到该订单'{orderNo}'的OrderDetailId='{orderDetailId}'");
                        continue;
                    }
                    if (!lsOrderDetails.Any(_ => _.Id == orderDetail.Id))
                    {
                        lsOrderDetails.Add(orderDetail);
                    }
                    if (!orderDetail.Status.In(OrderStatusV2.Paid.ToInt(), OrderStatusV2.ExWarehouse.ToInt()))
                    {
                        // orderDetail.Status
                        result.Errs.Add($"第{i}行: 当前订单状态不能发货");
                        continue;
                    }
                    //
                    var addcmd = lsAddCmds.FirstOrDefault(_ => _.OrderDetailId == orderDetail.Id);
                    if (addcmd == null)
                    {
                        lsAddCmds.Add(addcmd = new AddOrderLogisticsCommand
                        {
                            OrderId = orderDetail.Orderid,
                            OrderDetailId = orderDetail.Id,
                            UserId = cmd.UserId,
                            OrderLogistics = new List<OrderLogisticsData>()
                        });
                    }
                    if (count + addcmd.OrderLogistics.Sum(_ => _.Number) > orderDetail.Number - (orderDetail.RefundCount ?? 0) - (orderDetail.ReturnCount ?? 0))
                    {
                        result.Errs.Add($"第{i}行: 发货数量不能大于商品数量");
                        continue;
                    }
                    var oll = addcmd.OrderLogistics.FirstOrDefault(_ => _.ExpressCode == nu && _.ExpressType == com);
                    if (oll != null) oll.Number += (short)count;
                    else
                    {
                        addcmd.OrderLogistics.Add(oll = new OrderLogisticsData
                        {
                            ExpressCode = nu, ExpressType = com,
                            Number = (short)count,
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.Errs.Add($"第{i}行意外失败,请重新尝试.\n{ex.Message}");
                }
            }
            // 不能部分发货
            {
                foreach (var addcmd in lsAddCmds)
                {
                    var orderDetail = lsOrderDetails.FirstOrDefault(_ => _.Id == addcmd.OrderDetailId);
                    if (orderDetail == null) continue;
                    if (addcmd.OrderLogistics.Sum(_ => _.Number) != orderDetail.Number - (orderDetail.RefundCount ?? 0) - (orderDetail.ReturnCount ?? 0))
                    {
                        result.Errs.Add($"导入不支持部分发货,'{orderDetail.Id}'必须完全发货");
                    }
                }
            }

            // 导入
            if (!result.Errs.Any())
            {
                foreach (var addcmd in lsAddCmds)
                {
                    try { await _mediator.Send(addcmd); }
                    catch (CustomResponseException ex)
                    {
                        result.Errs.Add($"'{addcmd.OrderDetailId}'导入失败: {ex.Message}");
                    }
                }
            }

            LB_end:
            return result;
        }

        static string GetCom(string com)
        {
            if (string.IsNullOrEmpty(com)) return null;
            var i = com.IndexOf('-');
            return i > -1 ? com[(i + 1)..] : null;
        }

        public static (object Value, int Col) FindXlsxRowValue(ExcelWorksheet worksheet, int row, string field)
        {
            var col = -1;
            for (var i = 1; i <= worksheet.Dimension.Columns; i++)
            {
                var cv = worksheet.Cells[1, i].Value?.ToString()?.Trim();
                if (string.Equals(cv, field, StringComparison.OrdinalIgnoreCase))
                {
                    col = i;
                    break;
                }
            }
            if (col == -1) return (null, -1);
            return (worksheet.Cells[row, col].Value, col);
        }

        internal static bool CheckIfEmptyRow(ExcelWorksheet worksheet, int row, Func<object, bool> func = null)
        {
            if (row < 1 || row > worksheet.Dimension.Rows) return true;
            for (var col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                switch (func)
                {
                    case null when worksheet.Cells[row, col].Value?.ToString() is string s && !string.IsNullOrWhiteSpace(s):
                        return false;
                    case null:
                        break;
                    default:
                        if (!func(worksheet.Cells[row, col].Value)) return false;
                        else break;
                }
            }
            return true;
        }
    }
}
