using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Appliaction.RequestModels.WeChatNotification;
using iSchool.Organization.Appliaction.ViewModels.Aftersales;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class AuditFailCommandHandler : IRequestHandler<AuditFailCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        public AuditFailCommandHandler(IOrgUnitOfWork orgUnitOfWork
            , IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _mediator = mediator;
        }

        public async Task<bool> Handle(AuditFailCommand request, CancellationToken cancellationToken)
        {
            //退款/换货状态  
            //1. 提交申请  2.平台审核(发货) 3.平台审核(未发货)   4.平台退款  5.退款成功  6.审核失败
            //11.提交申请   12.平台审核   13.审核失败   14.寄回商品  15平台收货  16.验货失败   17.退款成功

            var orderRefund = _orgUnitOfWork.QueryFirstOrDefault<OrderRefunds>("SELECT * FROM OrderRefunds WHERE Id = @id", new { id = request.Id });
            if (orderRefund == null) throw new KeyNotFoundException("找不到该售后记录");
            List<string> sets = new List<string>();
            string filter;
            if (orderRefund.Status == 2 || orderRefund.Status == 3)
            {
                //退款
                orderRefund.PreStatus = orderRefund.Status;
                orderRefund.Status = 6;
                orderRefund.StepOneAuditor = request.Auditor;
                orderRefund.StepOneAuditRecord = request.Reason;
                orderRefund.StepOneTime = DateTime.Now;
                orderRefund.Modifier = request.Auditor;
                orderRefund.ModifyDateTime = DateTime.Now;
                sets.Add("[PreStatus] = @PreStatus");
                sets.Add("[Status] = @Status");
                sets.Add("[StepOneAuditor] = @StepOneAuditor");
                sets.Add("[StepOneAuditRecord] = @StepOneAuditRecord");
                sets.Add("[Modifier] = @Modifier");
                sets.Add("[ModifyDateTime] = @ModifyDateTime");
                sets.Add("[StepOneTime] = @StepOneTime");
                filter = "Id = @Id  And ([Status] = 2 Or [Status] = 3  )";

            }
            else  if (orderRefund.Status == 12)
            {
                //退货退款第一次审核
                orderRefund.PreStatus = orderRefund.Status;
                orderRefund.Status = 13;
                orderRefund.StepOneAuditor = request.Auditor;
                orderRefund.StepOneAuditRecord = request.Reason;
                orderRefund.StepOneTime = DateTime.Now;
                orderRefund.Modifier = request.Auditor;
                orderRefund.ModifyDateTime = DateTime.Now;
                sets.Add("[PreStatus] = @PreStatus");
                sets.Add("[Status] = @Status");
                sets.Add("[StepOneAuditor] = @StepOneAuditor");
                sets.Add("[StepOneAuditRecord] = @StepOneAuditRecord");
                sets.Add("[StepOneTime] = @StepOneTime");
                sets.Add("[Modifier] = @Modifier");
                sets.Add("[ModifyDateTime] = @ModifyDateTime");
                filter = "Id = @Id  And ([Status] = 12 )";
            }
            else if (orderRefund.Status == 15)
            {
                //退货退款第二次审核
                orderRefund.PreStatus = orderRefund.Status;
                orderRefund.Status = 16;
                orderRefund.StepTwoAuditor = request.Auditor;
                orderRefund.StepTwoAuditRecord = request.Reason;
                orderRefund.StepTwoTime = DateTime.Now;
                orderRefund.Modifier = request.Auditor;
                orderRefund.ModifyDateTime = DateTime.Now;
                sets.Add("[PreStatus] = @PreStatus");
                sets.Add("[Status] = @Status");
                sets.Add("[StepTwoAuditor] = @StepTwoAuditor");
                sets.Add("[StepTwoAuditRecord] = @StepTwoAuditRecord");
                sets.Add("[StepTwoTime] = @StepTwoTime");
                sets.Add("[Modifier] = @Modifier");
                sets.Add("[ModifyDateTime] = @ModifyDateTime");
                filter = "Id = @Id  And ([Status] = 15 )";
         
            }
            else
            {
                throw new Exception("该售后状态不允许进行审核操作。");
            }

            bool updateAuditFail = (await _orgUnitOfWork.ExecuteAsync(string.Format(@"Update OrderRefunds SET {0} WHERE {1} ",string.Join(",", sets), filter), orderRefund)) > 0;
            if(updateAuditFail) try { await _mediator.Send(new SendOrderRefundAuditFailTipsCommand() { ToUserId = orderRefund.RefundUserId.Value, OrderRefundId = orderRefund.Id }); } catch { }
            return updateAuditFail;
        }
    }
}
