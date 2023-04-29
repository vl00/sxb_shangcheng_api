using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.IntegrationEvents;
using iSchool.Organization.Appliaction.IntegrationEvents.Events;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.Wechat;

namespace iSchool.Organization.Appliaction.Services
{
    public class RefundApplyCmdHandler : IRequestHandler<RefundApplyCmd, RefundApplyCmdResult>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly CSRedisClient _redis;
        private readonly IConfiguration _config;
        private readonly IUserInfo me;
        private readonly ISxbGenerateNo _sxbGenerate;
        private readonly NLog.ILogger log;
        private readonly ILock1Factory _lock1Factory;
        private readonly IServiceProvider services;

        public RefundApplyCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, ILock1Factory lock1Factory,
            IUserInfo me, ISxbGenerateNo sxbGenerate, NLog.ILogger log,
            IConfiguration config, IServiceProvider services)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this.me = me;
            this._lock1Factory = lock1Factory;
            this._sxbGenerate = sxbGenerate;
            this.log = log;
            this.services = services;
        }

        public async Task<RefundApplyCmdResult> Handle(RefundApplyCmd cmd, CancellationToken cancellation)
        {
            var result = new RefundApplyCmdResult();
            var isFastRefund = cmd.RefundType == (int)RefundTypeEnum.FastRefund;
            string sql = null;
            var is_orderDetail_willbe_FullRefundOk = false;
            var is_order_willbe_FullRefundOk = false;
            Debugger.Break();

            #region valid cmd
            if (cmd.RefundCount < 1)
            {
                throw new CustomResponseException("退款数量必须大于等于1");
            }
            if (!isFastRefund && !Enum.IsDefined(typeof(RefundCauseEnum), (int)cmd.Cause))
            {
                throw new CustomResponseException("请选择退款原因");
            }
            if (cmd.RefundType == (int)RefundTypeEnum.Return && !Enum.IsDefined(typeof(RefundReturnModeEnum), (int)cmd.ReturnMode))
            {
                throw new CustomResponseException("请选择退货方式");
            }
            if (cmd.Desc?.Length > 200)
            {
                throw new CustomResponseException("补充描述最多200个字符");
            }
            if (cmd.Vouchers?.Length != cmd.Vouchers_s?.Length)
            {
                throw new CustomResponseException("凭证图片参数长度不一样");
            }
            if (cmd.Vouchers?.Length > 9)
            {
                throw new CustomResponseException("凭证图片数量不能超过9张");
            }
            #endregion valid cmd

            await using var _lck = await _lock1Factory.LockAsync(CacheKeys.Refund_applyLck.FormatWith(me.UserId), 3 * 60 * 1000);
            if (!_lck.IsAvailable) throw new CustomResponseException("系统繁忙", Consts.Err.RefundApplyCheck_CannotGetLck);

            // 检查当前可用申请
            var currCanApply = await _mediator.Send(new RefundApplyQry { OrderDetailId = cmd.OrderDetailId, IsInLck = true });
            //
            if (currCanApply._OrderDetial.Point.GetValueOrDefault() > 0)
            {
                throw new CustomResponseException("积分支付订单不支持退款", Consts.Err.RefundApply_PointsPay);
            }
            if (currCanApply.RefundMoney <= 0)
            {
                throw new CustomResponseException("0元订单不支持退款", Consts.Err.RefundApply_MoneyIsZero);
            }
            if (cmd.RefundCount > currCanApply.RefundCount)
            {
                throw new CustomResponseException("申请总数量不能高于购买数量", Consts.Err.RefundApply_OverCount);
            }
            if (cmd.RefundType > (int)RefundTypeEnum.FastRefund)
            {
                throw new CustomResponseException("非法操作", Consts.Err.RefundApply_TypeError);
            }
            if (cmd.RefundType == (int)RefundTypeEnum.FastRefund && !currCanApply.IsFastRefund)
            {
                //* 2021.11.24 去掉时间限制,未出库前都可以极速退款
                if (false) // (DateTime.Now - currCanApply.Paytime).TotalMinutes >= 30
                    throw new CustomResponseException("订单已超时, 极速退款失效, 请重新申请退款", Consts.Err.RefundApply_CannotFastRefund_30minTimeout);
                else
                    throw new CustomResponseException("商品已出库, 极速退款失效, 请重新申请退款", Consts.Err.RefundApply_CannotFastRefund_StatusError);
            }
            // 类型不一样
            if (currCanApply.RefundType != (cmd.RefundType.In((int)RefundTypeEnum.Refund, (int)RefundTypeEnum.Return) ? (int)RefundTypeEnum.Refund : cmd.RefundType))
            {
                throw new CustomResponseException("非法操作", Consts.Err.RefundApply_TypeNotSame);
            }
            // 极速退款之前不可能有其他类型的退款退货...
            if (isFastRefund && currCanApply._RfdCounts.RefundingCount > 0)
            {
                throw new CustomResponseException("系统繁忙", Consts.Err.RefundApply_FastRefund_HasOtherApplyBefore);
            }

            var refundPrices =   currCanApply._OrderDetial.RefundSpreadPrice(cmd.RefundCount);
            var orderRefund = new OrderRefunds();
            orderRefund.Id = Guid.NewGuid();
            orderRefund.IsValid = true;
            orderRefund.Code = $"{Consts.Prev_RefundCode}{_sxbGenerate.GetNumber()}";
            orderRefund.OrderId = currCanApply.OrderId;
            orderRefund.OrderDetailId = cmd.OrderDetailId;
            orderRefund.ProductId = currCanApply.Item.GoodsId;
            orderRefund.Type = (byte)cmd.RefundType;
            orderRefund.Count = (byte)cmd.RefundCount;
            orderRefund.Price = refundPrices.Sum(s => s.refundAmount); // 总价
            orderRefund.SpecialReason = Domain.Enum.OrderRefundSpecialReason.Nothing;
            orderRefund.IsContainFreight = false;
            orderRefund.CreateTime = DateTime.Now;
            orderRefund.RefundUserId = me.UserId;            
            if (isFastRefund)
            {
                orderRefund.Status = (byte)RefundStatusEnum.RefundSuccess;
                orderRefund.RefundTime = orderRefund.CreateTime;
                orderRefund.RefundPrice = orderRefund.Price;
            }
            else
            {
                orderRefund.Status = (byte)(cmd.RefundType == (int)RefundTypeEnum.Refund 
                    ? (currCanApply._OrderDetial.Status == (int)OrderStatusV2.Paid ? RefundStatusEnum.RefundAudit2 : RefundStatusEnum.RefundAudit1) : RefundStatusEnum.ReturnAudit);
                orderRefund.Cause = cmd.Cause;
                //? = cmd.ReturnMode;
                orderRefund.Reason = cmd.Desc;
                orderRefund.Voucher = cmd.Vouchers?.ToJsonString(); 
                orderRefund.Voucher_s = cmd.Vouchers_s?.ToJsonString();

                if (cmd.RefundType == (int)RefundTypeEnum.Return)
                {
                    //{ "Addr":"广州","Receiver":"深海","Phone":"18268121359"}                    
                    try
                    {
                        var addressJson = (await _orgUnitOfWork.ExecuteScalarAsync<string>(@"
                            select top 1 a.returnaddress from [SupplierAddress] a 
                            join CourseGoods g on g.SupplieAddressId=a.id
                            where g.id=@ProductId", new { orderRefund.ProductId })
                        ) ?? "{ \"Addr\":null,\"Receiver\":null,\"Phone\":null}";

                        var addressJobj = JToken.Parse(addressJson);
                        orderRefund.SendBackAddress = addressJobj["Addr"].Value<string>();
                        orderRefund.SendBackUserName = addressJobj["Receiver"].Value<string>();
                        orderRefund.SendBackMobile = addressJobj["Phone"].Value<string>();
                    }
                    catch { }
                }
            }

            // check orderdetail if will be true full refund ok
            if (isFastRefund && cmd.RefundCount == currCanApply.RefundCount)
            {
                var allNum = currCanApply._OrderDetial.Number;
                is_orderDetail_willbe_FullRefundOk = allNum > 0 && allNum == (currCanApply._RfdCounts.OkCount + cmd.RefundCount);
            }
            // check order if will be full refund ok
            if (isFastRefund && is_orderDetail_willbe_FullRefundOk)
            {
                sql = $"select count(1),sum(case when [status]={OrderStatusV2.RefundOk.ToInt()} then 1 else 0 end) from [{nameof(OrderDetial)}] where orderid=@OrderId ";
                var (c0, c1) = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<(int, int)>(sql, new { currCanApply.OrderId });
                is_order_willbe_FullRefundOk = c0 > 0 && c0 == (c1 + 1);
            }
            //
            // 极速退款 orderDetail退到了最后, 要加上最后的零碎的余钱
            var fast_productInfos = new List<RefundCmd.ProductInfo>();
            if (isFastRefund  && currCanApply._OrderDetial.Payment > 0)
            {
                var orderDetail = currCanApply._OrderDetial;
                #region
                //var surplus = orderDetail.Payment - orderDetail.Price * orderDetail.Number; // 余钱
                //orderRefund.Price += surplus;
                //if (surplus > 0)
                //{
                //    fast_productInfos[0].RefundProductNum -= 1;
                //    if (fast_productInfos[0].RefundProductNum <= 0) fast_productInfos.RemoveAt(0);
                //    fast_productInfos.Add(new RefundCmd.ProductInfo 
                //    {
                //        RefundProductNum = 1,
                //        RefundProductPrice = fast_productInfos[0].RefundProductPrice + surplus,
                //    });
                //}
                #endregion
                fast_productInfos.AddRange(
                    refundPrices.Select(_ => new RefundCmd.ProductInfo { RefundProductNum = _.number, RefundProductPrice = _.unitPrice, Amount = _.refundAmount })
                );
            }
            // 是否是最后一单要退运费. 极速退款需要判断, 其他类型不需要
            if (isFastRefund && is_order_willbe_FullRefundOk)
            {
                var freight = currCanApply._Order.Freight ?? 0;
                orderRefund.IsContainFreight = freight > 0;
                // 后续orderRefund.Price加上运费
            }

            // 极速退款 就直接退款
            if (isFastRefund)
            {
                // 退商品数量
                var rr = await _mediator.Send(new RefundCmd
                {
                    AdvanceOrderId = currCanApply._Order.AdvanceOrderId!.Value,
                    OrderId = orderRefund.OrderId,
                    OrderDetailId = orderRefund.OrderDetailId,
                    ProductId = orderRefund.ProductId,
                    Remark = "用户申请极速退款",

                    //RefundProductNum = orderRefund.Count,
                    //RefundAmount = orderRefund.Price,
                    RefundProductInfo = fast_productInfos,

                    RefundType = 3,
                });
                if (!rr.ApplySucess)
                {
                    throw new CustomResponseException("极速退款失败", Consts.Err.RefundApply_FastRefund_CallApiError);
                }
                // 后续发wx通知

                // 退运费
                if (orderRefund.IsContainFreight == true && (currCanApply._Order.Freight ?? 0) is decimal freight && freight > 0)
                {
                    #region 退运费在后续事件中进行
                    //rr = await _mediator.Send(new RefundCmd
                    //{
                    //    AdvanceOrderId = currCanApply._Order.AdvanceOrderId!.Value,
                    //    OrderId = orderRefund.OrderId,
                    //    OrderDetailId = orderRefund.OrderDetailId,
                    //    RefundAmount = freight,
                    //    Remark = "极速退款退运费",
                    //    RefundType = 4,
                    //});
                    //if (!rr.ApplySucess)
                    //{
                    //    throw new CustomResponseException("极速退款退运费失败", Consts.Err.RefundApply_FastRefund_CallApiError);
                    //}
                    //// 发wx通知？
                    
                    // orderRefund.Price += freight;
                    #endregion 退运费在后续事件中进行
                }
            }

            // write db
            try
            {
                _orgUnitOfWork.BeginTransaction();

                await _orgUnitOfWork.DbConnection.InsertAsync(orderRefund, _orgUnitOfWork.DbTransaction);                

                // if is 极速退款
                for (var __ = isFastRefund; __; __ = !__)
                {             
                    // up RefundCount计数
                    sql = $@"
update [{nameof(OrderDetial)}] set [status]={(is_orderDetail_willbe_FullRefundOk ? $"{OrderStatusV2.RefundOk.ToInt()}" : "[status]")},[RefundCount]=isnull([RefundCount],0)+@Count
{(!is_orderDetail_willbe_FullRefundOk ? "" : $",[RefundTime]=@RefundTime,[RefundUserId]=@RefundUserId")}
where id=@OrderDetailId 
";
                    await _orgUnitOfWork.ExecuteAsync(sql, new { cmd.OrderDetailId, orderRefund.Count, orderRefund.RefundTime, orderRefund.RefundUserId }, _orgUnitOfWork.DbTransaction);

                    //if (!is_orderDetail_willbe_FullRefundOk) break;

                    // up order if is_order_willbe_FullRefundOk
                    sql = $@"--[status]={(is_order_willbe_FullRefundOk ? $"{OrderStatusV2.RefundOk.ToInt()}" : "[status]")},
update [order] set  [IsPartialRefund]={(is_order_willbe_FullRefundOk ? 0 : 1)},ModifyDateTime=getdate(),Modifier=@UserId
where id=@OrderId and IsValid=1
";
                    await _orgUnitOfWork.ExecuteAsync(sql, new { currCanApply.OrderId, me.UserId }, _orgUnitOfWork.DbTransaction);
                }

                _orgUnitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.SafeRollback();
                log.Info(GetLogMsg(cmd).SetError(ex, Consts.Err.RefundApply_WriteDbError));
                throw new CustomResponseException("系统繁忙", Consts.Err.RefundApply_WriteDbError);
            }

            if (!isFastRefund && currCanApply._Order.Status == (int)OrderStatusV2.Shipping)
            {
                // 待收货时申请退款 暂停自动确认收货计时器...
                //...
            }

            // 极速退款成功后续事件
            if (isFastRefund)
            {
                // 退优惠劵等...
                if (is_orderDetail_willbe_FullRefundOk)
                {
                    AsyncUtils.StartNew(new Domain.Event.Order.OrderDetailTransferToRefundStateDomainEvent(cmd.OrderDetailId));
                }


                AsyncUtils.StartNew(async (sp, _) => 
                {
                    //await Task.Delay(1000);
                    await sp.GetService<IMediator>().Publish(new Domain.Event.OrderRefundSuccessDomainEvent { OrderRefundId = orderRefund.Id });
                });
            }
            // 申请成功发wx通知
            if (!isFastRefund)
            {
                try
                {
                    await _mediator.Send(new SendWxTemplateMsgCmd
                    {
                        UserId = me.UserId,
                        WechatTemplateSendCmd = new WechatTemplateSendCmd
                        {
                            KeyWord1 = $"您已成功发起《{currCanApply.Item.Title}》{cmd.RefundCount}件商品{(cmd.RefundType == (int)RefundTypeEnum.Refund ? "退款" : "退货退款")}申请，审核人员将尽快为您处理，请耐心等候",
                            KeyWord2 = DateTime.Now.ToDateTimeString(),
                            Remark = "点击下方【查看详情】查看申请详情",
                            MsyType = WechatMessageType.成功发起退货or退款申请时,
                            Args = new Dictionary<string, object> 
                            {
                                ["id"] = orderRefund.Id.ToString(),
                            }
                        }
                    });
                }
                catch { }
            }

            result.Id = orderRefund.Id;
            result.Code = orderRefund.Code;
            return result;
        }

        NLog.LogEventInfo GetLogMsg(object paramsObj = null)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
            msg.Properties["Caption"] = "退款提交申请";
            msg.Properties["UserId"] = me.UserId;
            msg.Properties["Level"] = "Error";
            if (paramsObj is string str) msg.Properties["Params"] = str;
            else if (paramsObj != null) msg.Properties["Params"] = (paramsObj).ToJsonString(camelCase: true);
            msg.Properties["Class"] = nameof(RefundApplyCmdHandler);
            //msg.Properties["Error"] = $"检测敏感词意外失败.网络异常.err={ex.Message}";
            //msg.Properties["StackTrace"] = ex.StackTrace;
            //msg.Properties["ErrorCode"] = 3;
            return msg;
        }
    }
}
