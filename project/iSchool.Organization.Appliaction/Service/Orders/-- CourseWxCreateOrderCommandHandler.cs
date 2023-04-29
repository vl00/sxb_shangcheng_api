using AutoMapper;
using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Modles;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    // old code
//    [Obsolete("1.9-")]
//    public class CourseWxCreateOrderCommandHandler : IRequestHandler<CourseWxCreateOrderCommand, CourseWxCreateOrderCmdResult>
//    {
//        private const string order_no_prev = "OGC";

//        private readonly IUserInfo me;
//        private readonly OrgUnitOfWork _orgUnitOfWork;
//        private readonly IMediator _mediator;        
//        private readonly CSRedisClient redis;                
//        private readonly IConfiguration _config;
//        private readonly ISxbGenerateNo _sxbGenerate;
//        private readonly NLog.ILogger log;
//        private readonly ILock1Factory _lck1fay;

//        public CourseWxCreateOrderCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
//            IConfiguration config, ISxbGenerateNo sxbGenerate, IUserInfo me, ILock1Factory lck1fay,
//            IServiceProvider services)
//        {
//            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
//            this._mediator = mediator;
//            this.redis = redis;            
//            this._config = config;
//            this.me = me;
//            this._sxbGenerate = sxbGenerate;
//            _lck1fay = lck1fay;
//            this.log = services.GetService<NLog.ILogger>();
//        }

//        public async Task<CourseWxCreateOrderCmdResult> Handle(CourseWxCreateOrderCommand cmd, CancellationToken cancellation)
//        {
//            var result = new CourseWxCreateOrderCmdResult();
//            var userId = me.UserId;
//            int? age = null;
//            Domain.Course course = default!;
//            CourseGoodsSimpleInfoDto courseGoods = default!;
//            Domain.ChildArchives[] childArchives = default!;
//            (CourseExchange courseExchange, UserSxbUnionIDDto unionID_dto, bool isOnRwInviteActivity) = (default, default, false);
//            await default(ValueTask);

//            // valid cmd
//            do
//            {
//                if (string.IsNullOrEmpty(cmd.AddressDto?.Address))
//                { 
//                    throw new CustomResponseException("请填写收货地址"); 
//                }
//                if (cmd.BuyAmount < 1)
//                {
//                    throw new CustomResponseException("购买数量应大于0");
//                }
//                if (cmd.Price < 0)
//                {
//                    throw new CustomResponseException("价格应大于0");
//                }
//                if (!string.IsNullOrEmpty(cmd.Age))
//                {
//                    age = int.TryParse(cmd.Age, out var _age1) ? _age1 : -1;
//                    if (age < 1) throw new CustomResponseException("年龄应大于0");
//                }

//                // 上课电话
//                if (cmd.Ver == "v3" && cmd.BeginClassMobile.IsNullOrEmpty())
//                {
//                    throw new CustomResponseException("请填写上课电话");
//                }
//                if (!cmd.BeginClassMobile.IsNullOrEmpty())
//                {
//                    if (Regex.IsMatch(cmd.BeginClassMobile, @"[^0-9\+\-\(\)]"))
//                        throw new CustomResponseException("不是有效的电话号码");
//                }
//                else cmd.BeginClassMobile = null;

//                // 商品
//                courseGoods = await _mediator.Send(new CourseGoodsSimpleInfoByIdQuery { GoodsId = cmd.GoodsId });
//                if (courseGoods == null || courseGoods.Id == default)
//                {
//                    result.Errcode = Consts.Err.CourseGoodsOffline;
//                    result.Errmsg = "商品已下架了";
//                    return result;
//                }
//                if (courseGoods.Price != cmd.Price)
//                {
//                    result.Errcode = Consts.Err.PriceChanged;
//                    result.Errmsg = "数据更新,请重新刷新页面";
//                    return result;
//                }
//                // (本次)限购数量
//                if ((courseGoods.LimitedBuyNum ?? -1) > 0 && cmd.BuyAmount > courseGoods.LimitedBuyNum) 
//                {
//                    result.Errcode = Consts.Err.OrderCreate_LimitedBuyNum1;
//                    result.Errmsg = $"超过了限购数量{courseGoods.LimitedBuyNum}";
//                    return result;
//                }

