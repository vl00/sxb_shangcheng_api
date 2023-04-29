using Dapper.Contrib.Extensions;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CouponInfo = iSchool.Organization.Domain.AggregateModel.CouponAggregate.CouponInfo;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class AddCouponInfoCommandHandler : IRequestHandler<AddCouponInfoCommand, bool>
    {
        ICouponInfoRepository _couponInfoRepository;
        IMediator _mediator;
        public AddCouponInfoCommandHandler(ICouponInfoRepository couponInfoRepository
            , IMediator mediator)
        {
            _couponInfoRepository = couponInfoRepository;
            _mediator = mediator;
        }

        public  async Task<bool> Handle(AddCouponInfoCommand request, CancellationToken cancellationToken)
        {
            var model = new CouponInfo()
            {
                Id = Guid.NewGuid(),
                Name = request.Title,
                Desc = request.RuleDesc,
                VaildDateType = request.ExpireTimeType,
                VaildStartDate = request.STime,
                VaildEndDate = request.ETime,
                MaxTake = request.MaxTake,
                Total = request.Stock,
                CouponType = request.CouponType,
                Fee = request.Fee,
                FeeOver = request.FeeOver,
                Discount = request.Discount,
                PriceOfTest = request.PriceOfTest,
                GetStartTime = request.STime,
                GetEndTime = request.ETime,
                Link = request.Link,
                CreateTime = DateTime.Now,
                Creator = request.Creator,
                Updator = request.Creator,
                UpdateTime = DateTime.Now,
                ICon = request.ICon,
                IsHide = request.IsHide,
            };
            model.InitialStock(request.Stock);
            model.SetVaildTime(request.ExpireDays.GetValueOrDefault());
            model.SetMaxFee(request.CouponType, request.Fee, request.FeeOver, request.PriceOfTest);
            model.SetEnableRanges(Newtonsoft.Json.JsonConvert.SerializeObject(request.EnableRanges));
            _couponInfoRepository.Add(model);
            await _mediator.Send(new SetCouponEnableRangeCommand() {  CouponInfo = model});
            return true;
        }

  
    }
}
