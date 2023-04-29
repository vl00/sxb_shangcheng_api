using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.WeChatNotification;
using iSchool.Organization.Domain;
using MediatR;
using EasyWeChat.Interface;
using EasyWeChat.Model;
using iSchool.Organization.Appliaction.RequestModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace iSchool.Organization.Appliaction.Service.WeChatNotification
{
    public class SendOrderRefundAuditFailTipsCommandHandler : IRequestHandler<SendOrderRefundAuditFailTipsCommand>
    {
        OrgUnitOfWork _orgUnitOfWork;
        ITemplateMessageService _templateMessageService;
        IMediator _mediator;
        ILogger<SendOrderRefundAuditFailTipsCommandHandler> _logger;
        IConfiguration _configuration;
        public SendOrderRefundAuditFailTipsCommandHandler(
            IOrgUnitOfWork orgUnitOfWork
            , ITemplateMessageService templateMessageService
            , IMediator mediator
            , ILogger<SendOrderRefundAuditFailTipsCommandHandler> logger
            , IConfiguration configuration = null)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _templateMessageService = templateMessageService;
            _mediator = mediator;
            _logger = logger;
            _configuration = configuration;
        }


        public async Task<Unit> Handle(SendOrderRefundAuditFailTipsCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var refundInfo = await _orgUnitOfWork.QueryFirstOrDefaultAsync(@"SELECT Course.[title] Title,OrderRefunds.[Count],OrderRefunds.Id,OrderRefunds.StepOneTime AuditTime FROM OrderRefunds
JOIN CourseGoods ON OrderRefunds.ProductId = CourseGoods.Id
JOIN Course ON Course.id = CourseGoods.Courseid
WHERE   (OrderRefunds.[status] = 13 Or OrderRefunds.[status] = 16 Or OrderRefunds.[status] = 6) AND OrderRefunds.Id= @orderRefundId ", new { orderRefundId = request.OrderRefundId });
                if (refundInfo == null) throw new KeyNotFoundException("找不到符合条件的售后单信息。");

                string fwhAccessToken = (await _mediator.Send(new GetWxGzhAccessTokenQuery() { GzhAppName = "fwh" })).Token;
                var (mobile, fwhopenId) = await _mediator.Send(new GetUserOpenIdQryArgs() { UserId = request.ToUserId });
                string pagePth = $"pagesMine/pages/order-refound-detail/order-refound-detail?id={request.OrderRefundId}";
                SendTemplateRequest sendRequest = new SendTemplateRequest(fwhopenId, "FoJDl1Zyukcx9Rnn1-OqInVt8K8gqIVGturxxn0riQk");
                sendRequest.SetMiniprogram(_configuration["AppSettings:WechatNotifyMiniProgramAppId"], pagePth);
#if DEBUG
                sendRequest = new SendTemplateRequest(fwhopenId, "GOzaXlv4aWRKBOR_JWBeOHz3OELLrCJ_t57Q_Bk6p6k");
                sendRequest.Url = "https://www.sxkid.com";
#endif
                sendRequest.SetData(
                new TemplateDataFiled()
                {
                    Filed = "first",
                    Value = $"很抱歉！您发起的《{refundInfo.Title}》{refundInfo.Count}件商品退款/退货退款申请未通过！",
                },
                new TemplateDataFiled()
                {
                    Filed = "keyword1",
                    Value = "退款/退货退款申请未通过",
                },
                new TemplateDataFiled()
                {
                    Filed = "keyword2",
                    Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                },
                new TemplateDataFiled()
                {
                    Filed = "remark",
                    Value = "点击下方【查看详情】查看申请未通过原因",
                });

                var response = await _templateMessageService.SendAsync(fwhAccessToken, sendRequest);
                if (response == null || response.errcode == ResponseCodeEnum.erro)
                {
                    throw new Exception($"调用发送微信模板消息返回结果异常。异常消息：{response?.errmsg}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送微信模板消息失败。");
            }
            return Unit.Value;
        }
    }
}
