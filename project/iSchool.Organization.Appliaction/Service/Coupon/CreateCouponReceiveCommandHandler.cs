using iSchool.Infras.Locks;
using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using iSchool.Organization.Domain.Event.Coupon;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Infrastructure;
using iSchool.Organization.Domain.Security;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class CreateCouponReceiveCommandHandler : IRequestHandler<CreateCouponReceiveCommand, CouponReceive>
    {
        ICouponReceiveRepository _couponReceiveRepository;
        ICouponInfoRepository _couponInfoRepository;
        ILogger<CreateCouponReceiveCommandHandler> _logger;
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        ILock1Factory _lock1Factory;
        public CreateCouponReceiveCommandHandler(ICouponReceiveRepository couponReceiveRepository
            , ILogger<CreateCouponReceiveCommandHandler> logger
            , IMediator mediator
            , IOrgUnitOfWork orgUnitOfWork
            , ICouponInfoRepository couponInfoRepository
            , ILock1Factory lock1Factory)
        {
            _couponReceiveRepository = couponReceiveRepository;
            _logger = logger;
            _mediator = mediator;
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _couponInfoRepository = couponInfoRepository;
            _lock1Factory = lock1Factory;
        }
        public async Task<CouponReceive> Handle(CreateCouponReceiveCommand request, CancellationToken cancellationToken)
        {
            //防止并发库存超减
           
            await using var _lck = await _lock1Factory.LockAsync(CacheKeys.Coupon_ReceiveLck.FormatWith(request.CouponId), 3 * 60 * 1000);
            if (!_lck.IsAvailable) throw new Exception("领取失败！系统繁忙");
            CouponInfo couponInfo;
            if (request.CouponId != null)
            {
                couponInfo = await _couponInfoRepository.GetAsync(request.CouponId.Value);
                if (couponInfo == null) throw new KeyNotFoundException("领取失败！找不到该券。");
            }
            else if (int.TryParse(request.Number, out int couponNumber))
            {
                couponInfo = await _couponInfoRepository.FindFromNumberAsync(couponNumber);
            }
            else {
                throw new ArgumentException("领取失败！优惠券标识无效。");
            }
            var (totalReceive,waitUseReceive) = await this.GetReceiveCount(request.UserId, couponInfo.Id);
            if (totalReceive >= couponInfo.MaxTake)
                throw new Exception("领取失败！您已经领取过此优惠券了");
            if (request.OriginType== CouponReceiveOriginType.SelfReceive)
            { 
                if (couponInfo.IsHide)
                    throw new Exception("领取失败！当前优惠券不可领取");
                if(waitUseReceive>0)
                    throw new Exception("领取失败！您已经拥有一张未使用的券");
            }

            if (couponInfo.VaildDateType == CouponInfoVaildDateType.SpecialDate)
            {
                if (DateTime.Now > couponInfo.VaildEndDate)
                {
                    throw new Exception("领取失败！该优惠券已过期。");
                }
            }
            if (couponInfo.Status == 0)
            {
                throw new Exception("领取失败！该优惠券已下线。");
            }
            if (couponInfo.Stock <= 0)
            {
                throw new Exception("领取失败！优惠券库存不足。");
            }

            DateTime ValidStartTime = DateTime.Now, ValidEndTime = DateTime.Now;
            if (couponInfo.VaildDateType == CouponInfoVaildDateType.Forever)
            {
                ValidStartTime = DateTime.Now;
                ValidEndTime = DateTime.MaxValue;
            }
            else if (couponInfo.VaildDateType == CouponInfoVaildDateType.SpecialDate)
            {
                ValidStartTime = couponInfo.VaildStartDate.GetValueOrDefault();
                ValidEndTime = couponInfo.VaildEndDate.GetValueOrDefault();
            }
            else if (couponInfo.VaildDateType == CouponInfoVaildDateType.SpecialDays)
            {
                ValidStartTime = DateTime.Now;
                ValidEndTime = DateTime.Now.AddHours(couponInfo.VaildTime);
            }
            CouponReceive couponReceive = new CouponReceive(Guid.NewGuid(), couponInfo.Id, request.UserId, DateTime.Now,ValidStartTime,ValidEndTime, originType: request.OriginType, remark: request.Remark);
            try
            {
                _couponReceiveRepository.UnitOfWork.BeginTransaction();
                couponReceive = _couponReceiveRepository.Add(couponReceive);
                couponInfo.ReduceStock(1);
                if (!await _couponInfoRepository.UpdateAsync(couponInfo, nameof(couponInfo.Stock))) throw new Exception("优惠券减库存失败。");
                if (request.OriginType == CouponReceiveOriginType.FromSystem)
                {
                    couponReceive.AddDomainEvent(new SystemGrantCouponDomainEvent(couponInfo,couponReceive));
                }
                else if(request.OriginType == CouponReceiveOriginType.SelfReceive) {
                    couponReceive.AddDomainEvent(new UserReceiveCouponDomainEvent(couponReceive));
                }

                await _mediator.DispatchDomainEventsAsync(new List<Entity>{ couponReceive });
                _couponReceiveRepository.UnitOfWork.CommitChanges();
            }
            catch(Exception ex){
                _logger.LogError(ex, $"领券发生异常,uid={ request.UserId},couponId={request.CouponId}");
                _couponReceiveRepository.UnitOfWork.Rollback();
                throw ex;

            }
            return couponReceive;
        }


        public async Task<(int total,int waitUse)> GetReceiveCount(Guid userId,Guid couponId)
        {
            string sql = @"SELECT COUNT(1) FROM CouponReceive WHERE UserId = @userId AND CouponId = @couponId  AND IsDel = 0;
SELECT COUNT(1) FROM CouponReceive WHERE UserId = @userId AND CouponId = @couponId  AND IsDel = 0 AND [Status] =1 AND GETDATE() BETWEEN VaildStartTime AND VaildEndTime ";
            using (var grid =await _orgUnitOfWork.QueryMultipleAsync(sql, new { userId, couponId }, _orgUnitOfWork.DbTransaction))
            {
                int total = await grid.ReadFirstAsync<int>();
                int waitUse = await grid.ReadFirstAsync<int>();
                return (total, waitUse);
            }

        }



    }
}
