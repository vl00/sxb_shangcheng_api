using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.Options;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Domain;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class OrderRefundCommandHandler : IRequestHandler<OrderRefundCommand>
    {

        HttpClient _client;
        AftersalesOption _option;
        ILogger<OrderRefundCommandHandler> _logger;
        IConfiguration _config;

        public OrderRefundCommandHandler(IHttpClientFactory httpClientFactory
            , IOrgUnitOfWork orgUnitOfWork
            , ILogger<OrderRefundCommandHandler> logger
            , IOptions<AftersalesOption> options
            , IConfiguration config)
        {

            _client = httpClientFactory.CreateClient();
            _logger = logger;
            _option = options.Value;
            _config = config;

            _client.BaseAddress = new Uri(_option.PayBaseUrl);
        }
        public async Task<Unit> Handle(OrderRefundCommand request, CancellationToken cancellationToken)
        {
            (bool success, string msg) = await PartRefund(new PartRefundRequest()
            {
                orderDetailId = request.OrderDetailId,
                advanceOrderId = request.AdvanceOrderId,
                productId = request.ProductId,
                orderId = request.OrderId,
                refundAmount = request.RefundPrices.Sum(s=>s.unitPrice * s.number),
                refundType = 3,
                remark = request.Remark,
                system = 2,
                RefundProductInfo = request.RefundPrices.Select(s => new RefundProductInfo()
                {
                     RefundProductNum = s.number,
                     RefundProductPrice = s.unitPrice,
                     Amount = s.refundAmount
                }).ToList()
            }) ;
            if (!success)
            {
                throw new Exception($"退款服务返回结果为失败。消息：{msg}");
            }
            return Unit.Value;
        }




        async Task<(bool, string)> PartRefund(PartRefundRequest request)
        {
            var paykey = _config["AppSettings:wxpay:paykey"];
            var system = _config["AppSettings:wxpay:system"];
            string url = "/api/PayOrder/PartRefund";
            var body = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            try
            {


                var httpReqMsg = new HttpRequestMessage(HttpMethod.Post, url);
                httpReqMsg.Content = new StringContent(body, Encoding.UTF8, "application/json");
                httpReqMsg.SetFinanceSignHeader(paykey, body, system);
                var response = await _client.SendAsync(httpReqMsg);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                 var jobj = JObject.Parse(responseContent);
                if (!jobj["succeed"].Value<bool>())
                {
                    string responsemsg = jobj["msg"].Value<string>();
                    _logger.LogError( $"退款服务返回失败结果。Body={body},msg={responsemsg}");
                    return (false, responsemsg);
                }
                else {
                    return (jobj["data"]["applySucess"].Value<bool>(), jobj["data"]["aapplyDesc"].Value<string>());
                }
                

            
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"请求退款服务发生异常。Body={body}");
                return (false, $"请求退款服务发生异常。Body={body}");
            }
        }


        class PartRefundRequest
        {

            /// <summary>
            /// 预支付订单ID
            /// </summary>
            public Guid advanceOrderId { get; set; }

            /// <summary>
            /// 订单ID
            /// </summary>
            public Guid orderId { get; set; }

            /// <summary>
            /// 订单详情ID
            /// </summary>
            public Guid orderDetailId { get; set; }
            /// <summary>
            /// 退款金额
            /// </summary>
            public decimal refundAmount { get; set; }
            /// <summary>
            /// 备注
            /// </summary>
            public string remark { get; set; }

            /// <summary>
            /// 系统 1.付费问答 2.机构
            /// </summary>
            public int system { get; set; } = 2;

            /// <summary>
            /// SKU ID
            /// </summary>
            public Guid productId { get; set; }

            /// <summary>
            /// [ 1 = All(全部), 2 = ChildOrder(子单), 3 = ProductOrder(子单里面单个商品) ]
            /// </summary>
            public int refundType { get; set; }

            public List<RefundProductInfo> RefundProductInfo { get; set; }



        }

        public class RefundProductInfo
        {
            /// <summary>
            /// 退款数量
            /// </summary>
            public int RefundProductNum { get; set; }

            /// <summary>
            /// 实际退款金额
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// 原退款单价
            /// </summary>
            public decimal RefundProductPrice { get; set; }



        }




    }
}