//                // 课程
//                try { course = await _mediator.Send(new CourseBaseInfoQuery { CourseId = courseGoods.CourseId }); } catch { }
//                if (course == null)
//                {
//                    result.Errcode = Consts.Err.CourseOffline;
//                    result.Errmsg = "商品已下架了";
//                    return result;
//                }
//                else if (course.LastOffShelfTime != null)
//                {
//                    // 自动下架job可能未跑完...
//                    var diff_sec = (DateTime.Now - course.LastOffShelfTime.Value).TotalSeconds;
//                    if (diff_sec >= 0 && diff_sec <= 120)
//                    {
//                        result.Errcode = Consts.Err.CourseOffline;
//                        result.Errmsg = "商品已下架了.";
//                        return result;
//                    }
//                }

//                // 孩子归档信息
//                if (cmd.ChildrenInfoIds?.Length > 0)
//                {
//                    var sql = $@"select * from ChildArchives where IsValid=1 and Id in @ChildrenInfoIds ";
//                    childArchives = (await _orgUnitOfWork.QueryAsync<Domain.ChildArchives>(sql, new { cmd.ChildrenInfoIds })).AsArray();
//                    if (cmd.ChildrenInfoIds!.Any(id => !childArchives.Select(_ => _.Id).Contains(id)))
//                    {
//                        throw new CustomResponseException("存在无效的孩子信息", Consts.Err.OrderCreate_ChildrenInfoIds_NotMatch);
//                    }
//                }
//                else if (cmd.Ver == "v3")
//                {
//                    throw new CustomResponseException("至少选择一个孩子信息");
//                }
//                else childArchives = Array.Empty<Domain.ChildArchives>();
//            }
//            while (false);

//            // 判断用户是否重复下单
//            if (!(await redis.SetAsync($"org:lck2:buy_course:userid_{userId}", userId, 5, RedisExistence.Nx)))
//            {
//                // 用户频繁操作
//                throw new CustomResponseException("请不要重复购买");
//            }

//            // rw邀请活动 顾问微信群拉粉丝
//            // 判断商品是否是给定的隐形上架的商品, 再判断是否有资格(积分)购买
//            for (var __ = course?.IsInvisibleOnline == true; __; __ = !__)
//            {
//                if (!(await redis.SIsMemberAsync(CacheKeys.RwInviteActivity_InvisibleOnlineCourses, course.Id.ToString())))
//                    break;

//                var sql = "select top 1 * from CourseExchange where CourseId=@Id and IsValid=1";
//                courseExchange = await _orgUnitOfWork.QueryFirstOrDefaultAsync<CourseExchange>(sql, new { course.Id });
//                if (courseExchange == null)
//                    throw new CustomResponseException("无积分配置", Consts.Err.OrderCreate_CourseExchangeIsNull);
//                if (courseExchange.StartTime != null && DateTime.Now < courseExchange.StartTime)
//                    throw new CustomResponseException("积分配置未生效", Consts.Err.OrderCreate_CourseExchangeNotStarted);
//                if (courseExchange.EndTime != null && courseExchange.EndTime <= DateTime.Now)
//                    throw new CustomResponseException("积分配置已失效", Consts.Err.OrderCreate_CourseExchangeIsEnded);

//                _ /*var rdk*/ = (CourseExchangeTypeEnum)courseExchange.Type switch
//                {
//                    CourseExchangeTypeEnum.Ty1 => CacheKeys.RwInviteActivity_InviteeBuyQualify,
//                    CourseExchangeTypeEnum.Ty2 => CacheKeys.RwInviteActivity_InviterBonusPoint,
//                    _ => throw new CustomResponseException("无效的积分配置", Consts.Err.OrderCreate_CourseExchangeIsNotRwInviteActivity),
//                };

