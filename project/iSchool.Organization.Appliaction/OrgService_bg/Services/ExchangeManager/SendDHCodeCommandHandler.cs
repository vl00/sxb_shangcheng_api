using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Domain.Enum;
using System.Linq;
using iSchool.Organization.Appliaction.Service.Organization;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.Apis;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Domain.Security.Settings;
using Microsoft.Extensions.Options;

namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{
    /// <summary>
    /// 前后台公用--发送兑换码
    /// </summary>
    public class SendDHCodeCommandHandler : IRequestHandler<SendDHCodeCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;
        private readonly IConfiguration _config;
        private readonly WechatMessageTplSetting _tplCollect;
        public SendDHCodeCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient,IMediator mediator, IConfiguration config, IOptions<WechatMessageTplSetting> tplCollect)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
            _config = config;
            _tplCollect = tplCollect.Value;
        }

        public async Task<ResponseResult> Handle(SendDHCodeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // find used                
                var rinfo = (await _mediator.Send(new GetOrderRedeemInfoQueryArgs { OrderIds = new[] { request.OrderId } })).FirstOrDefault();
                if (rinfo?.Redeem0?.IsVaild == true)
                {
                    Debug.Assert(rinfo.Redeem0.Used == true);
                    return ResponseResult.Success("该订单已发送过兑换码");
                }

                var msgTemplate = _orgUnitOfWork.Query<MsgTemplate>($@" select * from [dbo].[MsgTemplate] where CourseId = '{request.CourseId}' ").FirstOrDefault();
                if (msgTemplate == null)
                {
                    return ResponseResult.Failed("短信模板填充内容不存在，请先填写");
                }
                string key = CacheKeys.notUsedSingleCode.FormatWith(request.CourseId, request.OrderId);
                string codekey = "";
                var delCacheKeys = new List<string>() { key };//待清除缓存
                

                //如果订单有发送失败记录，则取失败的兑换码继续发送
                var failRecord = _orgUnitOfWork.Query<Exchange>(@$" select * from dbo.Exchange where IsValid=1 and orderid='{request.OrderId}' and status={ExchangeStatus.Fail_In_Send.ToInt()} ;").FirstOrDefault();
                if (failRecord != null)//把发送记录变为已发送
                {                    
                    request.DHCode = failRecord.Code;
                    request.SendMsgMobile = failRecord.Mobile;
                    codekey = CacheKeys.CodeIsLock.FormatWith(request.CourseId, request.DHCode);
                    _orgUnitOfWork.DbConnection.Execute($@"
update  dbo.Exchange  set status={ExchangeStatus.Converted.ToInt()} ,ModifyDateTime=getdate(),Modifier='{request.Creator}' where  IsValid=1 and orderid='{request.OrderId}'
;");
                }
                else//首次发送
                {
                    var redeemCode = _mediator.Send(new QuerySingleRedeemCode() { CourseId = request.CourseId, OrderId = request.OrderId }).Result;

                    #region 校验

                    //自动发送且无可用验证码
                    if (msgTemplate.IsAuto && redeemCode == null)
                    {
                        return ResponseResult.Failed("当前已无可用兑换码！");
                    }

                    //校验兑换码                   
                    if (msgTemplate.IsAuto == false && (redeemCode == null || redeemCode.Code != request.DHCode))
                    {
                        return ResponseResult.Failed("兑换码已被占用，请重新获取");
                    }

                    //手机号非空判断
                    if (string.IsNullOrEmpty(request.SendMsgMobile))
                    {
                        return ResponseResult.Failed("接收短信手机号为空，无法发送短信！");
                    }

                    #endregion

                    request.DHCode = redeemCode.Code;
                    key = CacheKeys.notUsedSingleCode.FormatWith(request.CourseId, request.OrderId);
                    codekey = CacheKeys.CodeIsLock.FormatWith(request.CourseId, request.DHCode);
                    delCacheKeys = new List<string>() { key, codekey };//待清除缓存

                    string sqlRedeemCode = "";//兑换码更新sql
                    string sqlExchange = "";//新增兑换记录sql

                    //1、兑换码更新order表发货时间、订单状态为待收货
                    string sql_UpdateOrder = $@" 
update [dbo].[Order] set [ShippingTime]=@time ,[status]=@orderstatus 
,[ModifyDateTime]=@time, [Modifier]=@modifier where id=@OrderId and [status] in({OrderStatusV2.Paid.ToInt()},{OrderStatusV2.ExWarehouse.ToInt()}) ;

update dbo.[OrderDetial] set [status]=@orderstatus where orderid=@OrderId and [status] in({OrderStatusV2.Paid.ToInt()},{OrderStatusV2.ExWarehouse.ToInt()})
                    ";
                    var dy = new DynamicParameters();
                    dy.Set("time", DateTime.Now);
                    dy.Set("OrderId", request.OrderId);
                    dy.Set("orderstatus", OrderStatusV2.Shipping.ToInt());//订单变为待收货状态
                    //dy.Set("orderstatus0", OrderStatusV2.Paid.ToInt());
                    dy.Set("modifier", request.Creator);
                    

                    #region 发送兑换码逻辑  
                    sqlRedeemCode = $@" update[dbo].[RedeemCode] set Used=1  where IsVaild=1 and id =@RedeemCodeId and Courseid=@Courseid  ; ";
                    dy.Set("RedeemCodeId", redeemCode.Id)
                        .Set("Courseid", request.CourseId);

                    //2.2、插入一条发送兑换码记录
                    sqlExchange = $@"insert into[dbo].[Exchange]([id], [orderid], [userid], [code], [mobile], [status], [CreateTime], [Creator], [IsValid])
                        values(NEWID(), @orderid, @userid, @code, @mobile, @status, @time, @Creator, @IsValid);";
                    dy.Set("orderid", request.OrderId);
                    dy.Set("userid", request.UserId);
                    dy.Set("code", request.DHCode);
                    dy.Set("mobile", request.SendMsgMobile);
                    dy.Set("status", ExchangeStatus.Converted.ToInt());
                    dy.Set("Creator", request.Creator);
                    dy.Set("IsValid", true);
                    #endregion

                    _orgUnitOfWork.BeginTransaction();

                    _orgUnitOfWork.DbConnection.Execute(sql_UpdateOrder + sqlRedeemCode + sqlExchange, dy, _orgUnitOfWork.DbTransaction);

                    _orgUnitOfWork.CommitChanges();
                                        
                }
                #region 短信通知 
                var msgresult1 = SendMsg1(request);
                if (msgresult1.code != 200)//失败则更新发送记录状态、订单状态为待发货
                {
                    _orgUnitOfWork.DbConnection.Execute($@"
update  dbo.Exchange  set status={ExchangeStatus.Fail_In_Send.ToInt()} ,ModifyDateTime=getdate(),Modifier='{request.Creator}' where  IsValid=1 and orderid='{request.OrderId}'
;");
                    return ResponseResult.Failed($"发送短信失败：{msgresult1.message}");
                }           
                #endregion
                #region 微信通知                
                var title = _orgUnitOfWork.Query<string>($@" select title from dbo.Course where IsValid=1 and id='{request.CourseId}'; ").FirstOrDefault();
                var openid = _orgUnitOfWork.Query<string>($@" select openID from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and userID='{request.UserId}'; ").FirstOrDefault();
                if(openid==null)
                    return ResponseResult.Failed($"用户Id[{request.UserId}]在[iSchoolUser].[dbo].[openid_weixin]中无有效记录");
                var openId = openid;//代写
                var courseName = title;//课程名称
                var RedeemCode = request.DHCode;//兑换码
                var deleverCompany = string.IsNullOrEmpty(request.CompanyName) ? "" : $",{request.CompanyName}："; //物流公司
                var deleverNo = request.ExpressCode;//物流单号
                string msg = "";
                if (!string.IsNullOrEmpty(request.DHCode))
                {
                    msg += $"，兑换码为：{request.DHCode}";
                }
                if (!string.IsNullOrEmpty(deleverCompany))
                {
                    msg += $"{deleverCompany}：{deleverNo}";
                }

                var wechatNotify = new WechatTemplateSendCmd()
                {

                    KeyWord1 = $"您购买的《{courseName}》已发货{msg}。",
                    KeyWord2 = DateTime.Now.ToDateTimeString(),
                    OpenId = openId,
                    Remark = "请点击详情查看兑换码",
                    MsyType = WechatMessageType.订单已发货 ,
                    OrderID = request.OrderId

                };
                await _mediator.Send(wechatNotify);

                #endregion
                               

                _ = _redisClient.BatchDelAsync(delCacheKeys, 10);
                return ResponseResult.Success("操作成功");
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return ResponseResult.Failed($"系统错误：【{ex.Message}】");
            }           
        }


        #region 
        /// <summary>
        /// 
        /// </summary>
        private CommRespon SendMsg1(SendDHCodeCommand request)
        {
            
            var SxbPhoneMsgSite = _config["AppSettings:SendMsg:SxbPhoneMsgSite"];
            try
            {
                var msgTemplate = _orgUnitOfWork.Query<MsgTemplate>($@" select * from [dbo].[MsgTemplate] where CourseId = '{request.CourseId}' ").FirstOrDefault();

               

                var url = $"{SxbPhoneMsgSite}/api/SMSApi/SendSxbMessage";
                var templateId = _config["AppSettings:SendMsg:QcloudCourseBookMessageTemplateId"];
                var phones = new string[1] { "+86" + request.SendMsgMobile };
                var value1 = msgTemplate.Variable1;
                var link =_tplCollect.Odershipped.link.Replace("{orderId}", request.OrderId.ToString("N"));
                var shortUrl =  _mediator.Send(new LongUrlToShortUrlRequest() { OriginUrl = link }).Result.data;
                var value2 = $@"{request.DHCode}{msgTemplate.Variable2}。{("兑换码链接为:" + shortUrl + "，").If(!string.IsNullOrEmpty(shortUrl))}请尽快关注【上学帮】公众号";
                var templateParams = new string[2] { value1, value2 };
                string postData = JsonConvert.SerializeObject(new
                {
                    templateId,
                    phones,
                    templateParams
                });
                var httpResult = HttpHelper.HttpPostWithHttps(url, postData, null, "application/json");
                var smsResult = JsonConvert.DeserializeObject<SMSAPIResult>(httpResult);

                string smsCode = smsResult.sendStatus.FirstOrDefault()?.code;
                if (smsResult.statu != 1 || smsCode != "Ok")
                {
                    return CommRespon.Failure(smsCode);
                }
                return CommRespon.Success("发送短信成功");


            }
            catch (Exception ex)
            {

                return new CommRespon() { code = 400, message = $"url:{SxbPhoneMsgSite}，Message：{ex.Message}" };
            }
        }
        #endregion

        /// <summary>
        /// 发送兑换码的短信
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <param name="dhCode">兑换码</param>
        /// <param name="sendMsgMobile">接收短信电话</param>
        /// <param name="msgTemplate">短信模板</param>
        /// <returns></returns>
        private  SmsSingleSenderResult SendMsg(Guid courseId,string dhCode,string sendMsgMobile,MsgTemplate msgTemplate)
        {

            var appid = Convert.ToInt32(_config["AppSettings:SendMsg:QcloudAppId"]);
            var apptoken = _config["AppSettings:SendMsg:QcloudAppKey"];
            var value1 = msgTemplate.Variable1;
            var value2 =$@"{dhCode},{msgTemplate.Variable2}，兑换码链接为:{msgTemplate.Url}，请尽快关注【上学帮】公众号";
            var templateParams = new List<string>() { value1, value2 };
            var templateId = Convert.ToInt32(_config["AppSettings:SendMsg:QcloudCourseBookMessageTemplateId"]);
            TXSMSHelper smsHelper = new TXSMSHelper();
            SmsSingleSenderResult res = smsHelper.SendWithParam(appid, apptoken, "86",sendMsgMobile, templateId, templateParams, "上学帮", null, null);
            return res;            
        }
    }
}
