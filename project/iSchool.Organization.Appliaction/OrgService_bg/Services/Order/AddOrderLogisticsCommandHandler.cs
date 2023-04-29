using Autofac.Features.Indexed;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction
{
    public class AddOrderLogisticsCommandHandler : IRequestHandler<AddOrderLogisticsCommand, bool>
    {

        private readonly IRepository<OrderLogistics> _orderLogisticsRepository;
        private readonly IRepository<OrderDetial> _orderDeatailRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly OrgUnitOfWork _orgUnitOfWork;

        private readonly IMediator _mediator;


        public AddOrderLogisticsCommandHandler(IOrgUnitOfWork unitOfWork, IIndex<string, IBaseRepository<OrderLogistics>> orderLogisticsRepository, IIndex<string, IBaseRepository<OrderDetial>> orderDeatailRepository, IIndex<string, IBaseRepository<Order>> orderRepository, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _orderLogisticsRepository = orderLogisticsRepository["OrgBaseRepository"];
            _orderDeatailRepository = orderDeatailRepository["OrgBaseRepository"];
            _orderRepository = orderRepository["OrgBaseRepository"];
            this._mediator = mediator;

        }

        public Task<bool> Handle(AddOrderLogisticsCommand request, CancellationToken cancellationToken)
        {

            if (request.OrderLogistics.Count(p => p.Number <= 0) > 0)
            {
                throw new CustomResponseException("发货数量存在小于等于零的情况！");
            }
            if (request.OrderLogistics.Count(p => string.IsNullOrEmpty(p.ExpressCode)) > 0 || request.OrderLogistics.Count(p => string.IsNullOrEmpty(p.ExpressType)) > 0)
            {
                throw new CustomResponseException("物流公司或物流单号存在为空的情况！");
            }

            foreach(var ol in request.OrderLogistics)
            {
                KdCompanyCodeDto kdcom = null;
                try { kdcom = (_mediator.Send(KuaidiServiceArgs.CheckNu(ol.ExpressCode, ol.ExpressType)).Result).GetResult<KdCompanyCodeDto>(); }
                catch
                {
                    continue;
                }
                if (kdcom == null)
                {
                    throw new CustomResponseException($"快递单号'{ol.ExpressCode}'与快递公司'{ol.ExpressType}'不匹配。");
                }
            }

            var order = _orgUnitOfWork.QueryFirstOrDefault<Order>("select top 1 * from [order] where id=@id and IsValid=1", new { id = request.OrderId });

            if (order == null)
            {
                throw new CustomResponseException("订单不存在！");
            }

            if (order.Status == (int)OrderStatusV2.RefundOk)
            {
                throw new CustomResponseException("当前订单已经退款！");
            }

            var orderDetails = _orderDeatailRepository.GetAll(p => p.Orderid == request.OrderId);

            if (orderDetails == null || orderDetails.FirstOrDefault(p => p.Id == request.OrderDetailId) == null)
            {
                throw new CustomResponseException("订单详情不存在");
            }

            var orderDetail = orderDetails.FirstOrDefault(p => p.Id == request.OrderDetailId);

            if (orderDetail.Status == (int)OrderStatusV2.RefundOk)
            {
                throw new CustomResponseException("当前商品已经退款！");
            }

            //if ((orderDetail.Status == (int)OrderStatusV2.PartialShipped || orderDetail.Status == (int)OrderStatusV2.Completed || orderDetail.Status == (int)OrderStatusV2.Shipping)&&request.OrderLogistics.Where)

            if (request.OrderLogistics == null || request.OrderLogistics.Count() <= 0)
            {
                throw new CustomResponseException("快递信息不能为空");
            }

            if (request.OrderLogistics.Sum(p => p.Number) > orderDetail.Number)
            {
                throw new CustomResponseException("发货数量不能大于商品数量");
            }

            if ((int)OrderStatusV2.Completed == orderDetail.Status || (int)OrderStatusV2.Shipped == orderDetail.Status)
            {

                if (request.OrderLogistics.Sum(p => p.Number) != orderDetail.Number)
                {
                    throw new CustomResponseException("发货数量不等于商品数量");
                }
            }

            //退款数
            var refundData = _mediator.Send(new OrderOrderRefundsByOrderIdsQuery
            {
                OrderIds = new Guid[] { request.OrderId }
            }).Result.Where(p => p.OrderDetailId == request.OrderDetailId);
            var refundedCount = 0;
            var refundingCount = 0;
            if (refundData.Count() != 0)
            {
                //已退款
                var refundc = refundData.Where(p =>
                 p.Type == (int)RefundTypeEnum.FastRefund
                 || p.Type == (int)RefundTypeEnum.BgRefund
                 || (p.Type == (int)RefundTypeEnum.Refund && p.Status == (int)RefundStatusEnum.RefundSuccess)
                 || (p.Type == (int)RefundTypeEnum.Return && p.Status == (int)RefundStatusEnum.ReturnSuccess));
                refundedCount = refundc.Sum(p => p.Count);


                //退款中
                //--查询审核失败数量
                var FailedCount = refundData.Where(p =>
                p.Status == (int)RefundStatusEnum.InspectionFailed
                  || p.Status == (int)RefundStatusEnum.RefundAuditFailed
                  || p.Status == (int)RefundStatusEnum.ReturnAuditFailed
                  || p.Status == (int)RefundStatusEnum.Cancel
                  || p.Status == (int)RefundStatusEnum.CancelByExpired).Sum(p => p.Count);

                refundingCount = refundData.Sum(p => p.Count) - FailedCount - refundedCount;
            }

            //超出能发货的数量
            if (request.OrderLogistics.Sum(p => p.Number) > orderDetail.Number - refundedCount)
            {
                throw new CustomResponseException("超出能发货的数量");
            }


            //查询订单下所有的物流
            var logistics = _orderLogisticsRepository.GetAll(p => p.OrderId == request.OrderId && p.IsValid == true);



            //更改订单状态   
            //更改订单详情状态
            if (orderDetail.Number - refundedCount > request.OrderLogistics.Sum(p => p.Number))
            {
                //部分发货
                orderDetail.Status = (int)OrderStatusV2.PartialShipped;
                order.Status = (int)OrderStatusV2.PartialShipped;
                order.SendExpressTime = DateTime.Now;
                order.Modifier = request.UserId;
                order.ModifyDateTime = DateTime.Now;
            }
            //订单数量
            else if (orderDetail.Number - refundedCount == request.OrderLogistics.Sum(p => p.Number))
            {
                //待收货
                orderDetail.Status = (int)OrderStatusV2.Shipping;
                if (orderDetails.Where(p => p.Id != request.OrderDetailId && !new int[] { (int)OrderStatusV2.Shipping, (int)OrderStatusV2.RefundOk }.Contains(p.Status)).Any())
                {
                    order.Status = (int)OrderStatusV2.PartialShipped;
                }
                else
                {
                    if (!new int[] { (int)OrderStatusV2.Shipping, (int)OrderStatusV2.Completed }.Contains(order.Status))
                    {
                        order.Status = (int)OrderStatusV2.Shipping;
                        order.SendExpressTime = DateTime.Now;
                        order.ShippingTime = DateTime.Now;
                    }
                }
                order.Modifier = request.UserId;
                order.ModifyDateTime = DateTime.Now;
            }

            //判断是否是多物流

            if (orderDetail.Status == (int)OrderStatusV2.PartialShipped || order.Status == (int)OrderStatusV2.PartialShipped)
            {
                order.IsMultipleExpress = true;
            }
            else
            {
                var exp = request.OrderLogistics.GroupBy(p => new { p.ExpressCode, p.ExpressType });
                if (exp.Count() > 1)
                {
                    order.IsMultipleExpress = true;
                }
                else
                {
                    if (orderDetails.Count() == 1)
                    {
                        order.IsMultipleExpress = false;
                        order.ExpressCode = request.OrderLogistics.First().ExpressCode;
                        order.ExpressType = request.OrderLogistics.First().ExpressType;
                        order.SendExpressTime = DateTime.Now;
                    }
                    else
                    {
                        var otherExp = logistics.Where(p => p.OrderDetailId != request.OrderDetailId).GroupBy(p => p.ExpressCode);
                        if (otherExp.Count() != 1)
                        {
                            order.IsMultipleExpress = true;
                        }
                        else
                        {
                            if (otherExp.First().Key == request.OrderLogistics.First().ExpressCode)
                            {
                                order.IsMultipleExpress = false;
                                order.ExpressCode = request.OrderLogistics.First().ExpressCode;
                                order.ExpressType = request.OrderLogistics.First().ExpressType;
                                order.SendExpressTime = DateTime.Now;
                                order.ShippingTime = DateTime.Now;
                            }
                            else
                            {
                                order.IsMultipleExpress = true;
                            }
                        }
                    }
                }
            }

            if (logistics == null || logistics.FirstOrDefault(p => p.OrderDetailId == request.OrderDetailId) == null)
            {
                //新增操作
                var data = request.OrderLogistics.Select(p => new OrderLogistics()
                {
                    Id = Guid.NewGuid(),
                    ExpressCode = p.ExpressCode,
                    ExpressType = p.ExpressType,
                    Number = p.Number,
                    OrderId = request.OrderId,
                    OrderDetailId = request.OrderDetailId,
                    SendExpressTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                    Creator = request.UserId,
                    Modifier = request.UserId,
                    ModifyDateTime = DateTime.Now,
                    IsValid = true
                }).ToList();


                try
                {
                    _orgUnitOfWork.CommitChanges();

                    _orgUnitOfWork.BeginTransaction();
                    _orderDeatailRepository.Update(orderDetail);
                    _orderRepository.Update(order);
                    _orderLogisticsRepository.BatchInsert(data);


                    _orgUnitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    _orgUnitOfWork.SafeRollback();
                    throw new CustomResponseException($"系统错误：【{ex.Message}】");
                }
            }
            else
            {
                //软删除数据
                var ids = request.OrderLogistics.Where(p => p.OrderLogisticsId != null).Select(p => p.OrderLogisticsId.Value);

                var delData = logistics.Where(p => !ids.Contains(p.Id) && p.OrderDetailId == request.OrderDetailId).ToList();
                delData.ForEach(p =>
                  {
                      p.Modifier = request.UserId;
                      p.ModifyDateTime = DateTime.Now;
                      p.IsValid = false;
                  });

                var updateData = logistics.Where(p => ids.Contains(p.Id)).ToList();

                updateData.ForEach(p =>
                {
                    p.ExpressCode = request.OrderLogistics.FirstOrDefault(c => c.OrderLogisticsId == p.Id)?.ExpressCode;
                    p.ExpressType = request.OrderLogistics.FirstOrDefault(c => c.OrderLogisticsId == p.Id)?.ExpressType;
                    p.Number = request.OrderLogistics.FirstOrDefault(c => c.OrderLogisticsId == p.Id)?.Number ?? 0;
                    p.Modifier = request.UserId;
                    p.ModifyDateTime = DateTime.Now;
                });


                var insertData = request.OrderLogistics
                    .Where(p => p.OrderLogisticsId == null)
                    .Select(p => new OrderLogistics
                    {
                        Id = Guid.NewGuid(),
                        ExpressCode = p.ExpressCode,
                        ExpressType = p.ExpressType,
                        Number = p.Number,
                        OrderId = request.OrderId,
                        OrderDetailId = request.OrderDetailId,
                        SendExpressTime = DateTime.Now,
                        CreateTime = DateTime.Now,
                        Creator = request.UserId,
                        Modifier = request.UserId,
                        ModifyDateTime = DateTime.Now,
                        IsValid = true
                    }).ToList();



                try
                {
                    _orgUnitOfWork.CommitChanges();

                    _orgUnitOfWork.BeginTransaction();
                    _orderDeatailRepository.Update(orderDetail);
                    _orderRepository.Update(order);
                    _orderLogisticsRepository.BatchUpdate(delData);
                    _orderLogisticsRepository.BatchInsert(insertData);
                    _orderLogisticsRepository.BatchUpdate(updateData);

                    _orgUnitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    _orgUnitOfWork.SafeRollback();
                    throw new CustomResponseException($"系统错误：【{ex.Message}】");
                }
            }



            //微信通知
            var ctn = orderDetail.Ctn;
            var ctnData = JsonConvert.DeserializeObject<CourseGoodsOrderCtnDto>(ctn);
            //微信推送
            var openid = _orgUnitOfWork.Query<string>($@" select openID from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and userID='{order.Userid}'; ").FirstOrDefault();
            if (openid == null)
            {
                throw new CustomResponseException($"发货成功，但用户Id[{request.UserId}]在[iSchoolUser].[dbo].[openid_weixin]中无有效记录,无法推送");
            }

            var openId = openid;//代写
            request.OrderLogistics //.Where(p => p.OrderLogisticsId != null)
                .GroupBy(p => new { p.ExpressCode, p.ExpressType })
                .Select(p => new { Key = p.Key, Count = p.Sum(c => c.Number) })
                .ToList()
                .ForEach(p =>
             {
                 var kdcom = _mediator.Send(KuaidiServiceArgs.GetCode(p.Key.ExpressType)).Result.GetResult<KdCompanyCodeDto>();
                 var deleverCompany = string.IsNullOrEmpty(kdcom?.Com) ? "" : $",{kdcom?.Com}："; //物流公司


                 var deleverNo = p.Key.ExpressCode;//物流单号
                 var msg = $"{deleverCompany}{deleverNo}";


                 if (orderDetail.Status == (int)(OrderStatusV2.PartialShipped))
                 {
                     var wechatNotify = new WechatTemplateSendCmd()
                     {

                         KeyWord1 = $"您购买的《{ctnData.Title}》{p.Count}件商品已部分发货{msg}，请留意物流信息。",
                         KeyWord2 = DateTime.Now.ToDateTimeString(),
                         OpenId = openId,
                         Remark = "点击下方【详情】查看订单发货详情",
                         MsyType = WechatMessageType.部分发货,
                         OrderID = request.OrderId

                     };
                     var res = _mediator.Send(wechatNotify).Result;
                 }
                 else if (orderDetail.Status == (int)OrderStatusV2.Shipping)
                 {
                     var wechatNotify = new WechatTemplateSendCmd()
                     {

                         KeyWord1 = $"您购买的《{ctnData.Title}》{p.Count}件商品已全部发货{msg}，请留意物流信息。",
                         KeyWord2 = DateTime.Now.ToDateTimeString(),
                         OpenId = openId,
                         Remark = "点击下方【详情】查看订单发货详情",
                         MsyType = WechatMessageType.物流,
                         OrderID = request.OrderId
                     };
                     var res = _mediator.Send(wechatNotify).Result;
                 }
             });
            return Task.FromResult(true);
        }
    }
}