//                unionID_dto = await _mediator.Send(new GetUserSxbUnionIDQuery { UserId = userId });
//                if (unionID_dto == null)
//                    throw new CustomResponseException("用户没UnionID", Consts.Err.OrderCreate_UserHasNoUnionID);

//                // 后续才扣积分
//                isOnRwInviteActivity = true;
//            }

//            Debugger.Break();
//            (ILock1 lck0, CustomResponseException lck0err) = (default, default);
//            var v3IsLimitedBuy = (cmd.Ver == "v3" && cmd.BeginClassMobile != null && (courseGoods.LimitedBuyNum ?? 0) > 0);

//            // 1个号码与1个商品 限购
//            if (v3IsLimitedBuy)
//            {
//                lck0 = await _lck1fay.LockAsync(new Lock1Option($"org:lck2:wx_buy_course:v3_BeginClassMobile_{cmd.BeginClassMobile}&goodsid_{courseGoods.Id}")
//                    .SetExpSec(60 * 2)
//                    .SetRetry(3));
//                if (!lck0.IsAvailable) lck0err = new CustomResponseException("系统繁忙", Consts.Err.OrderCreate_LimitedBuy_LockFailed);
//            }

//            await using var _lck0_ = lck0;
//            if (lck0err != null) throw lck0err;

//            // 1个号码与1个商品 限购
//            if (v3IsLimitedBuy)
//            {
//                var sql = $@"
//select sum(d.number) from [order] o 
//left join OrderDetial d on o.id=d.orderid 
//where o.IsValid=1 and o.[type]={OrderType.BuyCourseByWx.ToInt()} 
//and (o.[status]>100 and o.[status]<>{OrderStatusV2.RefundOk.ToInt()}) --
//and d.productid=@GoodsId and o.BeginClassMobile=@BeginClassMobile
//";
//                var numBuyed = await _orgUnitOfWork.DbConnection.ExecuteScalarAsync<int>(sql, new { cmd.GoodsId, cmd.BeginClassMobile });
//                if (numBuyed + cmd.BuyAmount > courseGoods.LimitedBuyNum!.Value)
//                {
//                    throw new CustomResponseException("超过限购,请更换新的电话号码", Consts.Err.OrderCreate_LimitedBuyNum2);
//                }
//            }

//            // 优先预锁上级
//            bool? prebindFxHead_ok = null;
//            Debugger.Break();
//            {
//                prebindFxHead_ok = (bool?)(await _mediator.Send(new ApiDrpFxRequest
//                {
//                    // 多次调用幂等
//                    Ctn = new ApiDrpFxRequest.BecomSecondCmd
//                    {
//                        UserId = userId,
//                        HeadUserId = default,
//                        CourseName = course.Title,
//                    }
//                })).Result;
//            }

//            // buying
//            Debugger.Break();
//            Order order = default!;
//            OrderDetial[] products = default!;
//            bool repay = false;
//            var needfixedStock = 0;  // 用于未支付修正库存: 上次与本次购买的数量可能不一样

//            // 查询之前待支付的单,拿那个单去预先支付
//            do
//            {
//                if (isOnRwInviteActivity) break;

//                var sql = $@"
//select top 1 o.* from [Order] o 
//left join [OrderDetial] d on d.orderid=o.id 
//where 1=1 and o.userid=@userId and o.courseid=@courseid and d.productid=@goodsid
//and o.IsValid=1 and o.[status]={OrderStatusV2.Unpaid.ToInt()} and datediff(second,o.CreateTime,getdate())<{30}*60
//order by o.CreateTime desc
//";
//                order = await _orgUnitOfWork.QueryFirstOrDefaultAsync<Order>(sql, new
//                {
//                    userId,
//                    courseid = course.Id,
//                    goodsid = courseGoods.Id,
//                });
//                if (order == null) break;

