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

using iSchool.Organization.Appliaction.Wechat;
using iSchool.Infrastructure.Extensions;

using Microsoft.Extensions.Configuration;


namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{
    /// <summary>
    /// 后台--发货
    /// </summary>
    public class SendGoodsCommandHandler : IRequestHandler<SendGoodsCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;
        private readonly IConfiguration _config;

        public SendGoodsCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IMediator mediator, IConfiguration config)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
            _config = config;
        }

        public async Task<ResponseResult> Handle(SendGoodsCommand request, CancellationToken cancellationToken)
        {

            var logistics = _orgUnitOfWork.DbConnection.QueryFirstOrDefault<OrderLogistics>("SELECT * FROM dbo.OrderLogistics WHERE OrderId=@orderid AND IsValid=1", new { orderid = request.OrderId });

            var orderDetial = _orgUnitOfWork.DbConnection.QueryFirstOrDefault<Domain.OrderDetial>("SELECT * FROM dbo.OrderDetial WHERE orderid=@orderid and id=@detailid", new { orderid = request.OrderId, detailid = request.OrderDetailId });

            if (orderDetial == null)
            {
                throw new CustomResponseException("订单详情不存在");
            }
            if (orderDetial.Status == (int)OrderStatusV2.RefundOk)
            {
                throw new CustomResponseException("订单已退款");
            }

            try
            {

                if (request.IsSendExpress)//是否发物流
                {

                    //发货更新order表的sql
                    string sql_UpdateOrder = $@" update [dbo].[Order] 
set [SendExpressTime]=@time,[expressType]=@expressType,[expressCode]=@expressCode
,[ModifyDateTime]=@time,[Modifier]=@operuserid
{" ,[status] = @orderstatus ,[ShippingTime]=@time".If(request.IsSendDHCode == false) } 
where id=@OrderId; 

update dbo.[OrderDetial] set [status]=@orderstatus where orderid=@OrderId and [status]=@orderstatus0;
";
                    if (logistics != null)
                    {
                        sql_UpdateOrder += @"   UPDATE dbo.OrderLogistics SET ExpressType=@expressType,ExpressCode=@expressCode,ModifyDateTime=@time,[Modifier]=@operuserid where OrderId=@OrderId   AND OrderDetailId=@OrderDetailId";
                    }
                    else
                    {
                        sql_UpdateOrder += @"   INSERT INTO dbo.OrderLogistics
(
    Id,
    OrderId,
    OrderDetailId,
    Number,
    ExpressType,
    ExpressCode,
    SendExpressTime,
    CreateTime,
    Creator,
    ModifyDateTime,
    Modifier,
    IsValid
)
VALUES
(   NEWID(),      -- Id - uniqueidentifier
    @OrderId,      -- OrderId - uniqueidentifier
    @OrderDetailId,      -- OrderDetailId - uniqueidentifier
    1,         -- Number - smallint
    @expressType,        -- ExpressType - varchar(100)
    @expressCode,        -- ExpressCode - varchar(500)
    GETDATE(), -- SendExpressTime - datetime
    GETDATE(), -- CreateTime - datetime
    @userid,      -- Creator - uniqueidentifier
    GETDATE(), -- ModifyDateTime - datetime
    @userid,      -- Modifier - uniqueidentifier
    1       -- IsValid - bit
    )";
                    }

                    var dy = new DynamicParameters();
                    dy.Set("time", DateTime.Now);
                    dy.Set("expressType", request.ExpressType);
                    dy.Set("expressCode", request.ExpressCode);
                    dy.Set("OrderId", request.OrderId);
                    dy.Set("OrderDetailId", request.OrderDetailId);
                    dy.Set("orderstatus", OrderStatusV2.Shipping.ToInt());
                    dy.Set("orderstatus0", OrderStatusV2.Paid.ToInt());
                    dy.Set("operuserid", request.Creator);
                    dy.Set("userid", request.UserId);



                    #region 暂不用
                    //if (request.IsSendDHCode)//发货且发送兑换码
                    //{
                    //    //校验兑换码
                    //    var redeemCode = _mediator.Send(new QuerySingleRedeemCode() { CourseId = request.CourseId, OrderId = request.OrderId }).Result;
                    //    if(redeemCode==null || redeemCode.Code != request.DHCode)
                    //    {
                    //        return ResponseResult.Failed("兑换码已更新，请重新发货！");
                    //    }
                    //    var isSendMsgSucceed = false;

                    //    #region 1、发送兑换码逻辑
                    //    if (string.IsNullOrEmpty(request.BeginClassMobile))
                    //    {
                    //        return ResponseResult.Failed("上课手机号为空，无法发送短信！");
                    //    }
                    //    var msgResult = SendMsg(request);
                    //    if (msgResult.result == 0) isSendMsgSucceed = true;

                    //    #endregion

                    //    //2、发送结果(成功、失败)
                    //    if (isSendMsgSucceed)//发送兑换码成功
                    //    {
                    //        sqlRedeemCode = $@" update[dbo].[RedeemCode] set Used=1  where id =@RedeemCodeId; ";
                    //        dy.Set("RedeemCodeId", request.RedeemCodeId);

                    //        //2.2、插入一条发送兑换码记录
                    //        sqlExchange = $@"insert into[dbo].[Exchange]([id], [orderid], [userid], [code], [mobile], [status], [CreateTime], [Creator], [IsValid])
                    //        values(NEWID(), @orderid, @userid, @code, @mobile, @status, @time, @Creator, @IsValid);";
                    //        dy.Set("orderid", request.OrderId);
                    //        dy.Set("userid", request.UserId);
                    //        dy.Set("code", request.DHCode);
                    //        dy.Set("mobile", request.BeginClassMobile);
                    //        dy.Set("status", ExchangeStatus.Converted.ToInt());
                    //        dy.Set("Creator", request.Creator);
                    //        dy.Set("IsValid", true);
                    //    }
                    //    else//发送兑换码失败
                    //    {
                    //        return ResponseResult.Failed($"操作失败：发送兑换码失败【{msgResult.errmsg}】");
                    //    }
                    //} 
                    #endregion

                    //先入库
                    _orgUnitOfWork.DbConnection.Execute(sql_UpdateOrder, dy);
                }

                //再发兑换码
                if (request.IsSendDHCode)
                {
                    string key = CacheKeys.notUsedSingleCode.FormatWith(request.CourseId, request.OrderId);
                    string codekey = CacheKeys.CodeIsLock.FormatWith(request.CourseId, request.DHCode);
                    var delCacheKeys = new List<string>() { key, codekey };//待清除缓存
                    var dhcodeResult = _mediator.Send(new SendDHCodeCommand()
                    {
                        CourseId = request.CourseId
                        ,
                        SendMsgMobile = request.BeginClassMobile ?? request.RecvMobile
                        ,
                        Creator = request.Creator
                        ,
                        DHCode = request.DHCode
                        ,
                        OrderId = request.OrderId
                        ,
                        UserId = request.UserId
                        ,
                        ExpressCode = request.ExpressCode
                        ,
                        CompanyName = request.CompanyName
                    }).Result;

                    if (dhcodeResult.Succeed == false)
                        return ResponseResult.Failed(dhcodeResult.Msg);
                    else
                        _ = _redisClient.BatchDelAsync(delCacheKeys, 10);
                }

                if (!request.IsSendDHCode && request.IsSendExpress)//只发物流，不发兑换码也需要推送
                {
                    #region 微信通知                

                    var courseIds = _orgUnitOfWork.Query<Guid>($"select courseid from OrderDetial where orderid='{request.OrderId}'");
                    var title = _orgUnitOfWork.QueryFirstOrDefault<string>($@" select title from dbo.Course where IsValid=1 and id='{(courseIds.FirstOrDefault())}'; ");
                    var courseName = title;//课程名称

                    var openid = _orgUnitOfWork.Query<string>($@" select openID from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and userID='{request.UserId}'; ").FirstOrDefault();
                    if (openid == null)
                        return ResponseResult.Failed($"用户Id[{request.UserId}]在[iSchoolUser].[dbo].[openid_weixin]中无有效记录");
                    var openId = openid;//代写

                    var RedeemCode = request.DHCode;//兑换码
                    var deleverCompany = string.IsNullOrEmpty(request.CompanyName) ? "" : $",{request.CompanyName}："; //物流公司
                    var deleverNo = request.ExpressCode;//物流单号
                    string msg = "";
                    msg += $"{deleverCompany}{deleverNo}";
                    var wechatNotify = new WechatTemplateSendCmd()
                    {

                        KeyWord1 = $"您购买的《{courseName}》{courseIds.Count()}个商品已发货，{msg}，请留意物流信息。",
                        KeyWord2 = DateTime.Now.ToDateTimeString(),
                        OpenId = openId,
                        Remark = "点击下方【详情】查看订单发货详情",
                        MsyType = WechatMessageType.全部发货,
                        OrderID = request.OrderId

                    };
                    await _mediator.Send(wechatNotify);

                    #endregion
                }
                return ResponseResult.Success("操作成功");

            }
            catch (Exception ex)
            {
                return ResponseResult.Failed($"系统错误：【{ex.Message}】");
            }

        }

    }
}
