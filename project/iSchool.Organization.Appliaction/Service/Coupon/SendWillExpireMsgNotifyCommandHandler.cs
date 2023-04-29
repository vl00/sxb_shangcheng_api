using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class SendWillExpireMsgNotifyCommandHandler : IRequestHandler<SendWillExpireMsgNotifyCommand>
    {
        IMediator _mediator;
        ICouponReceiveRepository _couponReceiveRepository;
        ILogger<SendWillExpireMsgNotifyCommandHandler> _logger;
        public SendWillExpireMsgNotifyCommandHandler(ICouponReceiveRepository couponReceiveRepository
            , IMediator mediator
            , ILogger<SendWillExpireMsgNotifyCommandHandler> logger)
        {
            _couponReceiveRepository = couponReceiveRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Unit> Handle(SendWillExpireMsgNotifyCommand request, CancellationToken cancellationToken)
        {
           var couponReceive =  await _couponReceiveRepository.FindAsync(request.CouponReceiveId);
            if (couponReceive == null) throw new KeyNotFoundException();
            _couponReceiveRepository.UnitOfWork.BeginTransaction();
            try
            {
                if (!couponReceive.SendWillExpireMsgNotifyEvent()) throw new Exception(" couponReceive.SendWillExpireMsgNotifyEvent 失败。");
                if (await _couponReceiveRepository.UpdateAsync(couponReceive, nameof(couponReceive.WillExpireMessageNotify)))
                {
                    await _mediator.DispatchDomainEventsAsync(couponReceive);
                }
                _couponReceiveRepository.UnitOfWork.CommitChanges();


            }
            catch(Exception ex){
                _couponReceiveRepository.UnitOfWork.Rollback();
                _logger.LogError(ex, $"couponReceiveId = {couponReceive.Id}");
            }
            return Unit.Value;

            
        }
    }
}