//                sql = $@"select * from OrderDetial where orderid=@orderid ";
//                products = (await _orgUnitOfWork.QueryAsync<OrderDetial>(sql, new { orderid = order.Id })).AsArray();

//                // 用于未支付修正库存
//                needfixedStock = products.Sum(_ => _.Number) - cmd.BuyAmount;

//                // 查询支付平台 订单状态
//                // ----已支付情况下也要更新数据库订单的状态 并且更新库存------------------------                
//                var status0 = order.Status;
//                {
//                    // 支付平台的订单状态
//                    var fcr_status = FinanceCenterOrderStatus.Wait.ToInt();
//                    try
//                    {
//                        var fcr = await _mediator.Send(new FinanceCheckOrderPayStatusQuery { OrderId = order.Id });
//                        fcr_status = fcr.OrderStatus;
//                    }
//                    catch (Exception ex)
//                    {
//                        var msg = GetLogMsg(new { OrderId = order.Id });
//                        msg.Properties["Error"] = $"查询之前待支付的订单失败.err={ex.Message}";
//                        msg.Properties["StackTrace"] = ex.StackTrace;
//                        msg.Properties["ErrorCode"] = Consts.Err.CallCheckPaystatusError;
//                        log.Error(msg);

//                        throw new CustomResponseException("系统繁忙", Consts.Err.CallCheckPaystatusError);
//                    }

//                    order.Status = (FinanceCenterOrderStatus)fcr_status switch
//                    {
//                        FinanceCenterOrderStatus.Wait => (int)OrderStatusV2.Unpaid,
//                        FinanceCenterOrderStatus.Process => (int)OrderStatusV2.Paiding,
//                        FinanceCenterOrderStatus.PaySucess => (int)OrderStatusV2.Paid,
//                        FinanceCenterOrderStatus.PayFaile => (int)OrderStatusV2.PaidFailed,
//                        FinanceCenterOrderStatus.Cancel => (int)OrderStatusV2.PaidFailed,
//                        _ => (int)OrderStatusV2.Completed,
//                    };

//                    // 已支付情况下也要更新数据库订单的状态，并且更新库存
//                    if (order.Status.In(OrderStatusV2.Paid.ToInt(), OrderStatusV2.PaidFailed.ToInt()))
//                    {
//                        await _mediator.Send(new UpdateOrderStatusCommand
//                        {
//                            OrderId = order.Id,
//                            NewStatus = order.Status,
//                            Status0 = status0,
//                            //Status0UnPaid_TimeoutMin = 30,
//                            NewStatusOk_Paymenttime = order.Status == OrderStatusV2.Paid.ToInt() ? DateTime.Now : (DateTime?)null,
//                        });

//                        if (order.Status == OrderStatusV2.PaidFailed.ToInt())
//                        {
//                            needfixedStock = 0;
//                            // 失败 回归库存                            
//                            await _mediator.Send(new CourseGoodsStockRequest
//                            {
//                                StockCmd = new GoodsStockCommand { Id = cmd.GoodsId, Num = -1 * products.Sum(_ => _.Number) }
//                            });
//                        }
//                        else
//                        {
//                            // at 支付成功 之后
//                            //AsyncUtils.StartNew(new OrderPayedOkEvent { OrderId = order.Id });
//                        }
//                    }
//                }

//                if (order.Status == OrderStatusV2.Paid.ToInt())
//                {
//                    result.Errcode = Consts.Err.PaidBefore;
//                    result.Errmsg = "你已购买该课程,是否重复购买?";
//                    return result;
//                }
//                else if (order.Status == OrderStatusV2.PaidFailed.ToInt())
//                {
//                    order = default;
//                    products = default;
//                    //
//                    // 失败 之前已回归库存
//                    break;
//                }
//                else if (order.Status.In(OrderStatusV2.Cancelled.ToInt(), OrderStatusV2.Completed.ToInt()))
//                {
//                    // 到了这里,会示为新请求订单,不归还库存. 如要归还库存,请在之前判断为' OrderStatusV2.PaidFailed '
//                    //
//                    order = default;
//                    products = default;
//                    break;
//                }

