using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.Event.Coupon;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class CouponLoseEfficacyCommandHandler : IRequestHandler<CouponLoseEfficacyCommand, bool>
    {
        ICouponInfoRepository _couponInfoRepository;
        IMediator _mediator;
        public CouponLoseEfficacyCommandHandler(ICouponInfoRepository couponInfoRepository, IMediator mediator)
        {
            _couponInfoRepository = couponInfoRepository;
            _mediator = mediator;
        }
        public async Task<bool> Handle(CouponLoseEfficacyCommand request, CancellationToken cancellationToken)
        {
            var couponInfo = await _couponInfoRepository.GetAsync(request.Id);
            if (couponInfo == null)
            {
                throw new KeyNotFoundException();
            }

            couponInfo.SetStatus(CouponInfoState.LoseEfficacy);
            couponInfo.AddDomainEvent(new CouponLoseEfficacyDomainEvent(couponInfo));
            _couponInfoRepository.UnitOfWork.BeginTransaction();
            try
            {
                await _couponInfoRepository.UpdateAsync(couponInfo, nameof(couponInfo.Status));
                await _mediator.DispatchDomainEventsAsync(new List<Entity> { couponInfo });
                _couponInfoRepository.UnitOfWork.CommitChanges();
                return true;

            }
            catch{
                _couponInfoRepository.UnitOfWork.SafeRollback();
                return false;
            }


        }
    }
}
