using CSRedis;
using Dapper.Contrib.Extensions;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class BackGroundRefundCommandHandler : IRequestHandler<BackGroundRefundCommand, bool>
    {

        OrgUnitOfWork _orgUnitOfWork;
        private readonly ILock1Factory _lock1Factory;
        IMediator _mediator;
        ILogger<BackGroundRefundCommandHandler> _logger;
        ISxbGenerateNo _sxbGenerateNo;
        public BackGroundRefundCommandHandler(IOrgUnitOfWork orgUnitOfWork
            , ILock1Factory lock1Factory
            , IMediator mediator
            , ILogger<BackGroundRefundCommandHandler> logger
            , ISxbGenerateNo sxbGenerateNo)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _lock1Factory = lock1Factory;
            _mediator = mediator;
            _logger = logger;
            _sxbGenerateNo = sxbGenerateNo;
        }


        public async Task<bool> Handle(BackGroundRefundCommand request, CancellationToken cancellationToken)
        {
            await using var _lck = await _lock1Factory.LockAsync(CacheKeys.Refund_applyLck.FormatWith(request.OrderDetailId), 3 * 60 * 1000);
            if (!_lck.IsAvailable) throw new CustomResponseException("系统繁忙", Consts.Err.RefundApplyCheck_CannotGetLck);
            var orderDetail = await _orgUnitOfWork.QueryFirstOrDefaultAsync<OrderDetial>(@"SELECT * FROM OrderDetial WHERE id = @id", new { id = request.OrderDetailId });
            if (orderDetail == null) throw new KeyNotFoundException("找不到OrderDetail");

            //统计在审核过程中锁定的退款数量
            int applyRefundAuditCount = await _mediator.Send(new StaticApplyRefundAuditCountCommand() {  OrderDetailId = orderDetail.Id});
            if ((request.RefundCount + applyRefundAuditCount) > orderDetail.Number)
            {
                throw new Exception("退款数量溢出。");
            }
            var orderInfo = await _orgUnitOfWork.QueryFirstOrDefaultAsync<Domain.Order>(@"SELECT * FROM [Order] WHERE id=@orderId", new { orderId = orderDetail.Orderid });
            _orgUnitOfWork.BeginTransaction();
            try
            {
                var refundPrices =  orderDetail.RefundSpreadPrice(request.RefundCount);
                //创建后台退款类型-售后单（直接状态为退款成功）
                OrderRefunds orderRefund = new OrderRefunds()
                {
                    Id = Guid.NewGuid(),
                    Cause = 15,
                    Code = $"{Consts.Prev_RefundCode}{_sxbGenerateNo.GetNumber()}",
                    Count = (short)request.RefundCount,
                    CreateTime = DateTime.Now,
                    Creator = request.Auditor,
                    Modifier = request.Auditor,
                    ModifyDateTime = DateTime.Now,
                    Status = 5,
                    Type = 4,
                    IsValid = true,
                    OrderDetailId = request.OrderDetailId,
                    Price = refundPrices.Sum(s=>s.refundAmount),
                    RefundPrice = refundPrices.Sum(s => s.refundAmount),
                    SpecialReason = Domain.Enum.OrderRefundSpecialReason.Nothing,
                    OrderId = orderDetail.Orderid,
                    ProductId = orderDetail.Productid,
                    Reason = "后台直接退款。",
                    RefundTime = DateTime.Now,
                    RefundUserId = orderInfo.Userid,
                };
                await _orgUnitOfWork.DbConnection.InsertAsync(orderRefund, _orgUnitOfWork.DbTransaction);
                //更新OrderDetail的退款数量
                string increaseSql = @"Update OrderDetial SET RefundCount = ISNULL(RefundCount,0)+ @Count WHERE id = @OrderDetailId";
               bool  updateOrderDetailFlag = (await _orgUnitOfWork.ExecuteAsync(increaseSql, new { OrderDetailId = orderRefund.OrderDetailId,Count =orderRefund.Count }, _orgUnitOfWork.DbTransaction)) > 0;
                if (!updateOrderDetailFlag)
                {
                    throw new Exception("更新 OrderDetail失败。");
                }

                //执行退款操作。
                await _mediator.Send(new OrderRefundCommand()
                {
                    OrderId = orderRefund.OrderId,
                    ProductId = orderRefund.ProductId.Value,
                    OrderDetailId = orderRefund.OrderDetailId.Value,
                    RefundPrices = refundPrices,
                    AdvanceOrderId = orderInfo.AdvanceOrderId.Value,
                    Remark = "售后审核通过申请退款。"
                });
                _orgUnitOfWork.CommitChanges();
                //审核退款通过后续交由事件处理。
                try { await _mediator.Publish(new OrderRefundSuccessDomainEvent() { OrderRefundId = orderRefund.Id }); } catch { }
                return true;

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "后台退款异常，具体请查看详情信息。");
                _orgUnitOfWork.Rollback();
                return false;
            }

        }
    }
}