//                // 待支付|支付中 直接调用预支付
//                //
//                repay = true;
//                // 覆盖之前的未支付的订单时,需要注意修正库存
//                if (needfixedStock != 0)
//                {
//                    await _mediator.Send(new CourseGoodsStockRequest
//                    {
//                        StockCmd = new GoodsStockCommand { Id = cmd.GoodsId, Num = -1 * needfixedStock }
//                    });
//                }
//                goto LB_write2db;
//            }
//            while (false);

//            // 正常下单
//            if (!cancellation.IsCancellationRequested)
//            {
//                Debugger.Break();
//                // 下单减库存
//                var stockAfterBuy = (await _mediator.Send(new CourseGoodsStockRequest 
//                {
//                    StockCmd = new GoodsStockCommand { Id = cmd.GoodsId, Num = cmd.BuyAmount }
//                })).StockResult;
//                // 没库存了
//                if (stockAfterBuy <= -2)
//                {
//                    result.Errcode = Consts.Err.NoStock;
//                    result.Errmsg = "没库存了";
//                    return result;
//                }

//                if (isOnRwInviteActivity)
//                {
//                    var consumed = (courseExchange.Point ?? 0) * cmd.BuyAmount;
//                    var score = (await _mediator.Send(new UserScoreOnRwInviteActivityArgs { UnionID = unionID_dto.UnionID }
//                        .SetCourseExchangeType((CourseExchangeTypeEnum)courseExchange.Type)
//                        .PreConsume(consumed)
//                        )).GetResult<double>();

//                    if (score <= -2)
//                    {
//                        // 积分不够 归还库存
//                        try
//                        {
//                            await _mediator.Send(new CourseGoodsStockRequest
//                            {
//                                StockCmd = new GoodsStockCommand { Id = cmd.GoodsId, Num = -1 * cmd.BuyAmount }
//                            });
//                        }
//                        catch { }

//                        throw new CustomResponseException("用户没资格购买该商品", Consts.Err.OrderCreate_UserHasNoScoreToBuy);
//                    }
//                }
//            }

//            // 正常下单 or 更新之前单(有更新信息)
//            LB_write2db:
//            if (true)
//            {
//                // Order
//                if (order == null)
//                {
//                    Debug.Assert(products == null);
//                    order = new Order
//                    {
//                        Id = Guid.NewGuid(),
//                        Code = $"{order_no_prev}{_sxbGenerate.GetNumber()}",
//                        Type = (byte)OrderType.BuyCourseByWx,
//                    };
//                    products = new[]
//                    {
//                        new OrderDetial
//                        {
//                            Id = Guid.NewGuid(),
//                            Orderid = order.Id,
//                        }
//                    };
//                }
//                order.Status = OrderStatusV2.Unpaid.ToInt();
//                order.Paymenttime = null;
//                order.Paymenttype = (byte)(cmd.IsWechatMiniProgram == 0 ? PaymentType.Wx :
//                    cmd.IsWechatMiniProgram == 1 ? PaymentType.Wx_MiniProgram :
//                    cmd.IsWechatMiniProgram == 2 ? PaymentType.Wx_InH5 :
//                    PaymentType.Wx);
//                order.IsValid = true;
//                order.Creator = userId;
//                order.Courseid = course.Id;
//                order.Userid = userId;
//                order.Address = cmd.AddressDto.Address;
//                order.Mobile = cmd.AddressDto.RecvMobile;
//                order.RecvProvince = cmd.AddressDto.Province;
//                order.RecvCity = cmd.AddressDto.City;
//                order.RecvArea = cmd.AddressDto.Area;
//                order.RecvPostalcode = cmd.AddressDto.Postalcode;
//                order.RecvUsername = cmd.AddressDto.RecvUsername;
//                // age
//                {
//                    if (childArchives?.Length > 0)
//                    {
//                        order.ChildArchivesIds = cmd.ChildrenInfoIds!.ToJsonString();
//                        age = childArchives[0].BirthDate is DateTime birth ? GetAge(DateTime.Now, birth) : age;
//                    }
//                    else order.ChildArchivesIds = null;
//                    order.Age = age;
//                }                
//                order.BeginClassMobile = cmd.BeginClassMobile;
//                order.AppointmentStatus = cmd.Ver == "v3" ? (int?)BookingCourseStatusEnum.WaitFor : null;
//                order.Remark = cmd.Remark;
//                // 来源
//                {
//                    var sourceExtend = (object)null;
//                    if (cmd.Source1?.Eid != null)
//                    {
//                        order.SourceType = (byte)OrderCreateFromSource.SchoolFromWx;
//                        order.SourceId = cmd.Source1.Eid.Value;
//                        sourceExtend = new { cmd.Fw, cmd.Source1.Surl, cmd.Source1.Eid };
//                    }
//                    else sourceExtend = new { cmd.Fw };
//                    if (sourceExtend != null) order.SourceExtend = (sourceExtend).ToJsonString(camelCase: true);
//                }

