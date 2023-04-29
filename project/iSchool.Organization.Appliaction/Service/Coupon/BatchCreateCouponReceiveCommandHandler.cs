using iSchool.Organization.Appliaction.RequestModels.Coupon;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using System.Linq;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using Microsoft.Extensions.Logging;
using iSchool.Infras.Locks;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Event.Coupon;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class BatchCreateCouponReceiveCommandHandler : IRequestHandler<BatchCreateCouponReceiveCommand, ResponseResult>
    {
        ICouponReceiveRepository _couponReceiveRepository;
        ICouponInfoRepository _couponInfoRepository;
        ILogger<CreateCouponReceiveCommandHandler> _logger;
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        ILock1Factory _lock1Factory;
        public BatchCreateCouponReceiveCommandHandler(ICouponReceiveRepository couponReceiveRepository
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

        public async Task<ResponseResult> Handle(BatchCreateCouponReceiveCommand wrap, CancellationToken cancellationToken)
        {
            if (wrap == null || wrap.UserIds == null || !wrap.UserIds.Any())
            {
                return ResponseResult.Failed("请选择发放给谁");
            }
            if (wrap.CouponId == default)
            {
                return ResponseResult.Failed("请选择优惠券");
            }
            try
            {
                await Grant(wrap);
                return ResponseResult.Success();
            }
            catch (Exception ex)
            {
                return ResponseResult.Failed(ex.Message);
            }
        }

        public async Task Grant(BatchCreateCouponReceiveCommand wrap)
        {
            //防止并发库存超减
            await using var _lck = await _lock1Factory.LockAsync(CacheKeys.Coupon_ReceiveLck.FormatWith(wrap.CouponId), 3 * 60 * 1000);
            if (!_lck.IsAvailable) throw new Exception("系统繁忙");

            Guid couponId = wrap.CouponId;
            Guid senderID = wrap.SenderID;

            var couponInfo = await _couponInfoRepository.GetAsync(wrap.CouponId);
            if (couponInfo == null) throw new KeyNotFoundException("找不到该券。");
            {
                //throw new Exception("当前用户领取数量大于优惠券最大领取限额。。");
            }
            if (couponInfo.VaildDateType == CouponInfoVaildDateType.SpecialDate)
            {
                if (DateTime.Now > couponInfo.VaildEndDate)
                {
                    throw new Exception("该优惠券已过期。");
                }
            }
            if (couponInfo.Status == 0)
            {
                throw new Exception("该优惠券已下线。");
            }
            if (couponInfo.Stock < wrap.UserIds.Count)
            {
                throw new Exception("优惠券库存不足。");
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


            foreach (var userId in wrap.UserIds)
            {
                try
                {
                    _couponReceiveRepository.UnitOfWork.BeginTransaction();
                    var request = new CreateCouponReceiveCommand()
                    {
                        UserId = userId,
                        CouponId = couponId,
                        Remark = "后台发放",
                        OriginType =  CouponReceiveOriginType.FromSystem
                    };
                    CouponReceive couponReceive = new CouponReceive(Guid.NewGuid(), request.CouponId.Value, request.UserId, DateTime.Now, ValidStartTime, ValidEndTime, originType: request.OriginType, remark: request.Remark);


                    couponReceive = _couponReceiveRepository.Add(couponReceive);
                    couponInfo.ReduceStock(1);
                    if (!await _couponInfoRepository.UpdateAsync(couponInfo, nameof(couponInfo.Stock))) throw new Exception("优惠券减库存失败。");
                    couponReceive.AddDomainEvent(new SystemGrantCouponDomainEvent(couponInfo, couponReceive));
                    await _mediator.DispatchDomainEventsAsync(new List<Entity> { couponReceive });
                    _couponReceiveRepository.UnitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"领券发生异常,uid={ wrap.UserIds.ToJsonString()},couponId={wrap.CouponId}");
                    _couponReceiveRepository.UnitOfWork.Rollback();
                    throw ex;
                }
            }


        }
    }
}
