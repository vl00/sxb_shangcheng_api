using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{
    public class ExcelImportOrderOutdepotStatusCommandHandler : IRequestHandler<ExcelImportOrderOutdepotStatusCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public ExcelImportOrderOutdepotStatusCommandHandler(IOrgUnitOfWork orgUnitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
            _redisClient = redisClient;
        }

        public Task<ResponseResult> Handle(ExcelImportOrderOutdepotStatusCommand request, CancellationToken cancellationToken)
        {
            using (var package = new ExcelPackage())
            {
                package.Load(request.Excel);

                var worksheet = package.Workbook.Worksheets.First(); //sheet1

                var ls = new List<Domain.Order>();
                var ods = new List<OrderDetial>();
                for (var i = 2; i <= worksheet.Dimension.Rows; i++)
                {
                    if (string.IsNullOrWhiteSpace( worksheet.Cells[$"A{i}"].Value?.ToString()))
                    {
                        continue;
                    }

                    //订单号
                    var code = worksheet.Cells[$"A{i}"].Value?.ToString();
                    //订单detailId
                    var detailId = Guid.Parse(worksheet.Cells[$"B{i}"].Value?.ToString());

                    var order = new Domain.Order();
                    order.Code = code;
                    order.Status = 301;
                    order.Modifier = request.UserId;
                    order.ModifyDateTime = DateTime.Now;
                    ls.Add(new Domain.Order
                    {
                        Code = code,
                        Status = 301,
                        Modifier = request.UserId,
                        ModifyDateTime = DateTime.Now
                    });

                    ods.Add(new OrderDetial
                    {
                        Id = detailId,
                        Status = 301
                    });

                }


                //检查有问题订单
                string sql = $@"select od.id,
                        case when oo.id is null then 1 else 0 end isNotExport,
                        case when (select count(1) from OrderRefunds where OrderDetailId = od.id and Status in (1,2,3,4,5) )>0 then 1 else 0 end isRefund,
                        case when od.status != 103 then 1 else 0 end isOutdepoted
                        from OrderDetial od 
                        LEFT JOIN OrderOutdepot oo on oo.id = od.id
                        where od.id in @Ids;";
                var result = _orgUnitOfWork.Query<UnreliableOrder>(sql, new { Ids = ods.Select(q=>q.Id).ToArray() } ).ToList();

                var resultId = result.Select(q => q.Id);
                

                if (ods.Where(q => !resultId.Contains(q.Id)).Any() || result.Where(q=>q.IsNotExport).Any() || result.Where(q => q.IsRefund).Any() || result.Where(q => q.IsOutdepoted).Any())
                {
                    List<Object> UnreliableOrderResult = new List<Object>();

                    if(ods.Where(q => !resultId.Contains(q.Id)).Any())
                    {
                        UnreliableOrderResult.Add(new
                        {
                            Result = "不存在的订单",
                            Ids = ods.Where(q => !resultId.Contains(q.Id)).Select(q=>q.Id).ToList()
                        });
                    }
                    if (result.Where(q => q.IsNotExport).Any())
                    {
                        UnreliableOrderResult.Add(new
                        {
                            Result = "尚未导出过邮件清单的订单",
                            Ids = result.Where(q => q.IsNotExport).Select(q => q.Id).ToList()
                        });
                    }
                    result.RemoveAll(q => q.IsNotExport);

                    if (result.Where(q =>  q.IsRefund).Any())
                    {
                        UnreliableOrderResult.Add(new {
                            Result = "申请退款或已退款的订单",
                            Ids = result.Where(q => q.IsRefund).Select(q => q.Id).ToList()
                        });
                    }
                    result.RemoveAll(q => q.IsRefund);

                    if (result.Where(q => q.IsOutdepoted).Any())
                    {
                        UnreliableOrderResult.Add(new
                        {
                            Result = "已经是出库状态，请勿重复导入",
                            Ids = result.Where(q => q.IsOutdepoted).Select(q => q.Id).ToList()
                        });
                    }
                    return Task.FromResult(ResponseResult.Failed("导入失败", UnreliableOrderResult));
                }
                try
                {
                    _orgUnitOfWork.BeginTransaction();
                    if (ls.Any())
                    {
                        foreach (var order in ls)
                        {
                            string sql1 = $@"
                            update [dbo].[Order] set 
                            Status=@Status,Modifier= @Modifier  ,ModifyDateTime=GETDATE()
                            where id in (
                            select oo.orderid 
                            from OrderOutdepot oo
                            INNER JOIN [dbo].[Order] o on o.id = oo.orderID
                            where o.IsValid=1 and o.code=@code
                            );";
                            _orgUnitOfWork.DbConnection.Execute(sql1, new DynamicParameters()
                            .Set("Modifier", order.Modifier)
                            .Set("code", order.Code)
                            .Set("Status", order.Status)
                            , _orgUnitOfWork.DbTransaction);
                        }
                        foreach (var orderDetail in ods)
                        {
                            string sql2 = $@" update [dbo].[OrderDetial] set  Status=@Status where id in (
                             select oo.id from OrderOutdepot oo
	                            INNER JOIN [dbo].[OrderDetial] o on o.id = oo.id
	                            where o.id=@id
                             ); ";
                            _orgUnitOfWork.DbConnection.Execute(sql2, new DynamicParameters()
                            .Set("id", orderDetail.Id)
                            .Set("Status", orderDetail.Status)
                            , _orgUnitOfWork.DbTransaction);
                        }
                    }
                    _orgUnitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    _orgUnitOfWork.Rollback();
                    throw ex;
                }

                return Task.FromResult(ResponseResult.Success("导入成功"));
            }
        }

        private class UnreliableOrder
        {
            public Guid Id { get; set; }
            public bool IsNotExport { get;set; }//未导出过
            public bool IsRefund { get; set; }//已退款
            public bool IsOutdepoted { get; set; }//已修改为出库
        }
    }
}