//                var org_info = await _mediator.Send(new OrgzBaseInfoQuery { OrgId = course.Orgid });
//                products[0].Productid = courseGoods.Id;
//                products[0].Courseid = course.Id;
//                products[0].Name = $"{org_info.Name}-{course.Title}"; //\n{string.Join(' ', courseGoods.PropItems.Select(_ => _.Name))}
//                products[0].Status = order.Status;
//                products[0].Number = (short)cmd.BuyAmount;
//                products[0].Price = cmd.Price;
//                products[0].Producttype = course.Type;
//                // set goods-content
//                {
//                    var ctn = new CourseGoodsOrderCtnDto();
//                    ctn.Id = course.Id;
//                    ctn.No = course.No;
//                    ctn.Title = course.Title;
//                    ctn.Subtitle = course.Subtitle;
//                    ctn.Banner = course.Banner?.ToObject<string[]>()?.FirstOrDefault();
//                    ctn.Authentication = org_info.Authentication;
//                    ctn.OrgId = org_info.Id;
//                    ctn.OrgNo = org_info.No;
//                    ctn.OrgName = org_info.Name;
//                    ctn.OrgLogo = org_info.Logo;
//                    ctn.OrgDesc = org_info.Desc;
//                    ctn.OrgSubdesc = org_info.Subdesc;
//                    ctn.GoodsId = courseGoods.Id;
//                    ctn.PropItemIds = courseGoods.PropItems.Select(_ => _.Id).ToArray();
//                    ctn.PropItemNames = courseGoods.PropItems.Select(_ => _.Name).ToArray();
//                    ctn.ProdType = course.Type;
//                    ctn._Ver = cmd.Ver;
//                    ctn._FxHeaducode = cmd.FxHeaducode;
//                    ctn._prebindFxHead_ok = prebindFxHead_ok;
//                    if (isOnRwInviteActivity)
//                    {
//                        ctn._RwInviteActivity = new CourseGoodsOrderCtnDto.RwInviteActivity
//                        {
//                            UnionID = unionID_dto.UnionID,
//                            CourseExchange = courseExchange,
//                            ConsumedScores = (courseExchange.Point ?? 0) * cmd.BuyAmount,
//                        };
//                    }
//                    products[0].Ctn = ctn.ToJsonString(camelCase: true);
//                }
//                products[0].ChildArchives = childArchives.ToJsonString(camelCase: true);
//                products[0].Remark = cmd.Remark;

//                order.CreateTime = DateTime.Now;
//                order.Payment = products.Sum(_ => _.Price * _.Number);
//                order.Freight = 0m;
//                order.Totalpayment = (order.Payment ?? 0m) + (order.Freight ?? 0m);

//                // write_to_db
//                try
//                {
//                    _orgUnitOfWork.BeginTransaction();

