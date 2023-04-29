using Dapper.Contrib.Extensions;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CouponInfo = iSchool.Organization.Domain.AggregateModel.CouponAggregate.CouponInfo;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class UpdateCouponInfoCommandHandler : IRequestHandler<UpdateCouponInfoCommand, bool>
    {
        ICouponInfoRepository _couponInfoRepository;
        IMediator _mediator;
        public UpdateCouponInfoCommandHandler(ICouponInfoRepository couponInfoRepository
            , IMediator mediator)
        {
            _couponInfoRepository = couponInfoRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(UpdateCouponInfoCommand request, CancellationToken cancellationToken)
        {
            //涉及到更新Stock，加上库存锁。

            CouponInfo couponInfo = await _couponInfoRepository.GetAsync(request.Id);
            couponInfo.Name = request.Title;
            couponInfo.Desc = request.RuleDesc;
            couponInfo.VaildDateType = request.ExpireTimeType;
            couponInfo.VaildStartDate = request.STime;
            couponInfo.VaildEndDate = request.ETime;
            couponInfo.MaxTake = request.MaxTake;
            couponInfo.CouponType = request.CouponType;
            couponInfo.Fee = request.Fee;
            couponInfo.FeeOver = request.FeeOver;
            couponInfo.Discount = request.Discount;
            couponInfo.PriceOfTest = request.PriceOfTest;
            couponInfo.GetStartTime = request.STime;
            couponInfo.GetEndTime = request.ETime;
            couponInfo.Link = request.Link;
            couponInfo.UpdateTime = DateTime.Now;
            couponInfo.Updator = request.Updator;
            couponInfo.ICon = request.ICon;
            couponInfo.IsHide = request.IsHide;
            couponInfo.SetVaildTime(request.ExpireDays.GetValueOrDefault());
            couponInfo.SetMaxFee(request.CouponType, request.Fee, request.FeeOver, request.PriceOfTest);
            couponInfo.SetEnableRanges(Newtonsoft.Json.JsonConvert.SerializeObject(request.EnableRanges));
            couponInfo.InitialStock(request.Stock);
            var res =  await _couponInfoRepository.UpdateAsync(couponInfo,new[] { 
                nameof(couponInfo.Name)
                ,nameof(couponInfo.Desc)
                ,nameof(couponInfo.VaildDateType)
                ,nameof(couponInfo.VaildStartDate)
                ,nameof(couponInfo.VaildEndDate)
                ,nameof(couponInfo.MaxTake)
                ,nameof(couponInfo.CouponType)
                ,nameof(couponInfo.Fee)
                ,nameof(couponInfo.FeeOver)
                ,nameof(couponInfo.Discount)
                ,nameof(couponInfo.PriceOfTest)
                ,nameof(couponInfo.GetStartTime)
                ,nameof(couponInfo.GetEndTime)
                ,nameof(couponInfo.Link)
                ,nameof(couponInfo.UpdateTime)
                ,nameof(couponInfo.Updator)
                ,nameof(couponInfo.ICon)
                ,nameof(couponInfo.IsHide)
                ,nameof(couponInfo.VaildTime)
                ,nameof(couponInfo.MaxFee)
                ,nameof(couponInfo.EnableRange_JSN)
                ,nameof(couponInfo.Stock)
                ,nameof(couponInfo.KeyWord)
            });
            await _mediator.Send(new SetCouponEnableRangeCommand() { CouponInfo = couponInfo });
            return res;
        }
    }
}
