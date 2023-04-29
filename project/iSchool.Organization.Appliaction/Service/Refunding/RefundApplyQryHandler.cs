using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace iSchool.Organization.Appliaction.Services
{
    public class RefundApplyQryHandler : IRequestHandler<RefundApplyQry, RefundApplyQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        IUserInfo me;
        ILock1Factory _lock1Factory;
        IMapper _mapper;

        public RefundApplyQryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IUserInfo me,
            ILock1Factory lock1Factory, IMapper mapper,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._lock1Factory = lock1Factory;
            this.me = me;
            this._mapper = mapper;
        }

        public async Task<RefundApplyQryResult> Handle(RefundApplyQry query, CancellationToken cancellation)
        {
            var result = new RefundApplyQryResult { OrderDetailId = query.OrderDetailId };
            //Debugger.Break();

            await using var _lck = query.IsInLck ? null : await _lock1Factory.LockAsync(CacheKeys.Refund_applyLck.FormatWith(me.UserId));
            if (_lck?.IsAvailable == false) throw new CustomResponseException("系统繁忙", Consts.Err.RefundApplyCheck_CannotGetLck);

            var sql = "select * from [OrderDetial] where id=@OrderDetailId";
            var orderDetail = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<OrderDetial>(sql, new { query.OrderDetailId });
            if (orderDetail == null) throw new CustomResponseException("订单不存在", 404);
            if (orderDetail.Producttype == (byte)CourseTypeEnum.Course) throw new CustomResponseException("网课不支持退款");
            
            sql = "select * from [order] where type>=2 and IsValid=1 and id=@OrderId ";
            var order = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<Order>(sql, new { orderDetail.Orderid });
            if (order == null) throw new CustomResponseException("订单不存在", 404);
            if (order.Paymenttime == null) throw new CustomResponseException("操作失败", Consts.Err.RefundApplyCheck_NoPaytime);
            if (order.Userid != me.UserId) throw new CustomResponseException("非法操作", 500);

            // == 103 || ( > 300 && != 333 )
            var is_status_can_refund = order.Status == (int)OrderStatusV2.Paid || (order.Status > 300 && order.Status != (int)OrderStatusV2.Completed);
            if (!is_status_can_refund) throw new CustomResponseException("当前订单状态不能退款");
            
            var fnums = await _mediator.Send(new OrderDetailRefundCountsQryArgs { OrderDetailId = orderDetail.Id });
            // 剩余可用申请数
            var avalNum = orderDetail.Number - (fnums.OkCount + fnums.RefundingCount);
            if (avalNum <= 0)
            {
                if (avalNum == 0) throw new CustomResponseException("商品已全部申请退款了", Consts.Err.RefundApplyCheck_ApplyCountIsOver);
                throw new CustomResponseException("申请总数量不能高于购买数量", Consts.Err.RefundApplyCheck_ApplyCountIsOver);
            }
            result._RfdCounts = fnums;

            // 判断退款类型
            result.RefundType = -1;
            {
                var min = (DateTime.Now - order.Paymenttime!.Value).TotalMinutes;
                switch (order.Status)
                {
                    //* 2021.11.24 去掉时间限制,未出库前都可以极速退款
                    //
                    //// 支付后30min后, 为售后
                    //case int _ when (min >= 30):
                    //    result.RefundType = (int)RefundTypeEnum.Refund;
                    //    break;

                    // 支付后30min内, Status=103 为极速退款
                    case int _ when (orderDetail.Status == (int)OrderStatusV2.Paid):
                        result.RefundType = (int)RefundTypeEnum.FastRefund;
                        break;

                    // 301出库对整个orderDetail
                    case int _ when (orderDetail.Status == (int)OrderStatusV2.ExWarehouse):
                        result.RefundType = (int)RefundTypeEnum.Refund;
                        break;
                    // 302 已全发货
                    case int _ when (orderDetail.Status == (int)OrderStatusV2.Shipping):
                        result.RefundType = (int)RefundTypeEnum.Refund;
                        break;

                    // 303部分发货 优先极速退款并且退未发货的数量
                    default:
                        {
                            // 发货数(有物流)
                            sql = "select sum(number) from OrderLogistics w where w.IsValid=1 and w.OrderDetailId=@OrderDetailId";
                            var c = await _orgUnitOfWork.DbConnection.ExecuteScalarAsync<int>(sql, new { query.OrderDetailId });
                            //
                            // 可申请极速退款的未发货数
                            var d = avalNum - c;
                            if (d > 0)
                            {
                                avalNum = d;
                                result.RefundType = (int)RefundTypeEnum.FastRefund;
                            }
                            else
                            {
                                result.RefundType = (int)RefundTypeEnum.Refund;
                            }
                        }
                        break;
                }
            }
            if (result.RefundType <= 0)
            {
                throw new CustomResponseException("当前订单状态不能退款", Consts.Err.RefundApplyCheck_NotFound_RefundType);
            }

            result._Order = order;
            result._OrderDetial = orderDetail;
            result.Item = OrderHelper.ConvertTo_CourseOrderProdItemDto(orderDetail);
            // 默认退可用的全部数量
            result.Item.BuyCount = avalNum;
            result.RefundMoney = result.Item.PricesAll;

            // 其他界面数据s
            {
                result.RefundServiceTypes = new[]
                {
                    KeyValuePair.Create(RefundTypeEnum.Refund.ToInt(), "仅退款"),
                    KeyValuePair.Create(RefundTypeEnum.Return.ToInt(), "退货退款"),
                };

                var causes = EnumUtil.GetDescs<RefundCauseEnum>().Select(_ => KeyValuePair.Create(_.Value.ToInt(), _.Desc));
                result.RefundCauses1 = causes.Where(_ => _.Key < 10).ToArray();
                result.RefundCauses2 = causes.Where(_ => _.Key > 10).ToArray();

                result.ReturnModes2 = EnumUtil.GetDescs<RefundReturnModeEnum>().Select(_ => KeyValuePair.Create(_.Value.ToInt(), _.Desc));
            }

            return result;
        }

    }
}
