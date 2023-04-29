using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.Wechat;

namespace iSchool.Organization.Appliaction.Services
{
    public class RefundCancelCmdHandler : IRequestHandler<RefundCancelCmd, object>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        IUserInfo me;
        NLog.ILogger log;

        public RefundCancelCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IUserInfo me, NLog.ILogger log,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this.me = me;
            this.log = log;
        }

        public async Task<object> Handle(RefundCancelCmd cmd, CancellationToken cancellation)
        {
            var orderRefund = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<OrderRefunds>($@"
                select * from OrderRefunds where IsValid=1 and id=@Id
            ", new { cmd.Id });

            if (orderRefund == null)
            {
                throw new CustomResponseException("无效的退款单");
            }
            if (!cmd.IsFromExpired && me.UserId != orderRefund.RefundUserId)
            {
                throw new CustomResponseException("非法操作", Consts.Err.RefundCancel_NotSameUser);
            }
            if (((RefundTypeEnum)orderRefund.Type).In(RefundTypeEnum.FastRefund, RefundTypeEnum.BgRefund)
                || ((RefundStatusEnum)orderRefund.Status).In(RefundStatusEnum.RefundSuccess, RefundStatusEnum.ReturnSuccess,
                    RefundStatusEnum.RefundAuditFailed, RefundStatusEnum.ReturnAuditFailed, RefundStatusEnum.InspectionFailed,
                    RefundStatusEnum.Cancel, RefundStatusEnum.CancelByExpired))
            {
                throw new CustomResponseException("当前退款单状态不能取消");
            }

            try
            {
                var sql = "update [OrderRefunds] set [status]=@cstt,[ModifyDateTime]=getdate(),Modifier=@UserId where Id=@Id and [status]=@st0 ";
                await _orgUnitOfWork.ExecuteAsync(sql, new
                {
                    cmd.Id,
                    st0 = orderRefund.Status,
                    UserId = !cmd.IsFromExpired ? me.UserId.ToString() : "00111111-1111-1111-1111-111111111100",
                    cstt = cmd.IsFromExpired ? RefundStatusEnum.CancelByExpired : RefundStatusEnum.Cancel,
                });
            }
            catch (Exception ex)
            {
                if (cmd.IsFromExpired) throw;

                log.Error(GetLogMsg(cmd).SetError(ex));
                throw new CustomResponseException("系统繁忙");
            }

            var orders = await _mediator.Send(new OrderDetailSimQuery { OrderId = orderRefund.OrderId });
            var order = orders.Orders?.FirstOrDefault();

            // 发wx通知
            if (order != null && order.Prods.FirstOrDefault(_ => _.OrderDetailId == orderRefund.OrderDetailId) is CourseOrderProdItemDto cprod)
            {
                try
                {
                    await _mediator.Send(new SendWxTemplateMsgCmd
                    {
                        UserId = order.UserId,
                        WechatTemplateSendCmd = new WechatTemplateSendCmd
                        {
                            KeyWord1 = $"您发起的《{cprod.Title}》{orderRefund.Count}件商品{(orderRefund.Type == (int)RefundTypeEnum.Refund ? "退款" : "退货退款")}申请已取消！",
                            KeyWord2 = DateTime.Now.ToDateTimeString(),
                            Remark = "点击下方【查看详情】查看申请详情",
                            MsyType = WechatMessageType.退款or退货退款申请已取消,
                            Args = new Dictionary<string, object>
                            {
                                ["id"] = orderRefund.Id.ToString(),
                            }
                        }
                    });
                }
                catch { }
            }

            return null;
        }

        NLog.LogEventInfo GetLogMsg(object paramsObj = null)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
            msg.Properties["Caption"] = "取消退款申请";
            msg.Properties["UserId"] = me.UserId;
            msg.Properties["Level"] = "Error";
            if (paramsObj is string str) msg.Properties["Params"] = str;
            else if (paramsObj != null) msg.Properties["Params"] = (paramsObj).ToJsonString(camelCase: true);
            msg.Properties["Class"] = nameof(RefundCancelCmdHandler);
            //msg.Properties["Error"] = $"检测敏感词意外失败.网络异常.err={ex.Message}";
            //msg.Properties["StackTrace"] = ex.StackTrace;
            //msg.Properties["ErrorCode"] = 3;
            return msg;
        }
    }
}
