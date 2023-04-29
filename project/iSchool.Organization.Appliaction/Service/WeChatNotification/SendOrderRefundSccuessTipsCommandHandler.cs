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
    public class SendOrderRefundSccuessTipsCommandHandler : IRequestHandler<SendOrderRefundSccuessTipsCommand>
    {
        OrgUnitOfWork _orgUnitOfWork;
        ITemplateMessageService _templateMessageService;
        IMediator _mediator;
        ILogger<SendOrderRefundSccuessTipsCommandHandler> _logger;
        IConfiguration _configuration;
        public SendOrderRefundSccuessTipsCommandHandler(
            IOrgUnitOfWork orgUnitOfWork
            , ITemplateMessageService templateMessageService
            , IMediator mediator
            , ILogger<SendOrderRefundSccuessTipsCommandHandler> logger
            , IConfiguration configuration = null)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _templateMessageService = templateMessageService;
            _mediator = mediator;
            _logger = logger;
            _configuration = configuration;
        }


        public async Task<Unit> Handle(SendOrderRefundSccuessTipsCommand request, CancellationToken cancellationToken)
        {



            try
            {
                var refundInfo = await _orgUnitOfWork.QueryFirstOrDefaultAsync(@"SELECT Course.[title] Title,OrderRefunds.[Count],OrderRefunds.Id,OrderRefunds.StepOneTime AuditTime FROM OrderRefunds
JOIN CourseGoods ON OrderRefunds.ProductId = CourseGoods.Id
JOIN Course ON Course.id = CourseGoods.Courseid
WHERE  (OrderRefunds.[status] = 17 Or OrderRefunds.[status] = 5) AND OrderRefunds.Id= @orderRefundId ", new { orderRefundId = request.OrderRefundId });
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

                var first = new TemplateDataFiled()
                {
                    Filed = "first",
                    Value = $"您购买的《{refundInfo.Title}》{refundInfo.Count}件商品已成功退款，请注意查收！",
                };
                var kw1 = new TemplateDataFiled()
                {
                    Filed = "keyword1",
                    Value = "退货退款/退款申请已完成",
                };
                var kw2 = new TemplateDataFiled()
                {
                    Filed = "keyword2",
                    Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                };
                var remark = new TemplateDataFiled()
                {
                    Filed = "remark",
                    Value = "点击下方【查看详情】查看退货退款/退款详情",
                };
                if (request.RefundType == Domain.Enum.RefundTypeEnum.FastRefund)
                {
                    kw1.Value = "极速退款已完成";
                    remark.Value = "点击下方【详情】查看退款详情";
                }

                sendRequest.SetData(first,kw1, kw2, remark);
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