//                    if (repay)
//                    {
//                        await _orgUnitOfWork.DbConnection.ExecuteAsync(@"
//                            delete from [Order] where id=@orderid ;
//                            delete from [OrderDetial] where orderid=@orderid;
//                        ", new { orderid = order.Id }, _orgUnitOfWork.DbTransaction);
//                    }

//                    await _orgUnitOfWork.DbConnection.InsertAsync(order, _orgUnitOfWork.DbTransaction);
//                    foreach (var product in products)
//                        await _orgUnitOfWork.DbConnection.InsertAsync(product, _orgUnitOfWork.DbTransaction);

//                    //
//                    // 支付成功后才更新db里的库存.
//                    //
//                    _orgUnitOfWork.CommitChanges();
//                }
//                catch (Exception ex)
//                {
//                    _orgUnitOfWork.SafeRollback();

//                    var msg = GetLogMsg(new { cmd, order, products });
//                    msg.Properties["Error"] = $"下单失败.err={ex.Message}";
//                    msg.Properties["StackTrace"] = ex.StackTrace;
//                    msg.Properties["ErrorCode"] = !repay ? Consts.Err.OrderCreateFailed : Consts.Err.PrevOrderUpdateFailed;
//                    log.Error(msg);

//                    var num = !repay ? cmd.BuyAmount :   // 下新单失败回归库存
//                        repay && needfixedStock > 0 ? -1 * needfixedStock : // repay改单失败可能需要修正库存
//                        0;

//                    if (num != 0)
//                    {
//                        await _mediator.Send(new CourseGoodsStockRequest
//                        {
//                            AddStock = new AddGoodsStockCommand { Id = cmd.GoodsId, Num = num, FromDBIfNotExists = false }
//                        });

//                        if (isOnRwInviteActivity)
//                        {
//                             await _mediator.Send(new UserScoreOnRwInviteActivityArgs { UnionID = unionID_dto.UnionID }
//                                .SetCourseExchangeType((CourseExchangeTypeEnum)courseExchange.Type)
//                                .PreConsume((courseExchange.Point ?? 0) * num));
//                        }
//                    }

//                    throw new CustomResponseException("系统繁忙", !repay ? Consts.Err.OrderCreateFailed : Consts.Err.PrevOrderUpdateFailed);
//                }
//            }

//            // call 预支付       
//            //LB_callpay:
//            result.OrderId = order.Id;
//            Debugger.Break();           
//            {
//                var args_callpay = new WxPayRequest
//                {
//                    AddPayOrderRequest = new ApiWxAddPayOrderRequest
//                    {
//                        UserId = userId,
//                        OrderNo = order.Code,
//                        OrderId = order.Id,
//                        //OrderType = 3,
//                        OrderStatus = order.Status,
//                        TotalAmount = order.Totalpayment ?? 0,
//                        PayAmount = order.Totalpayment ?? 0,
//                        DiscountAmount = 0,
//                        RefundAmount = 0,
//                        Remark = $"上学帮商城订单-订单尾号{order.Code[^6..]}", //"机构wx方式购买课程订单",
//                        OpenId = cmd.OpenId,
//                        //System = 2,
//                        Attach = $"from=org&od={result.OrderId?.ToString("n")}",
//                        OrderByProducts = products.Select(product => new ApiOrderByProduct
//                        {
//                            ProductId = product.Productid,
//                            ProductType = product.Producttype,
//                            Status = product.Status,
//                            Amount = product.Price * product.Number,
//                            Remark = product.Remark,
//                        }).ToArray(),
//                        IsRepay = repay ? 1 : 0,
//                        IsWechatMiniProgram = cmd.IsWechatMiniProgram,
//                        AppId = cmd.AppId,
//                        NoNeedPay = ((order.Totalpayment ?? 0) == 0 ? 1 : 0),
//                    }
//                };                
//                try
//                {                    
//                    result.WxPayResult = (await _mediator.Send(args_callpay)).AddPayOrderResponse;
//                }
//                catch (Exception ex)
//                {
//                    var msg = GetLogMsg(args_callpay);
//                    msg.Properties["Error"] = $"下单后调用支付平台(wxpay)失败.err={ex.Message}";
//                    msg.Properties["StackTrace"] = ex.StackTrace;
//                    msg.Properties["ErrorCode"] = Consts.Err.CallPaidApiError;
//                    log.Error(msg);

