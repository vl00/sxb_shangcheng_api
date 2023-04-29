using EasyWeChat.Interface;
using EasyWeChat.Model;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.WeChatNotification;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.WeChatNotification
{
    public class SendCouponReceiveWillExpireTipsCommnadHandler : IRequestHandler<SendCouponReceiveWillExpireTipsCommnad>
    {

        ITemplateMessageService _templateMessageService;
        IMediator _mediator;
        ILogger<SendCouponReceiveWillExpireTipsCommnadHandler> _logger;
        IConfiguration _configuration;
        public SendCouponReceiveWillExpireTipsCommnadHandler(ITemplateMessageService templateMessageService
            , IMediator mediator
            , ILogger<SendCouponReceiveWillExpireTipsCommnadHandler> logger
            , IConfiguration configuration)
        {
            _templateMessageService = templateMessageService;
            _mediator = mediator;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<Unit> Handle(SendCouponReceiveWillExpireTipsCommnad request, CancellationToken cancellationToken)
        {
            try
            {
                string fwhAccessToken = (await _mediator.Send(new GetWxGzhAccessTokenQuery() { GzhAppName = "fwh" })).Token;
                var (mobile, fwhopenId) = await _mediator.Send(new GetUserOpenIdQryArgs() { UserId = request.ToUserId });
                string pagePth = $"pagesA/pages/activity-goods/activity-goods?couponId={request.CouponId}&sourcePage=couponPage";
                SendTemplateRequest sendRequest = new SendTemplateRequest(fwhopenId, "FoJDl1Zyukcx9Rnn1-OqInVt8K8gqIVGturxxn0riQk");
                sendRequest.SetMiniprogram(_configuration["AppSettings:WechatNotifyMiniProgramAppId"], pagePth);
#if DEBUG
                sendRequest = new SendTemplateRequest(fwhopenId, "GOzaXlv4aWRKBOR_JWBeOHz3OELLrCJ_t57Q_Bk6p6k");
                sendRequest.Url = "https://m.sxkid.com";
#endif
                sendRequest.SetData(
                new TemplateDataFiled()
                {
                    Filed = "first",
                    Value = $"您的 {request.CouponValue}优惠券即将到期！请及时使用！ ",
                },
                new TemplateDataFiled()
                {
                    Filed = "keyword1",
                    Value = $"{request.CouponValue}优惠券",
                },
                new TemplateDataFiled()
                {
                    Filed = "keyword2",
                    Value = "即将过期",
                },
                new TemplateDataFiled()
                {
                    Filed = "remark",
                    Value = "点击下方【查看详情】去使用吧～",
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
