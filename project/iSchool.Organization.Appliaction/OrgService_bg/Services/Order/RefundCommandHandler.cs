using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Orders;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Order
{
    /// <summary>
    /// (后台退款)订单退款
    /// </summary>
    public class RefundCommandHandler : IRequestHandler<RefundCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IHttpClientFactory httpClientFactory;
        AppSettings appSettings;
        IMediator _mediator;

        public RefundCommandHandler(IOrgUnitOfWork unitOfWork,IWXUnitOfWork wXUnitOfWork           
            , IHttpClientFactory httpClientFactory
            , IOptions<AppSettings> options      
            , IMediator mediator
            )
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.httpClientFactory = httpClientFactory;
            this.appSettings = options.Value;
            _mediator = mediator;
        }


        public async Task<ResponseResult> Handle(RefundCommand request, CancellationToken cancellationToken)
        {
           
            try
            {
                var rr = await OnHandle(request);
                var r1 = rr.ToString().ToObject<RefundResult>();
                //退款成功 更新订单状态
                if (r1.ApplySucess)
                {
                    #region 微信通知                
                    try
                    {
                        var courseNames = (await _orgUnitOfWork.QueryAsync<string>($@" 
                            select j.title from [OrderDetial] d 
                            cross apply openjson(d.ctn) with(title nvarchar(max) '$.title')j
                            where d.orderid=@OrdId
                        ", new { request.OrdId })).AsArray();

                        var k1 = $"《{courseNames.FirstOrDefault()}》" + ($"等{courseNames.Length}个商品"); // courseNames.Length < 1 ? "" : 

                        var openid = await _orgUnitOfWork.QueryFirstOrDefaultAsync<string>($@" select top 1 openID from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and userID='{request.OrderUserId}'; ");
                        var wechatNotify = new WechatTemplateSendCmd()
                        {
                            KeyWord1 = $"您购买的{k1}已退款。",
                            KeyWord2 = DateTime.Now.ToDateTimeString(),
                            OpenId = openid,
                            Remark = "点击更多查看详情",
                            MsyType = WechatMessageType.订单退款,
                            OrderID = request.OrdId
                        };
                        await _mediator.Send(wechatNotify);
                    }
                    catch { }
                    #endregion

                    await _orgUnitOfWork.ExecuteAsync($@" 
                        update [dbo].[Order] set [status]={OrderStatusV2.RefundOk.ToInt()},RefundTime=GETDATE(),RefundUserId=@UserId,[ModifyDateTime]=getdate(), [Modifier]=@UserId where id=@OrdId

                        update dbo.[OrderDetial] set [status]={OrderStatusV2.RefundOk.ToInt()} where orderid=@OrdId
                    ", new { request.OrdId, request.UserId });

                    //+ 2021-10-14 退款成功,需要删除种草机会
                    {
                        await _orgUnitOfWork.ExecuteAsync($@"
                            update [EvaluationReward] set [IsValid]=0 where [Used]=0 and [orderid]=@OrdId
                        ", new { request.OrdId });
                    }
                    
                    return ResponseResult.Success("已退款");
                }
                else
                {
                    return ResponseResult.Failed(r1.AapplyDesc);
                }

            }
            catch (Exception ex)
            {
                _orgUnitOfWork.SafeRollback();
                return ResponseResult.Failed(ex.Message);
            }
        }


        /// <summary>退款</summary>
        private async Task<JToken> OnHandle(RefundCommand request)
        {
            var refundUrl = request.RefundApiUrl;// 调用秀彬退款api   
            var paykey = request.PayKey;
            var system = request.System;
            await default(ValueTask);
            var amount = 0M;
            //Debugger.Break();
            //
            // 暂时 多订单退款退全部(但不含运费)
            //if (Guid.Empty != request.OrdId)
            //{
            //     amount = await _orgUnitOfWork.QueryFirstOrDefaultAsync<decimal>("select payment from [order] where IsValid=1 and Id=@OrdId", new { request.OrdId });
            //    //分销收益撤销 
            //    await MaketCancel(request.OrdId);
            //}
            //else 
            {
                amount = await _orgUnitOfWork.QueryFirstOrDefaultAsync<decimal>("select sum(payment) from [order] where IsValid=1 and AdvanceOrderId=@AdvanceOrderId", new { request.AdvanceOrderId });
            } 
            //
            using var http = httpClientFactory.CreateClient(string.Empty);
            var req = new HttpRequestMessage(HttpMethod.Post, refundUrl);
            var body = (new 
            {
                advanceOrderId = request.AdvanceOrderId,
                //orderId = request.OrdId,
                refundAmount = amount, //request.Price,
                remark = "后台退款",
                system = 2
            }).ToJsonString(camelCase: true);
            req.SetContent(new StringContent(body, Encoding.UTF8, "application/json"));
            req.SetFinanceSignHeader(paykey, body, system);
            HttpResponseMessage res = null;
            try
            {
                res = await http.SendAsync(req);
                res.EnsureSuccessStatusCode();
            }
            catch
            {
                throw;
            }
            ResponseResult<JToken> rr = null;
            try
            {
                var str = await res.Content.ReadAsStringAsync();
                rr = str.ToObject<ResponseResult<JToken>>();
            }
            catch
            {
                throw;
            }
            if (!rr.Succeed)
            {
                throw new CustomResponseException(rr.Msg, (int)rr.status);
            }
            return rr.Data;
        }
        private async Task<JToken> MaketCancel(Guid orderId)
        {
            var orderNo = await _orgUnitOfWork.QueryFirstOrDefaultAsync<string>("select code from [order] where IsValid=1 and Id=@orderId", new { orderId });
            if (string.IsNullOrEmpty(orderNo)) throw new CustomResponseException("撤销订单分销收益时找不到该订单");

            var cancelBonusUrl = this.appSettings.DrpfxBaseUrl + "/api/FxOrder/CancelFxInfo";
            using var http = httpClientFactory.CreateClient(string.Empty);
            var req = new HttpRequestMessage(HttpMethod.Post, cancelBonusUrl);
            var body = (new
            {
                orgOrderNo = orderNo,
               
            }).ToJsonString(camelCase: true);
            req.SetContent(new StringContent(body, Encoding.UTF8, "application/json"));
            HttpResponseMessage res = null;
            try
            {
                res = await http.SendAsync(req);
                res.EnsureSuccessStatusCode();
            }
            catch
            {
                throw;
            }
            ResponseResult<JToken> rr = null;
            try
            {
                var str = await res.Content.ReadAsStringAsync();
                rr = str.ToObject<ResponseResult<JToken>>();
            }
            catch
            {
                throw;
            }
            if (!rr.Succeed)
            {
                throw new CustomResponseException(rr.Msg, (int)rr.status);
            }
            return rr.Data;
        }

    }

   
}