//                    // 失败等到订单过期回归库存
//                    // or 下次下单会用回此单,库存在以后处理
//                    throw new CustomResponseException("系统繁忙", Consts.Err.CallPaidApiError);
//                }
//            }            

//            // 预支付成功
//            if (result.WxPayResult != null)
//            {
//                result.PollId = $"{result.OrderId?.ToString("n")}";
//                var args_poll = new PollCallRequest
//                {
//                    PreSetCmd = new PollPreSetCommand
//                    {
//                        Id = CacheKeys.OrderPoll_wxpay_order.FormatWith(result.PollId),
//                        ResultType = typeof(WxPayOkOrderDto).FullName,
//                        ResultStr = (new WxPayOkOrderDto
//                        {
//                            OrderId = order.Id,
//                            OrderNo = order.Code,
//                            //UserPayTime = DateTime.Now,
//                            UserId = userId,
//                            Paymoney = order.Totalpayment ?? 0,
//                            OrderType = order.Type ?? 0,
//                            BuyAmount = products.Sum(_ => _.Number),
//                            CourseId = course.Id,
//                            GoodsId = courseGoods.Id,
//                            FxHeaducode = cmd.FxHeaducode,
//                            _Ver = cmd.Ver,
//                        }).ToJsonString(camelCase: true),
//                        ExpSec = 60 * 60 * 3,
//                    }
//                };
//                try { await _mediator.Send(args_poll); }
//                catch (Exception ex)
//                {
//                    var msg = GetLogMsg(args_poll);
//                    msg.Properties["Error"] = $"下单后创建轮询缓存失败.err={ex.Message}";
//                    msg.Properties["StackTrace"] = ex.StackTrace;
//                    msg.Properties["ErrorCode"] = Consts.Err.PollError;
//                    log.Error(msg);
//                    throw new CustomResponseException("系统繁忙", Consts.Err.PollError);
//                }
//            }

//            // 0元支付 直接变成已支付
//            if ((order.Totalpayment ?? 0) == 0)
//            {
//                AsyncUtils.StartNew(new WxPayRequest 
//                {
//                    WxPayCallback = new WxPayCallbackNotifyMessage
//                    {
//                        OrderId = order.Id,
//                        PayStatus = WxPayCallbackNotifyPayStatus.Success,
//                        AddTime = DateTime.Now.AddSeconds(1),
//                    }
//                });
//            }

//            return result;
//        }

//        NLog.LogEventInfo GetLogMsg(object paramsObj = null)
//        {
//            var msg = new NLog.LogEventInfo();
//            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
//            msg.Properties["Caption"] = "wx购买课程";
//            msg.Properties["UserId"] = me.UserId;
//            msg.Properties["Level"] = "Error";
//            if (paramsObj is string str) msg.Properties["Params"] = str;
//            else if (paramsObj != null) msg.Properties["Params"] = (paramsObj).ToJsonString(camelCase: true);
//            //msg.Properties["Error"] = $"检测敏感词意外失败.网络异常.err={ex.Message}";
//            //msg.Properties["StackTrace"] = ex.StackTrace;
//            //msg.Properties["ErrorCode"] = 3;
//            return msg;
//        }

//        static int GetAge(DateTime now, DateTime birth)
//        {
//            var b = now.Month > birth.Month
//                || (now.Month == birth.Month && now.Day >= birth.Day)
//                || false;

//            return b ? (now.Year - birth.Year) : (now.Year - birth.Year - 1);
//        }
//    }
}
