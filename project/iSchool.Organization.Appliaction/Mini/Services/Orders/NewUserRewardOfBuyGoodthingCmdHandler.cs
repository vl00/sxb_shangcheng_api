using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class NewUserRewardOfBuyGoodthingCmdHandler : IRequestHandler<NewUserRewardOfBuyGoodthingCmd>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient redis;
        IConfiguration _config;
        NLog.ILogger _log;
        ILock1Factory _lock1Factory;

        public NewUserRewardOfBuyGoodthingCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IConfiguration config,
            NLog.ILogger log, ILock1Factory lock1Factory,
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.redis = redis;
            this._config = config;
            this._log = log;
            _lock1Factory = lock1Factory;
        }

        public async Task<Unit> Handle(NewUserRewardOfBuyGoodthingCmd cmd, CancellationToken cancellation)
        {
            if (cmd.AdvOrderId == default && cmd.OrdersEntity == default) return default;
            var ordersEntity = cmd.OrdersEntity;
            if (ordersEntity == null)
            {
                ordersEntity = await _mediator.Send(new OrderDetailSimQuery { AdvanceOrderId = cmd.AdvOrderId });
            }

            var cc = ordersEntity.Orders.SelectMany(o => o.Prods.OfType<CourseOrderProdItemDto>().Select(_ => (o, Prod: _)))
                .Where(_ => _.Prod.ProdType == CourseTypeEnum.Goodthing.ToInt());

            if (!cc.Any())
            {
                return default;
            }
            // 新人专享跟新人立返一起购买, 新人立返会无效
            if (cc.Any(_ => _.Prod.NewUserExclusive == true))
            {
                return default;
            }

            // get CourseDrpInfos
            if (cmd.CourseDrpInfos == null)
            {
                IList<CourseDrpInfo> courseDrpinfos = default!;
                foreach (var x in ordersEntity.Orders)
                {
                    foreach (var courseOrderProdItemDto in x.Prods.OfType<CourseOrderProdItemDto>())
                    {
                        var courseDrpinfo = courseDrpinfos?.FirstOrDefault(_ => _.Courseid == courseOrderProdItemDto.Id);
                        if (courseDrpinfo == null) courseDrpinfo = await _mediator.Send(new GetCourseFxSimpleInfoQuery { CourseId = courseOrderProdItemDto.Id });
                        if (courseDrpinfo == null)
                        {
                            continue;
                        }
                        courseDrpinfos ??= new List<CourseDrpInfo>();
                        courseDrpinfos.Add(courseDrpinfo);
                    }
                }
                cmd.CourseDrpInfos = courseDrpinfos?.Where(c => c.IsValid && c.NewUserRewardType != null && (c.NewUserRewardValue ?? 0) > 0);
                if ((cmd.CourseDrpInfos?.Count() ?? -1) <= 0)
                {
                    return default;
                }
            }

            // 关联
            cc = cc.Where(c => cmd.CourseDrpInfos.Any(_ => _.Courseid == c.Prod.Id));
            if (!cc.Any()) return default;

            // 拿单价最大那个好物
            var maxPrice = cc.Max(_ => _.Prod.Price);
            var (order, prod) = cc.FirstOrDefault(x => x.Prod.Price == maxPrice);
            if (order == default) return default;
            var courseId = prod.Id;

            var drpInfo = cmd.CourseDrpInfos.FirstOrDefault(_ => _.Courseid == courseId);
            if (drpInfo == null) return default;

            // 去重复call
            await using var _lck_ = await _lock1Factory.LockAsync(new Lock1Option(
                $"org:lck2:newuser_reward_of_buy_goodthing:userid_{ordersEntity.UserId}"
                ).SetExpSec(5)
                .SetRetry(1, 5000, false));

            // 是否(好物)新用户
            var user_IsNewbuyer = (await _mediator.Send(new UserIsCourseTypeNewBuyerQuery
            {
                UserId = order.UserId,
                ExcludedOrderIds = ordersEntity.Orders.Select(_ => _.OrderId).ToArray(),
                CourseType = CourseTypeEnum.Goodthing,
            })).IsNewBuyer;
            if (!user_IsNewbuyer) return default;

            // do
            var money4reward = (CashbackTypeEnum)drpInfo.NewUserRewardType switch
            {
                CashbackTypeEnum.Percent => (prod.Price * (drpInfo.NewUserRewardValue ?? 0m) / 100m),
                CashbackTypeEnum.Yuan => drpInfo.NewUserRewardValue ?? 0m,
                _ => 0m,
            };

            {
                #region old codes
                //var jtk = await _mediator.Send(new CompanyPayToUserWalletCmd
                //{
                //    ToUserId = order.UserId,
                //    OrderId = order.OrderId,
                //    Money = money4reward,
                //    Remark = "机构-好物新人立返佣金",
                //    _others = new { prod.OrderDetailId, CourseDrpInfos = drpInfo }
                //});
                //// wx通知
                //if (jtk != null && money4reward > 0m)
                //{
                //    try
                //    {
                //        await _mediator.Send(new SendWxTemplateMsgCmd
                //        {
                //            UserId = order.UserId,
                //            WechatTemplateSendCmd = new WechatTemplateSendCmd()
                //            {
                //                KeyWord1 = $"您购买《{prod.Title}》获得的新人立返奖励（{fmt_money(money4reward)}元）已下发到钱包",
                //                KeyWord2 = DateTime.Now.ToDateTimeString(),
                //                Remark = "点击详情查看我的钱包",
                //                MsyType = WechatMessageType.好物新人立返佣金,
                //            }
                //        });
                //    }
                //    catch { }
                //}
                #endregion old codes
            }
            //* mp1.6* 奖励在确认发货后发放
            {
                var rr = await _mediator.Send(new WalletFreezeAmountIncomeApiArgs 
                {
                    UserId = order.UserId,
                    BlockedAmount = money4reward,
                    Remark = "好物新人立返佣金",
                    OrderId = order.OrderId,
                    Type = 1,
                    _others = new { prod.OrderDetailId, CourseDrpInfos = drpInfo }
                });
                if (rr.Success)
                {
                    // 冻结成功后记录冻结id
                    var freezeMoneyInLogId = rr.FreezeMoneyInLogId;
                    prod._ctn ??= new JObject();
                    prod._ctn["_FreezeMoneyInLogIds"] = JObject.Parse((new CourseGoodsOrderCtnDto.FreezeMoneyInLogIdDto
                    {
                        Id = freezeMoneyInLogId,
                        Type = 1,
                    }).ToJsonString(camelCase: true));

                    await _orgUnitOfWork.ExecuteAsync($@"
                        update [OrderDetial] set ctn=@ctn where id=@id
                    ", new { id = prod.OrderDetailId, ctn = prod._ctn.ToJsonString() });
                }
            }

            return default;
        }

        /// <summary>整数时没小数位,小数时保2位不四舍五入</summary>
        static string fmt_money(decimal v)
        {
            var v0 = decimal.Truncate(v);
            return v0 == v ? v0.ToString() : Math.Round(v, 2, MidpointRounding.ToZero).ToString();
        }

        void LogError(Guid userid, string errdesc, object obj, Exception ex, int errcode = 500)
        {
            LogError(userid, errdesc, obj?.ToJsonString(camelCase: true), ex, errcode);
        }

        void LogError(Guid userid, string errdesc, string paramsStr, Exception ex, int errcode = 500)
        {
            if (_log != null)
            {
                var msg = _log.GetNLogMsg(nameof(OrderPayedOkEvent))
                    .SetUserId(userid)
                    .SetParams(paramsStr)
                    .SetLevel("错误")
                    .SetError(ex, errdesc, errcode);
                msg.Properties["Class"] = nameof(NewUserRewardOfBuyGoodthingCmdHandler);
                _log.Error(msg);
            }
        }
    }
}
