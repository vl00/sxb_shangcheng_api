using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetOrderLogisticsDetailQueryHandler : IRequestHandler<GetOrderLogisticsDetailQuery, KuaidiDetailDto>
    {
        private readonly IRepository<OrderLogistics> _orderLogisticsRepository;
        private readonly IRepository<Domain.Order> _orderRepository;
        private readonly IMediator _mediator;

        public GetOrderLogisticsDetailQueryHandler(IRepository<OrderLogistics> orderLogisticsRepository, IRepository<Domain.Order> orderRepository, IMediator mediator)
        {
            _orderLogisticsRepository = orderLogisticsRepository;
            _orderRepository = orderRepository;
            _mediator = mediator;
        }

        public async Task<KuaidiDetailDto> Handle(GetOrderLogisticsDetailQuery request, CancellationToken cancellationToken)
        {


            var orderlogistics = _orderLogisticsRepository.Get(p => p.Id == request.LogisticId && p.IsValid == true);
            if (orderlogistics == null)
            {
                throw new CustomResponseException("不存在当前快递详情");
            }
            var order = _orderRepository.Get(p => p.Id == orderlogistics.OrderId && p.IsValid == true);
            if (order == null)
            {
                throw new CustomResponseException("该订单不存在");
            }
            var res = await _mediator.Send(new GetKuaidiDetailQuery
            {
                ExpressCode = orderlogistics.ExpressCode,
                ExpressType = orderlogistics.ExpressType,
                SendExpressTime = orderlogistics.SendExpressTime,
                Address = order.Address,
                City = order.RecvCity,
                Area = order.RecvArea,
                Province = order.RecvProvince,
                RecvMobile = order.Mobile,
                Postalcode = order.RecvPostalcode,
                RecvUsername = order.RecvUsername

            });

            return res;
        }
    }
}
