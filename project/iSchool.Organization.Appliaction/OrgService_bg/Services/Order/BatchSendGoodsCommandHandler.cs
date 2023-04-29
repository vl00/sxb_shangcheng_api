using iSchool.Organization.Appliaction.RequestModels.Orders;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Infrastructure;
using iSchool.Organization.Domain;
using System.Linq;
using iSchool.Organization.Domain.Enum;
using Dapper;

namespace iSchool.Organization.Appliaction.OrgService_bg.Order
{
    /// <summary>
    /// 批量一键发货
    /// </summary>
    public class BatchSendGoodsCommandHandler : IRequestHandler<BatchSendGoodsCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public BatchSendGoodsCommandHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }
        public async Task<ResponseResult> Handle(BatchSendGoodsCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            try
            {
                if (request.OrderIds.Any())
                {
                    var ids = string.Join("','", request.OrderIds);
                    string batchsql = $@" update [dbo].[Order] set 
[status]=@status,  [ModifyDateTime]=@time, [Modifier]=@Modifier,  [ShippingTime]=@time
where IsValid=1 and id in ('{ids}') ;
update [dbo].[OrderDetial] set status=@status  where orderid in('{ids}');
";
                    _orgUnitOfWork.DbConnection.Execute(batchsql,new DynamicParameters()
                        .Set("status", OrderStatusV2.Shipping.ToInt())
                        .Set("time", DateTime.Now)
                        .Set("Modifier", request.UserId)
                        );                    
                }
                return ResponseResult.Success("操作成功");
            }
            catch(Exception ex)
            {
                return ResponseResult.Failed($"系统错误：【{ex.Message}】");
            }
           
        }
    }
}
