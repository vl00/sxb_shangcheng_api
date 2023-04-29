
using CSRedis;
using EasyWeChat.Model;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security.Settings;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyWeChat.Interface;
namespace iSchool.Organization.Appliaction.Wechat
{
    public class WechatTemplateSendHanler : IRequestHandler<WechatTemplateSendCmd, bool>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        private readonly WechatMessageTplSetting _tplCollect;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMediator _mediator;
        IConfiguration config;
        ITemplateMessageService _templateMessageService;
        public WechatTemplateSendHanler(IConfiguration config, IMediator mediator, IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IOptions<WechatMessageTplSetting> tplCollect, IHttpClientFactory httpClientFactory, ITemplateMessageService templateMessageService)
        {
            _httpClientFactory = httpClientFactory;
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _tplCollect = tplCollect.Value;
            _mediator = mediator;
            this.config = config;
            _templateMessageService = templateMessageService;
        }
        public async Task<bool> Handle(WechatTemplateSendCmd request, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient("org_system_send_msg");
            var message = await GetTplContent(request);
            var tokenCmd = new GetWxGzhAccessTokenQuery() { GzhAppName = "fwh" };
            var accessToken = await _mediator.Send(tokenCmd);
            if (null == accessToken || string.IsNullOrEmpty(accessToken.Token)) throw new CustomResponseException($"发送模板消息{request.MsyType.ToString()},accessToken获取错误");

            var response = await _templateMessageService.SendAsync(accessToken.Token, message);

            if (!string.IsNullOrEmpty(response.errmsg))
                return false;
            return true;

        }
        private async Task<SendTemplateRequest> GetTplContent(WechatTemplateSendCmd param)
        {
            var tpl_id = "";
            var list_filed = new List<TemplateDataFiled>();

            list_filed.Add(new TemplateDataFiled()
            {
                Filed = "keyword1",
                Value = param.KeyWord1,


            });
            list_filed.Add(new TemplateDataFiled()
            {
                Filed = "keyword2",
                Value = param.KeyWord2,

            });
            //--未处理风险。各种字段的长度问题
            switch (param.MsyType)
            {
                case WechatMessageType.订单已发货:
                    tpl_id = _tplCollect.Odershipped.tplid;
                    param.Href = _tplCollect.Odershipped.link.Replace("{orderId}", param.OrderID.ToString("N"));
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"订单已发货通知",

                    });
                    break;
                case WechatMessageType.物流:
                    tpl_id = _tplCollect.OderdWL.tplid;
                    param.Href = _tplCollect.OderdWL.link.Replace("{orderId}", param.OrderID.ToString("N"));
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"订单已发货通知",


                    });
                    break;
                case WechatMessageType.部分发货:
                    tpl_id = _tplCollect.OderdWL.tplid;
                    param.Href = _tplCollect.OderdWL.link.Replace("{orderId}", param.OrderID.ToString("N"));
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"订单商品已部分发货",


                    });
                    break;
                case WechatMessageType.全部发货:
                    tpl_id = _tplCollect.OderdWL.tplid;
                    param.Href = _tplCollect.OderdWL.link.Replace("{orderId}", param.OrderID.ToString("N"));
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"订单商品已全部发货",
                    });
                    break;
                case WechatMessageType.订单已完成:
                    tpl_id = _tplCollect.OrderFinished.tplid;
                    param.Href = _tplCollect.OrderFinished.link.Replace("{orderId}", param.OrderID.ToString("N"));
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"订单已完成通知",

                    });
                    break;

                case WechatMessageType.订单退款:
                    tpl_id = _tplCollect.OrderRefund.tplid;
                    param.Href = _tplCollect.OrderRefund.link.Replace("{orderId}", param.OrderID.ToString("N"));
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"订单已退款通知",
                    });
                    break;
                case WechatMessageType.支付成功回调发现订单已关闭而进行退款:
                    tpl_id = _tplCollect.RefundByOrderCancelledOnHandlePaided.tplid;
                    param.Href = _tplCollect.RefundByOrderCancelledOnHandlePaided.link;
                    list_filed.Add(new TemplateDataFiled
                    {
                        Filed = "first",
                        Value = $"订单已退款通知",
                    });
                    break;

                case WechatMessageType.种草审核不通过:
                    tpl_id = _tplCollect.EvltRewardUnPass.tplid;
                    param.Href = _tplCollect.EvltRewardUnPass.link.Replace("{evltId}", param.EvltId.ToString("N"));
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"种草审核不通过",

                    });
                    break;
                case WechatMessageType.种草审核通过:
                    tpl_id = _tplCollect.EvltRewardPass.tplid;
                    param.Href = _tplCollect.EvltRewardPass.link;
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"种草奖励到账",

                    });
                    break;
                case WechatMessageType.升级顾问通知:
                    tpl_id = _tplCollect.CheckAndNotifyUserToDoFxlvup.tplid;
                    param.Href = _tplCollect.CheckAndNotifyUserToDoFxlvup.link;
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"升级顾问通知",
                    });
                    break;
                case WechatMessageType.好物新人立返佣金:
                    tpl_id = _tplCollect.NewUserRewardOfBuyGoodthing.tplid;
                    param.Href = _tplCollect.NewUserRewardOfBuyGoodthing.link;
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"奖励到账通知",
                    });
                    break;

                case WechatMessageType.成功发起退货or退款申请时:
                    tpl_id = _tplCollect.RefundApplyOk.tplid;
                    param.Href = _tplCollect.RefundApplyOk.link.Replace("{id}", param.Args["id"].ToString());
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"退款申请已发起",
                    });
                    break;
                case WechatMessageType.退款or退货退款申请已取消:
                    tpl_id = _tplCollect.RefundApplyCancel.tplid;
                    param.Href = _tplCollect.RefundApplyCancel.link.Replace("{id}", param.Args["id"].ToString());
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"退款申请已取消",
                    });
                    break;
                case WechatMessageType.确认收货导致退款申请取消:
                    tpl_id = _tplCollect.RefundApplyCancelByShipped.tplid;
                    param.Href = _tplCollect.RefundApplyCancelByShipped.link;
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"退款申请已取消",
                    });
                    break;
                case WechatMessageType.填写退货物流信息即将超时:
                    tpl_id = _tplCollect.RefundApplySendbackWL1.tplid;
                    param.Href = _tplCollect.RefundApplySendbackWL1.link.Replace("{id}", param.Args["id"].ToString());
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"填写退货物流信息即将超时",
                    });
                    break;
            }
            var message = new SendTemplateRequest(param.OpenId, tpl_id);
            if (param.MsyType == WechatMessageType.订单已发货)//发h5
            {
                var shortUrl = (await _mediator.Send(new LongUrlToShortUrlRequest() { OriginUrl = param.Href })).data;
                message.Url = shortUrl;
            }
            else
            {
                message.Url = param.Href;
#if !DEBUG
                message.SetMiniprogram(config["AppSettings:WechatNotifyMiniProgramAppId"], message.Url);
#endif
            }

            list_filed.Add(new TemplateDataFiled()
            {
                Filed = "remark",
                Value = param.Remark,
            });
            message.SetData(list_filed.ToArray());
            return message;


        }
    }
}
