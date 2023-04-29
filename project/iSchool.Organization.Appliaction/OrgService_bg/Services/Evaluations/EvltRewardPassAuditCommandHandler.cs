using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels.Evaluations;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Infrastructure.Extensions;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using iSchool.Organization.Appliaction.RequestModels;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{

    /// <summary>
    /// 种草奖励审核通过
    /// </summary>
    public class EvltRewardPassAuditCommandHandler : IRequestHandler<EvltRewardPassAuditCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        ILock1Factory _lock1Factory;
        IConfiguration _config;
        IHttpClientFactory _httpClientFactory;        

        private static object obj = new object();
        public EvltRewardPassAuditCommandHandler(IOrgUnitOfWork unitOfWork
            , CSRedisClient redisClient, IMediator mediator
            , ILock1Factory lock1Factory
            , IConfiguration config
            , IHttpClientFactory httpClientFactory)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
            _lock1Factory = lock1Factory;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<ResponseResult> Handle(EvltRewardPassAuditCommand request, CancellationToken cancellationToken)
        {
            var isSucceed = false;
            var evltBonus = -1m;
            var dp = new DynamicParameters();
            EvaluationReward reward = default!;
            Exception rerr = null;
            await Task.CompletedTask;

            lock (obj)
            {
                if (request.Reward > 10)
                {
                    return ResponseResult.Failed("输入的奖励金额不正确！");
                }
                dp.Set("EvltId", request.Id)
                    .Set("Operator", request.Operator)
                    .Set("Auditor", request.Operator)
                    .Set("auditstatus", EvltAuditStatusEnum.Ok.ToInt());


                string chanceSql = $@"SELECT member_id as userID,Convert(int,COUNT(member_id)*3*0.2) as TenYuanTotalChance,
(select count(*) from EvaluationReward as er
left join [Order] as o on o.id=er.OrderId
where er.UserId = sih.member_id and er.Used=1 and er.ModifyDateTime > '2021/11/1' and o.paymenttime>'2021/11/1') as TenYuanUsedChance
FROM sign_in_history as sih
WHERE bu_no='SHUANG11_ACTIVITY' 
and member_id in (select top 1000 member_id from sign_in where bu_no='SHUANG11_ACTIVITY' order by CreateTime)
and member_id=@UserID
and blocked=0 GROUP BY member_id";
                var chanceData = _orgUnitOfWork.QueryFirstOrDefaultAsync<ViewModels.EvaluationDto>(chanceSql, new { request.UserId }).Result;
                if (chanceData != null)
                {
                    chanceData.TenYuanRemainChance = chanceData.TenYuanTotalChance - chanceData.TenYuanUsedChance;
                }
                if (chanceData == null || chanceData.TenYuanRemainChance <= 0)
                {
                    return ResponseResult.Failed("当前用户已无奖励机会！");
                }


                //一、审核(核对是否有机会)
                reward = _mediator.Send(new EvltRewardCheckPassCommand { EvltId = request.Id, UserId = request.UserId }).Result;
                if (reward == null) return ResponseResult.Failed("当前用户种草奖励机会为0！");
                else //先占用
                {
                    dp.Set("RewardId", reward.Id);
                    _orgUnitOfWork.DbConnection.Execute($@"
update [dbo].[EvaluationReward] set Used=1,[Modifier]=@Operator,[ModifyDateTime]=GETDATE() where IsValid=1 and id=@RewardId ;
update dbo.Evaluation set auditstatus=@auditstatus,Auditor=@Auditor where IsValid=1 and id=@EvltId ;
", dp);
                }

                //二、发放奖励TODO
                dp.Set("Courseid", reward.CourseId)
                    .Set("CourseGoodsId",reward.GoodsId);
                var priceAndType = GetPriceAndType(reward.Id);

                #region evltBonus--种草奖励金额，全配置文件
                // 网课
                if (priceAndType?.Type == CourseTypeEnum.Course.ToInt())
                {
                    evltBonus = request.Reward * 2;

                    //// 新用户
                    //if (reward.IsNewbuy == true)
                    //{
                    //    var conditions = _config.GetSection("AppSettings:EvltReward:CourseBonus:newbuyer:bonus").GetChildren();
                    //    var condition = conditions.FirstOrDefault(_ => MathInterval.Parse(_.Key).Contains((double)priceAndType.Price));
                    //    if (condition != null)
                    //        evltBonus = decimal.Parse(condition.Value);
                    //}
                    //// 顾问
                    //else if (reward.CourseId != null && reward.TgOrderDetialId != null)
                    //{
                    //    var conditions = _config.GetSection("AppSettings:EvltReward:CourseBonus:fxhead:self").GetChildren();
                    //    var condition = conditions.FirstOrDefault(_ => MathInterval.Parse(_.Key).Contains((double)priceAndType.Price));
                    //    if (condition != null)
                    //        evltBonus = decimal.Parse(condition.Value);
                    //}
                }
                // 好物
                else if (priceAndType?.Type == CourseTypeEnum.Goodthing.ToInt())
                {
                    evltBonus = request.Reward;
                    //if (true)
                    //{
                    //    var conditions = _config.GetSection("AppSettings:EvltReward:GoodThingBonus").GetChildren();
                    //    var condition = conditions.FirstOrDefault(_ => MathInterval.Parse(_.Key).Contains((double)priceAndType.Price));
                    //    if (condition != null)
                    //        evltBonus = decimal.Parse(condition.Value);
                    //}
                }
                #endregion

                if (evltBonus > -1m)
                {
                    try
                    {
                        //查orderDetail

                        var sql = $@"
SELECT Id from OrderDetial where ProductId=@ProductId
and orderid=@OrderId";
                        var orderDetailId = _orgUnitOfWork.DbConnection.QueryFirstOrDefault<Guid>(sql, new { ProductId= reward.GoodsId, OrderId=reward.OrderId });

                        request.UserId = reward.UserId;
                        _ = OnHandle(reward.UserId, evltBonus, reward.OrderId.Value, orderDetailId).Result;
                    }
                    catch (Exception ex)
                    {
                        rerr = ex;
                    }
                }
                else
                {
                    rerr = new CustomResponseException("旧机会无法匹配新配置", 404);
                }
                if (rerr == null) //发放成功
                {
                    dp.Set("Reward", evltBonus);
                    _orgUnitOfWork.DbConnection.Execute($@" 
 update [dbo].[EvaluationReward] set Used=1, [EvaluationId]=@EvltId ,[Reward]=@Reward,[Modifier]=@Operator,[ModifyDateTime]=GETDATE() where IsValid=1 and id=@RewardId ;", dp);
                    isSucceed = true;
                }
                else //发放失败
                {
                    _orgUnitOfWork.DbConnection.Execute($@"
update [dbo].[EvaluationReward] set Used=0 where IsValid=1 and id=@RewardId;
update dbo.Evaluation set auditstatus=null,AuditRecord=null,Auditor=null where IsValid=1 and id=@EvltId;
", dp);
                }
            }

            if (isSucceed)
            {
                #region 微信通知    
                try
                {
                    await _mediator.Send(new SendWxTemplateMsgCmd
                    {
                        UserId = request.UserId,
                        WechatTemplateSendCmd = new WechatTemplateSendCmd()
                        {
                            KeyWord1 = $"您发布的种草已审核通过，{fmt_money(evltBonus)}元奖励已到账，点击查看收益",
                            KeyWord2 = DateTime.Now.ToDateTimeString(),
                            Remark = "点击查看",
                            MsyType = WechatMessageType.种草审核通过,
                            EvltId = request.Id
                        }
                    });
                }
                catch (Exception ex)
                {
                    return ResponseResult.Failed($"已发放奖励.但发送微信通知失败:{ex.Message}");
                }
                #endregion
                return ResponseResult.Success("操作成功");
            }
            else
            {
                return ResponseResult.Failed($"操作失败: {rerr?.Message}");
            }
            

        }

        #region 发放奖励
        /// <summary>
        /// 种草奖励[公司打款入账个人]
        /// </summary>
        /// <param name="toUserId">钱包需要入账的用户id</param>
        /// <param name="amount">变动金额（正数）</param>
        /// <param name="orderId">订单Id</param>
        /// <returns></returns>
        private async Task<bool> OnHandle(Guid toUserId, decimal amount, Guid orderId,Guid orderDetailId)
        {            
            var url = $"{_config["AppSettings:wxpay:baseUrl"]}/api/Wallet/CompanyOperate";// 调用秀彬api[公司打款入账个人]
            var paykey = _config["AppSettings:wxpay:payKey"];
            var system = _config["AppSettings:wxpay:system"];
            await default(ValueTask);

            using var http = _httpClientFactory.CreateClient(string.Empty);
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            var postJson = "{\"toUserId\":\"" + toUserId + "\",\"orderId\":\"" + orderId + "\",\"OrderDetailId\":\"" + orderDetailId + "\",\"orderType\":7,\"amount\":\"" + amount + "\",\"remark\":\"种草奖励\",\"system\": 2}";
            var body = postJson;
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
            return rr.Succeed;
        }


        #endregion

        private PriceAndType GetPriceAndType(Guid rewardId)
        {
            var sql = $@"
--select cg.Price,c.[type] from [dbo].[CourseGoods] as cg
--left join [dbo].[Course] as c on cg.Courseid=c.id and c.IsValid=1
--where cg.IsValid=1 and cg.Courseid=@Courseid and cg.id=@CourseGoodsId ;

select p.price,p.Producttype as [type] from EvaluationReward er
join OrderDetial p on {"p.orderid=er.orderid and er.goodsid=p.Productid".If(rewardId != default)}
where 1=1 {"and er.id=@rewardId".If(rewardId != default)} 
";
            var priceAndType = _orgUnitOfWork.DbConnection.QueryFirstOrDefault<PriceAndType>(sql, new { rewardId });
            return priceAndType;
        }

        class PriceAndType
        {
            /// <summary>
            /// 商品价格
            /// </summary> 
            public decimal Price { get; set; }

            /// <summary>
            /// 类型1=课程，2=好物
            /// </summary>
            public int Type { get; set; }            
        }

        /// <summary>整数时没小数位,小数时保2位不四舍五入</summary>
        static string fmt_money(decimal v)
        {
            var v0 = decimal.Truncate(v);
            return v0 == v ? v0.ToString() : Math.Round(v, 2, MidpointRounding.ToZero).ToString();
        }
    }

}
