using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Domain.Modles;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{/*
    [Obsolete]
    public class MiniOrderRepayHandler0 //: IRequestHandler<MiniOrderRepayCmd, Res2Result<CourseWxCreateOrderCmdResult_v4>>
    {
        private readonly IUserInfo me;
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;
        private readonly ILock1Factory _lck1fay;
        private readonly NLog.ILogger log;

        public MiniOrderRepayHandler0(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper, ILock1Factory lck1fay, IServiceProvider services, 
            IUserInfo me)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
            this._lck1fay = lck1fay;
            this.log = services.GetService<NLog.ILogger>();
            this.me = me;
        }

        public async Task<Res2Result<CourseWxCreateOrderCmdResult_v4>> Handle(MiniOrderRepayCmd cmd, CancellationToken cancellation)
        {
            var sql = $@"
select o.* from [Order] o with (nolock)
where 1=1 and o.IsValid=1 and o.AdvanceOrderId=@AdvanceOrderId
order by o.CreateTime desc
";
            var orders = (await _orgUnitOfWork.DbConnection.QueryAsync<Order>(sql, new { AdvanceOrderId = cmd.OrderId })).AsList();
            if (orders.Count < 1)
            {
                throw new CustomResponseException("订单不存在.");
            }
            if (orders.Any(_ => _.Status != (int)OrderStatusV2.Unpaid))
            {
                throw new CustomResponseException("当前订单状态不允许支付.");
            }
            if (orders.Any(_ => _.CreateTime.Value < DateTime.Now.AddMinutes(-30)))
            {
                throw new CustomResponseException("订单超过30分钟，已失效.");
            }
            if (orders.Any(_ => _.Userid != me.UserId))
            {
                throw new CustomResponseException("订单用户信息有误.");
            }

            var userId = me.UserId;

            // 判断用户是否重复下单
            await using var lck0 = await _lck1fay.LockAsync($"org:lck2:wx_repay_course:userid_{userId}&orderid_{cmd.OrderId}", 5 * 1000, 1);
            if (!lck0.IsAvailable)
            {
                throw new CustomResponseException("操作太快了，请稍等.");
            }

            var result = new CourseWxCreateOrderCmdResult_v4();
            var advanceOrderId = orders[0].AdvanceOrderId;
            var advanceOrderNo = orders[0].AdvanceOrderNo;

            // find goods
            sql = $@"select * from OrderDetial where orderid in @orderids ";
            var products = (await _orgUnitOfWork.QueryAsync<OrderDetial>(sql, new { orderids = orders.Select(_ => _.Id).Distinct() })).AsArray();
            var goodss = products.Select(x => new GoodsItem4Order { GoodsId = x.Productid, BuyCount = x.Number, Price = x.Price }).ToArray();
            var ctn = products.Length < 1 || products[0].Ctn.IsNullOrEmpty() ? null : JObject.Parse(products[0].Ctn);

            List<(CourseGoodsSimpleInfoDto Info, GoodsItem4Order Input)> courseGoodsLs = default!;
            {
                // get goods
                foreach (var goods in goodss)
                {
                    // 商品
                    CourseGoodsSimpleInfoDto courseGoods = null;
                    try { courseGoods = await _mediator.Send(new CourseGoodsSimpleInfoByIdQuery { GoodsId = goods.GoodsId, AllowNotValid = true, NeedCourse = true }); } catch { }
                    if (courseGoods == null) throw new CustomResponseException("非法操作", Consts.Err.CourseGoodsOffline);

                    courseGoodsLs ??= new List<(CourseGoodsSimpleInfoDto, GoodsItem4Order)>();
                    courseGoodsLs.Add((courseGoods, goods));
                }

                // 下架 - 重新支付锁价格了        
                {
                    //foreach (var (courseGoods, _) in courseGoodsLs)
                    //{
                    //    if (!courseGoods.IsValid)
                    //    {
                    //        result.NotValids.Add(courseGoods);
                    //    }
                    //    else if (!courseGoods._Course.IsValid)
                    //    {
                    //        result.NotValids.Add(courseGoods);
                    //    }
                    //    else if (courseGoods._Course.Status != CourseStatusEnum.Ok.ToInt())
                    //    {
                    //        result.NotValids.Add(courseGoods);
                    //    }
                    //    else if (courseGoods._Course.LastOffShelfTime != null)
                    //    {
                    //        // 自动下架job可能未跑完...
                    //        var diff_sec = (DateTime.Now - courseGoods._Course.LastOffShelfTime.Value).TotalSeconds;
                    //        if (diff_sec >= 0 && diff_sec <= 120)
                    //        {
                    //            result.NotValids.Add(courseGoods);
                    //        }
                    //    }
                    //}
                    //if (result.NotValids?.Count > 0)
                    //{
                    //    return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                    //        "您订单里的部分商品已下架,系统为您重新核价", Consts.Err.CourseGoodsIsOffline).SetData(result);
                    //}
                }
                // 新人专享
                {
                    var newUserArr = new (CourseGoodsSimpleInfoDto Info, GoodsItem4Order)[EnumUtil.GetDescs<CourseTypeEnum>().Count()];
                    foreach (var (courseGoods, goods) in courseGoodsLs)
                    {
                        if (courseGoods._Course.NewUserExclusive)
                        {
                            if (goods.BuyCount > 1)
                                throw new CustomResponseException("新人专享仅限首个,点击确定返回重新下单", Consts.Err.OrderCreate_OnlyCanBuy1);

                            if (newUserArr[courseGoods.Type - 1] != default)
                                throw new CustomResponseException("新人专享仅限首个,点击确定返回重新下单", Consts.Err.OrderCreate_OnlyCanBuy1);

                            newUserArr[courseGoods.Type - 1] = (courseGoods, goods);
                        }
                    }
                    foreach (var (courseGoods, goods) in newUserArr)
                    {
                        if (courseGoods == default) continue;

                        // 是否新用户
                        var isNewUser = (await _mediator.Send(new UserIsCourseTypeNewBuyerQuery { UserId = userId, CourseType = (CourseTypeEnum)courseGoods.Type })).IsNewBuyer;
                        if (!isNewUser) throw new CustomResponseException("下单失败,你不符合本次购买条件", Consts.Err.OrderCreate_NewUserExclusiveAndOldUser);

                        // 不能产生2个新人专享的待支付单                
                        var ano = await _redis.GetAsync(CacheKeys.UnpaidOrderOfNewUserExclusive.FormatWith(courseGoods.Type, userId));
                        if (ano == null) 
                            throw new CustomResponseException("系统繁忙", Consts.Err.OrderCreate_NewUserExclusiveNotInCache);
                        if (ano != $"{advanceOrderNo}_{advanceOrderId:n}")
                            throw new CustomResponseException("您的未支付订单已包含新用户专享商品，点击确定返回重新下单！", Consts.Err.OrderCreate_NewUserExclusiveNotAllowMuitlUnpaidOrder);
                    }
                }
                // 价格 - 重新支付锁价格了
                {
                    //foreach (var (courseGoods, goods) in courseGoodsLs)
                    //{
                    //    if (courseGoods.Price != goods.Price)
                    //        result.PriceChangeds.Add(courseGoods);
                    //}
                    //if (result.PriceChangeds?.Count > 0)
                    //{
                    //    return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                    //        "购买失败,商品价格有变动,点击确定系统为您重新核价", Consts.Err.PriceChanged).SetData(result);
                    //}
                }
            }

            //
            result.OrderId = cmd.OrderId;
            result.OrderNo = orders[0].AdvanceOrderNo;
            var totalpayment = orders.Sum(_ => _.Totalpayment ?? 0m);

            // call 预支付
            Debugger.Break();
            var is_already_payedok = false; // 订单是否其实已支付,可能之前回调处理不成功
            {
                var args_callpay = new WxPayRequest
                {
                    AddPayOrderRequest = new ApiWxAddPayOrderRequest
                    {
                        UserId = userId,
                        OrderNo = result.OrderNo,
                        OrderId = result.OrderId!.Value,
                        OrderStatus = orders[0].Status,
                        TotalAmount = totalpayment,
                        PayAmount = totalpayment,
                        Remark = $"上学帮商城订单-订单尾号{result.OrderNo[^6..]}",
                        OpenId = cmd.OpenId,
                        Attach = $"from=org&od={result.OrderId?.ToString("n")}",

                        OrderByProducts = products.Select(product => new ApiOrderByProduct
                        {
                            ProductId = product.Productid,
                            ProductType = product.Producttype,
                            Status = product.Status,
                            Amount = product.Price * product.Number,
                            Remark = product.Remark,
                            BuyNum = product.Number,
                            Price = product.Price,
                            AdvanceOrderId = result.OrderId!.Value,
                            OrderDetailId = product.Id,
                            OrderId = product.Orderid,
                        }).Union(orders.Where(_ => _.AdvanceOrderId != default && (_.Freight ?? 0) > 0).Select(x => new ApiOrderByProduct
                        {
                            AdvanceOrderId = x.AdvanceOrderId!.Value,
                            OrderId = x.Id,
                            Amount = x.Freight!.Value,
                            Price = x.Freight!.Value,
                            BuyNum = 1,
                            ProductType = 3,
                        })).ToArray(),

                        IsRepay = 1,
                        IsWechatMiniProgram = cmd.IsWechatMiniProgram,
                        AppId = cmd.AppId,

                        FreightFee = orders.Sum(_ => _.Freight ?? 0),
                    }
                };
                try { result.WxPayResult = (await _mediator.Send(args_callpay)).AddPayOrderResponse; }
                catch (Exception ex)
                {
                    var msg = GetLogMsg(args_callpay);
                    msg.Properties["Error"] = $"重新支付调用支付平台(wxpay)失败.err={ex.Message}";
                    msg.Properties["StackTrace"] = ex.StackTrace;
                    msg.Properties["ErrorCode"] = Consts.Err.CallPaidApiError;
                    log.Error(msg);

                    is_already_payedok = ex.Message == "该订单已经支付过了，请勿重复支付";

                    if (!is_already_payedok) throw new CustomResponseException("系统繁忙", Consts.Err.CallPaidApiError);
                }
            }
            // try 补偿
            if (is_already_payedok)
            {
                var b = false;
                try { b = await _mediator.Send(new TryRePayorderCommand { AdvanceOrderId = cmd.OrderId }); } catch { }
                if (b) throw new CustomResponseException("订单已支付.请刷新页面查看.");
                else throw new CustomResponseException("系统繁忙", Consts.Err.CallPaidApiError);
            }

            result.PollId = $"{result.OrderId?.ToString("n")}";
            Debugger.Break();
            {
                var args_poll = new PollCallRequest
                {
                    PreSetCmd = new PollPreSetCommand
                    {
                        Id = CacheKeys.OrderPoll_wxpay_order.FormatWith(result.PollId),
                        ResultType = typeof(WxPayOkOrderDto).FullName,
                        ResultStr = (new WxPayOkOrderDto
                        {
                            OrderId = result.OrderId!.Value,
                            OrderNo = result.OrderNo,
                            AdvanceOrderId = orders.Count > 1 ? advanceOrderId : (Guid?)null,
                            AdvanceOrderNo = result.OrderNo,
                            PayIsOk = null,
                            UserPayTime = null,
                            UserId = userId,
                            Paymoney = totalpayment,
                            OrderType = OrderType.BuyCourseByWx.ToInt(),
                            //BuyAmount = orders.Count == 1 ? products.Sum(_ => _.Number) : default,
                            //CourseId = courseGoodsLs.Count == 1 ? courseGoodsLs[0].Info.CourseId : default,
                            //GoodsId = courseGoodsLs.Count == 1 ? courseGoodsLs[0].Info.Id : default,
                            //Prods = courseGoodsLs.Count == 1 ? null : courseGoodsLs.Select(c => (c.Info.Id, c.Info.CourseId, c.Input.BuyCount)).ToArray(),
                            Prods = products.Select(c => (c.Id, c.Orderid, c.Productid, c.Courseid, (int)c.Number)).ToArray(),
                            FxHeaducode = (string)ctn?["_FxHeaducode"],
                            _Ver = (string)ctn?["_Ver"],
                        }).ToJsonString(camelCase: true),
                        ExpSec = 60 * 60 * 3,
                    }
                };
                try { await _mediator.Send(args_poll); }
                catch (Exception ex)
                {
                    var msg = GetLogMsg(args_poll);
                    msg.Properties["Error"] = $"下单后创建轮询缓存失败.err={ex.Message}";
                    msg.Properties["StackTrace"] = ex.StackTrace;
                    msg.Properties["ErrorCode"] = Consts.Err.PollError;
                    log.Error(msg);
                    throw new CustomResponseException("系统繁忙", Consts.Err.PollError);
                }
            }

            result.NotValids = null;
            result.NoStocks = null;
            result.PriceChangeds = null;
            result.NoRwScores = null;
            return Res2Result.Success(result);
        }

        NLog.LogEventInfo GetLogMsg(object paramsObj = null)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
            msg.Properties["Caption"] = "wx购买课程v4-重新支付";
            msg.Properties["UserId"] = me.UserId;
            msg.Properties["Level"] = "Error";
            if (paramsObj is string str) msg.Properties["Params"] = str;
            else if (paramsObj != null) msg.Properties["Params"] = (paramsObj).ToJsonString(camelCase: true);
            //msg.Properties["Error"] = $"检测敏感词意外失败.网络异常.err={ex.Message}";
            //msg.Properties["StackTrace"] = ex.StackTrace;
            //msg.Properties["ErrorCode"] = 3;
            return msg;
        }
    }
*/
}
