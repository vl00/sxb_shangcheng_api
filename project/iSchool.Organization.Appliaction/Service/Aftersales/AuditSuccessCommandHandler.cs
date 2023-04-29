using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Appliaction.RequestModels.WeChatNotification;
using iSchool.Organization.Appliaction.Service.WeChatNotification;
using iSchool.Organization.Appliaction.ViewModels.Aftersales;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class AuditSuccessCommandHandler : IRequestHandler<AuditSuccessCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        ILogger<AuditSuccessCommandHandler> _logger;
        public AuditSuccessCommandHandler(IOrgUnitOfWork orgUnitOfWork
            , IMediator mediator
            , ILogger<AuditSuccessCommandHandler> logger)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<bool> Handle(AuditSuccessCommand request, CancellationToken cancellationToken)
        {

            //更新售后记录状态。
            //退款/换货状态  
            //1. 提交申请  2.平台审核(发货) 3.平台审核(未发货)   4.平台退款  5.退款成功  6.审核失败
            //11.提交申请   12.平台审核   13.审核失败   14.寄回商品  15平台收货  16.验货失败   17.退款成功
            var orderRefund = _orgUnitOfWork.QueryFirstOrDefault<OrderRefunds>("SELECT * FROM OrderRefunds WHERE Id = @id", new { id = request.Id });
            if (orderRefund == null) throw new KeyNotFoundException("找不到该售后记录");
            if (orderRefund.Type == 1)
            {
                if (orderRefund.Status == 2 || orderRefund.Status == 3)
                {
                  return await RefundAuditSuccessHandle(request, orderRefund);
                }
            }
            else if (orderRefund.Type == 2)
            {
                if (orderRefund.Status == 12)
                {
                    return await SalesReturnFirstAuditSuccessHandle(request, orderRefund);
                }
                else if(orderRefund.Status == 15) {
                    return await SalesReturnSecondAuditSuccessHandle(request, orderRefund);
                }

            }

            throw new Exception($"暂不支持该类型的审核：类型={orderRefund.Type},状态={orderRefund.Status}。");
        }



        /// <summary>
        /// 仅退款审核通过
        /// </summary>
        /// <returns></returns>
        private async Task<bool> RefundAuditSuccessHandle(AuditSuccessCommand request, OrderRefunds orderRefund)
        {
            if (orderRefund.Type != 1)
            {
                throw new Exception("当前退款单不是仅退款单。");
            }
            if (orderRefund.Status != 2 && orderRefund.Status != 3)
            {
                throw new Exception("仅退款单只有状态为[平台审核]才能进行审核通过。");
            }
            var orderDetail = await _orgUnitOfWork.QueryFirstOrDefaultAsync<OrderDetial>(@"SELECT * FROM OrderDetial WHERE id = @id", new { id = orderRefund.OrderDetailId });
            if (orderDetail == null) throw new KeyNotFoundException("找不到OrderDetail");

            var orderInfo = await _orgUnitOfWork.QueryFirstOrDefaultAsync(@"SELECT [Code], [AdvanceOrderId],[Freight] FROM [Order] WHERE id=@orderId", new { orderId = orderRefund.OrderId });
            if (orderInfo == null) throw new KeyNotFoundException("找不到该售后记录中的订单信息");

            var refundPrices = orderDetail.RefundSpreadPrice(orderRefund.Count, orderRefund.Price);
            DynamicParameters parameters = new DynamicParameters();
            //退款
            orderRefund.PreStatus = orderRefund.Status;
            orderRefund.Status = 5;
            orderRefund.StepOneAuditor = request.Auditor;
            orderRefund.StepOneTime = DateTime.Now;
            orderRefund.Modifier = request.Auditor;
            orderRefund.ModifyDateTime = DateTime.Now;
            orderRefund.RefundTime = DateTime.Now;
            orderRefund.RefundPrice = refundPrices.Sum(s => s.refundAmount);
            orderRefund.SpecialReason = request.SpecialReason;
            orderRefund.Remark = string.IsNullOrEmpty(request.SpecialReasonRemark) ? request.SpecialReason.GetDesc() : request.SpecialReasonRemark;


            //执行退款操作。
            await _mediator.Send(new OrderRefundCommand()
            {
                OrderId = orderRefund.OrderId,
                ProductId = orderRefund.ProductId.Value,
                OrderDetailId = orderRefund.OrderDetailId.Value,
                RefundPrices = refundPrices,
                AdvanceOrderId = orderInfo.AdvanceOrderId,
                Remark = "售后审核通过申请退款。"
            });

            //DB 操作
            _orgUnitOfWork.BeginTransaction();
            try
            {
                List<string> sets = new List<string>();
                string filter;
                sets.Add("[PreStatus] = @PreStatus");
                sets.Add("[Status] = @Status");
                sets.Add("[StepOneAuditor] = @StepOneAuditor");
                sets.Add("[Modifier] = @Modifier");
                sets.Add("[ModifyDateTime] = @ModifyDateTime");
                sets.Add("[StepOneTime] = @StepOneTime");
                sets.Add("[RefundTime] = @RefundTime");
                sets.Add("[RefundPrice] = @RefundPrice");
                sets.Add("[SpecialReason] = @SpecialReason");
                sets.Add("[Remark] = @Remark");
                filter = "Id = @Id  And ([Status] = 2 Or [Status] = 3 )  And ([Type] = 1)";
                parameters.AddDynamicParams(orderRefund);
                bool auditSuccessFlag = (await _orgUnitOfWork.ExecuteAsync(string.Format(@"Update OrderRefunds SET {0} WHERE {1} ", string.Join(",", sets), filter), parameters, _orgUnitOfWork.DbTransaction)) > 0;
                if (auditSuccessFlag)
                {
                    //更新OrderDetail的退款数量
                    string increaseSql = @"Update OrderDetial SET RefundCount = ISNULL(RefundCount,0)+ @Count WHERE id = @OrderDetailId";
                    bool updateOrderDetailFlag = (await _orgUnitOfWork.ExecuteAsync(increaseSql, new { OrderDetailId = orderRefund.OrderDetailId, Count = orderRefund.Count }, _orgUnitOfWork.DbTransaction)) > 0;
                    if (!updateOrderDetailFlag)
                    {
                        _orgUnitOfWork.Rollback();
                        return false;
                    }
                    _orgUnitOfWork.CommitChanges();
                    //审核退款通过后续处理。
                    await _mediator.Publish(new OrderRefundSuccessDomainEvent() { OrderRefundId = orderRefund.Id });
                    return true;
                }
                else
                {
                    _orgUnitOfWork.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrderRefundId:{id}; DB操作失败。具体原因请查看异常信息。", orderRefund.Id);
                _orgUnitOfWork.Rollback();
                return false;
            }

        }


        /// <summary>
        /// 退货退款第一次审核通过
        /// </summary>
        /// <param name="request"></param>
        /// <param name="orderRefund"></param>
        /// <returns></returns>
        private async Task<bool> SalesReturnFirstAuditSuccessHandle(AuditSuccessCommand request, OrderRefunds orderRefund)
        {
            if (orderRefund.Type != 2)
            {
                throw new Exception("当前退款单不是退货退款单。");
            }
            if (orderRefund.Status != 12)
            {
                throw new Exception("退货退款单只有状态为[平台审核]才能进行审核通过。");
            }
            List<string> sets = new List<string>();
            string filter;
            //退货退款第一次审核通过。
            orderRefund.PreStatus = orderRefund.Status;
            if (request.SpecialReason == Domain.Enum.OrderRefundSpecialReason.Nothing)
            {
                //没有特殊原因，按正常逻辑，需要用户退货这一步骤
                orderRefund.Status = 14;
            }
            else
            {
                //有特殊原因，不需要退货了，直接默认平台已收货
                orderRefund.Status = 15;
            }
            orderRefund.StepOneAuditor = request.Auditor;
            orderRefund.StepOneTime = DateTime.Now;
            orderRefund.Modifier = request.Auditor;
            orderRefund.ModifyDateTime = DateTime.Now;
            orderRefund.SpecialReason = request.SpecialReason;
            orderRefund.Remark = string.IsNullOrEmpty(request.SpecialReasonRemark) ? request.SpecialReason.GetDesc() : request.SpecialReasonRemark;
            sets.Add("[PreStatus] = @PreStatus");
            sets.Add("[Status] = @Status");
            sets.Add("[StepOneAuditor] = @StepOneAuditor");
            sets.Add("[Modifier] = @Modifier");
            sets.Add("[ModifyDateTime] = @ModifyDateTime");
            sets.Add("[StepOneTime] = @StepOneTime");
            sets.Add("[SpecialReason] = @SpecialReason");
            sets.Add("[Remark] = @Remark");
            filter = "Id = @Id  And ([Status] = 12 ) And ([Type] = 2)";
            bool firstAuditSuccess = (await _orgUnitOfWork.ExecuteAsync(string.Format(@"Update OrderRefunds SET {0} WHERE {1} ", string.Join(",", sets), filter), orderRefund)) > 0;
            if (firstAuditSuccess)
            {
                if (orderRefund.Status == 15)
                {
                    //直接自动进行第二次审核
                    bool auditSecondRes = await SalesReturnSecondAuditSuccessHandle(request, orderRefund);
                    if (!auditSecondRes)
                    {
                        return false;
                    }
                }
                else
                {
                    //需要发通知提醒填写物流信息
                    try { await _mediator.Send(new SendInputFreightInfoTipsCommand() { ToUserId = orderRefund.RefundUserId.Value, OrderRefundId = orderRefund.Id }); } catch { }
                }
            }

            return firstAuditSuccess;

        }


        /// <summary>
        /// 退货退款第二次审核通过
        /// </summary>
        /// <param name="request"></param>
        /// <param name="orderRefund"></param>
        /// <returns></returns>
        private async Task<bool> SalesReturnSecondAuditSuccessHandle(AuditSuccessCommand request, OrderRefunds orderRefund)
        {

            if (orderRefund.Type != 2)
            {
                throw new Exception("当前退款单不是退货退款单。");
            }
            if (orderRefund.Status != 15)
            {
                throw new Exception("退货退款单只有状态为[平台收货]才能进行审核通过。");
            }
            var orderDetail = await _orgUnitOfWork.QueryFirstOrDefaultAsync<OrderDetial>(@"SELECT * FROM OrderDetial WHERE id = @id", new { id = orderRefund.OrderDetailId });
            if (orderDetail == null) throw new KeyNotFoundException("找不到OrderDetail");

            var orderInfo = await _orgUnitOfWork.QueryFirstOrDefaultAsync(@"SELECT [Code], [AdvanceOrderId],[Freight] FROM [Order] WHERE id=@orderId", new { orderId = orderRefund.OrderId });
            if (orderInfo == null) throw new KeyNotFoundException("找不到该售后记录中的订单信息");
            if (request.RefundAmount > orderRefund.Price)
            {
                throw new Exception($"实退金额不能超过申请退款金额,申请退款金额={orderRefund.Price}");
            }
            var refundPrices = orderDetail.RefundSpreadPrice(orderRefund.Count, request.RefundAmount.GetValueOrDefault());
            DynamicParameters parameters = new DynamicParameters();
            parameters.AddDynamicParams(orderRefund);
            orderRefund.PreStatus = orderRefund.Status;
            orderRefund.Status = 17;
            orderRefund.StepTwoAuditor = request.Auditor;
            orderRefund.StepTwoTime = DateTime.Now;
            orderRefund.Modifier = request.Auditor;
            orderRefund.ModifyDateTime = DateTime.Now;
            orderRefund.RefundTime = DateTime.Now;
            orderRefund.RefundPrice = refundPrices.Sum(s => s.refundAmount);

            //执行退款操作。
            await _mediator.Send(new OrderRefundCommand()
            {
                OrderId = orderRefund.OrderId,
                ProductId = orderRefund.ProductId.Value,
                OrderDetailId = orderRefund.OrderDetailId.Value,
                RefundPrices = refundPrices,
                AdvanceOrderId = orderInfo.AdvanceOrderId,
                Remark = "售后审核通过申请退款。"
            });


            //DB操作
            _orgUnitOfWork.BeginTransaction();
            try
            {
                List<string> sets = new List<string>();
                string filter;
                sets.Add("[PreStatus] = @PreStatus");
                sets.Add("[Status] = @Status");
                sets.Add("[StepTwoAuditor] = @StepTwoAuditor");
                sets.Add("[StepTwoTime] = @StepTwoTime");
                sets.Add("[Modifier] = @Modifier");
                sets.Add("[ModifyDateTime] = @ModifyDateTime");
                sets.Add("[RefundTime] = @RefundTime");
                sets.Add("[RefundPrice] = @RefundPrice");
                filter = "Id = @Id  And ([Status] = 15 )  And ([Type] = 2)";
                bool auditSuccessFlag = (await _orgUnitOfWork.ExecuteAsync(string.Format(@"Update OrderRefunds SET {0} WHERE {1} ", string.Join(",", sets), filter), parameters, _orgUnitOfWork.DbTransaction)) > 0;
                if (auditSuccessFlag)
                {
                    //更新OrderDetail的退款数量
                    string increaseSql = @"Update OrderDetial SET ReturnCount = ISNULL(ReturnCount,0)+  @Count WHERE id = @OrderDetailId";
                    bool updateOrderDetailFlag = (await _orgUnitOfWork.ExecuteAsync(increaseSql, new { OrderDetailId = orderRefund.OrderDetailId, Count = orderRefund.Count }, _orgUnitOfWork.DbTransaction)) > 0;
                    if (!updateOrderDetailFlag)
                    {
                        _orgUnitOfWork.Rollback();
                        return false;
                    }
                    _orgUnitOfWork.CommitChanges();
                    //审核退款通过后续处理。
                    await _mediator.Publish(new OrderRefundSuccessDomainEvent() { OrderRefundId = orderRefund.Id });
                    return true;
                }
                else
                {
                    _orgUnitOfWork.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrderRefundId:{id}; DB操作失败。具体原因请查看异常信息。", orderRefund.Id);
                _orgUnitOfWork.Rollback();
                return false;
            }

        }





    }
}
