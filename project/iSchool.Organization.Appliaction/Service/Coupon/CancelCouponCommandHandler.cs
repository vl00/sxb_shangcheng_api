using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class CancelCouponCommandHandler : IRequestHandler<CancelCouponCommand>
    {
        OrgUnitOfWork _unitOfWork;
        ICouponReceiveRepository _couponReceiveRepository;
        IUserInfo _userInfo;
        ILogger _logger;
        public CancelCouponCommandHandler(
            ICouponReceiveRepository couponReceiveRepository
            , IUserInfo userInfo
            , ILogger<CancelCouponCommandHandler> logger
            , IOrgUnitOfWork unitOfWork)
        {
            _couponReceiveRepository = couponReceiveRepository;
            _userInfo = userInfo;
            _logger = logger;
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
        }
        public async Task<Unit> Handle(CancelCouponCommand request, CancellationToken cancellationToken)
        {
            _couponReceiveRepository.UnitOfWork.BeginTransaction();
            try
            {
                var couponReceive = await _couponReceiveRepository.FindFromOrderAsync(request.OrderId);
                if (couponReceive == null) throw new KeyNotFoundException();
                couponReceive.SetOrderId(null);
                couponReceive.SetStatus(CouponReceiveState.WaitUse);
                if (!(await _couponReceiveRepository.UpdateAsync(couponReceive, nameof(couponReceive.Status), nameof(couponReceive.OrderId)))) throw new Exception("DB操作失败。");
                string sql = @"DELETE
OrderDiscount
WHERE 
OrderId IN (SELECT  OrderDetial.Id FROM OrderDetial JOIN [Order] ON [Order].id = OrderDetial.orderid AND AdvanceOrderId = @orderId)";
                await _unitOfWork.ExecuteAsync(sql, new { orderId = request.OrderId }, _unitOfWork.DbTransaction);
                _couponReceiveRepository.UnitOfWork.CommitChanges();

            }
            catch (Exception ex)
            {
                _couponReceiveRepository.UnitOfWork.Rollback();
                _logger.LogError(ex, $"撤销优惠券处理失败。orderId={request.OrderId}");
                throw ex;
            }
            return Unit.Value;
        }
    }

}
