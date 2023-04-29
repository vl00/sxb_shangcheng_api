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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class OrderRefundFreightCommandHandler : IRequestHandler<OrderRefundFreightCommand>
    {
        HttpClient _client;
        AftersalesOption _option;
        OrgUnitOfWork _orgUnitOfWork;
        ILogger<OrderRefundFreightCommandHandler> _logger;
        IConfiguration _config;

        public OrderRefundFreightCommandHandler(IOrgUnitOfWork orgUnitOfWork
            , ILogger<OrderRefundFreightCommandHandler> logger
            , IHttpClientFactory  httpClientFactory
            , IOptions<AftersalesOption> options
            , IConfiguration config)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _logger = logger;
            _client = httpClientFactory.CreateClient();
            _option = options.Value;
            _config = config;


            _client.BaseAddress = new Uri(_option.PayBaseUrl);
        }


        public async Task<Unit> Handle(OrderRefundFreightCommand request, CancellationToken cancellationToken)
        {
            
            if (request.Freight > 0)
            {
                //开启事务
                _orgUnitOfWork.BeginTransaction();

                try
                {
                    //更新OrderRefund中的价格（+ 运费）以及是否包含运费字段。
                    string updateOrderRefundSql = @"
UPDATE OrderRefunds 
SET 
Price += @Freight,
IsContainFreight = 1
WHERE Id = @OrderRefundId And ([Status] = 5 Or [Status] = 17)
";
                    int updateOrderRefundFlag = await _orgUnitOfWork.ExecuteAsync(updateOrderRefundSql, new { OrderRefundId = request.OrderRefundId, Freight = request.Freight }, _orgUnitOfWork.DbTransaction);
                    if (updateOrderRefundFlag > 0)
                    {
                        //申请退运费
                        var (refundFlag,msg) = await RefundFreight(new RefundFreightRequest()
                        {
                            advanceOrderId =  request.AdvanceOrderId,
                            orderId = request.OrderId,
                            refundAmount = request.Freight,
                            remark = "订单已全部退款，申请退运费。",
                            system = 2,
                            refundType = 4
                        });
                        if (refundFlag)
                        {
                            //退运费成功 提交事务
                            _orgUnitOfWork.CommitChanges();
                        }
                        else {
                            throw new Exception($"调用申请退运费服务返回结果失败。Message:{msg}");
                        }

                    }
                    else
                    {
                        throw new KeyNotFoundException("该售后退款记录不符合退运费条件。");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "订单退运费失败。");
                    _orgUnitOfWork.Rollback();
                    throw ex;
                }


            }
            return Unit.Value;
        }


        /// <summary>
        /// 退运费
        /// </summary>
        async Task<(bool, string)> RefundFreight(RefundFreightRequest request)
        {
            var paykey = _config["AppSettings:wxpay:paykey"];
            var system = _config["AppSettings:wxpay:system"];
            try
            {
                string url = "/api/PayOrder/PartRefund";
                var body = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var httpReqMsg = new HttpRequestMessage(HttpMethod.Post, url);
                httpReqMsg.Content = new StringContent(body, Encoding.UTF8, "application/json");
                httpReqMsg.SetFinanceSignHeader(paykey, body, system);
                var response = await _client.SendAsync(httpReqMsg);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                var jobj = JObject.Parse(responseContent);
                if (!jobj["succeed"].Value<bool>())
                {
                    return (false, jobj["msg"].Value<string>());
                }
                else
                {
                    return (jobj["data"]["applySucess"].Value<bool>(), jobj["data"]["aapplyDesc"].Value<string>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "请求退运费服务发生异常。");
                return (false, "请求退运费服务发生异常");
            }

            //{
            //  "msgTime": "2021-10-25T14:45:05.902885+08:00",
            //  "succeed": false,
            //  "status": 201,
            //  "msg": "找不到该订单的支付订单，退款失败",
            //  "data": null
            //}
        }


        class RefundFreightRequest
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
            /// 运费金额
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
            /// [ 1 = All(全部), 2 = ChildOrder(子单), 3 = ProductOrder(子单里面单个商品) ] 4 = 退运费
            /// </summary>
            public int refundType { get; set; }

        }

    }

}
