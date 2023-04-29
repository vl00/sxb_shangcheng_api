using Autofac.Features.Indexed;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services.Order
{
    public class OrderLogisticsByOrderIdQueryHandler : IRequestHandler<OrderLogisticsByOrderIdQuery, OrderLogisticsDto>
    {

        private readonly IRepository<OrderLogistics> _orderLogisticsRepository;
        private readonly IRepository<OrderDetial> _orderDeatailRepository;
        private readonly IMediator _mediator;


        public OrderLogisticsByOrderIdQueryHandler(IIndex<string, IBaseRepository<OrderDetial>> orderDeatailRepository, IIndex<string, IBaseRepository<OrderLogistics>> orderLogisticsRepository, IMediator mediator)
        {
            _orderLogisticsRepository = orderLogisticsRepository["OrgBaseRepository"];
            _orderDeatailRepository = orderDeatailRepository["OrgBaseRepository"];
            _mediator = mediator;
        }

        public Task<OrderLogisticsDto> Handle(OrderLogisticsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            var detail = _orderDeatailRepository.Get(p => p.Id == request.OrderDetailId);
            if (detail == null)
            {
                throw new CustomResponseException("订单详情不存在");
            }

            var logistics = _orderLogisticsRepository.GetAll(p =>
            p.OrderDetailId == request.OrderDetailId && p.IsValid == true);

            OrderLogisticsDto dto = new OrderLogisticsDto
            {
                Count = detail.Number,
                OrderDetailId = detail.Id,
                OrderId = detail.Orderid,
                OrderStatus = detail.Status,
                LogisticCount = 0,
                LogisticDataList = new List<LogisticData>()
            };

            if (logistics != null && logistics.Count() > 0)
            {
                dto.LogisticCount = logistics.Sum(p => p.Number);
                dto.LogisticDataList = logistics.Select(p => new LogisticData { ExpressCode = p.ExpressCode, Number = p.Number, ExpressType = p.ExpressType, SendExpressTime = p.SendExpressTime }).ToList();
            }

            var refundData = _mediator.Send(new OrderOrderRefundsByOrderIdsQuery
            {
                OrderIds = new Guid[] { detail.Orderid }
            }).Result.Where(p => p.OrderDetailId == detail.Id);

            if (refundData.Count() != 0)
            {
                //已退款
                var refundc = refundData.Where(p =>
                 p.Type == (int)RefundTypeEnum.FastRefund
                 || p.Type == (int)RefundTypeEnum.BgRefund
                 || (p.Type == (int)RefundTypeEnum.Refund && p.Status == (int)RefundStatusEnum.RefundSuccess)
                 || (p.Type == (int)RefundTypeEnum.Return && p.Status == (int)RefundStatusEnum.ReturnSuccess));

                dto.RefundedCount = refundc.Sum(p => p.Count);


                //退款中
                //--查询审核失败数量
                var FailedCount = refundData.Where(p =>
                p.Status == (int)RefundStatusEnum.InspectionFailed
                  || p.Status == (int)RefundStatusEnum.RefundAuditFailed
                  || p.Status == (int)RefundStatusEnum.ReturnAuditFailed
                  || p.Status == (int)RefundStatusEnum.Cancel
                  || p.Status == (int)RefundStatusEnum.CancelByExpired).Sum(p => p.Count);


                dto.RefundingCount = refundData.Sum(p => p.Count) - FailedCount - dto.RefundedCount;
            }


            return Task.FromResult(dto);
        }
    }
}
