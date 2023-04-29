using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class GetRefundDetailQueryHandler : IRequestHandler<GetRefundDetailQuery, RefundDetailDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public GetRefundDetailQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<RefundDetailDto> Handle(GetRefundDetailQuery query, CancellationToken cancellation)
        {
            var result = new RefundDetailDto();
            var id = Guid.TryParse(query.Id, out var _id) ? _id : default;
            var code = id == default ? query.Id : default;
            await default(ValueTask);

            var orderRefund = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<OrderRefunds>($@"
                select * from OrderRefunds where IsValid=1 {"and id=@id".If(id != default)} {"and code=@code".If(code != default)}
            ", new { id, code });
            if (orderRefund == null)
            {
                throw new CustomResponseException("无效的退款单", Consts.Err.Kuaidi_OrderIsNotValid);
            }
            id = result.Id = orderRefund.Id;
            code = result.Code = orderRefund.Code;
            result.RefundType = orderRefund.Type;
            result.CreateTime = orderRefund.CreateTime ?? default;
            result.RefundMoney = orderRefund.Price;

            // item
            var orderDetail = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<OrderDetial>(@"
                select * from [OrderDetial] where id=@OrderDetailId
            ", new { orderRefund.OrderDetailId });
            if (orderDetail == null) throw new CustomResponseException("订单不存在", 404);
            result.Item = OrderHelper.ConvertTo_CourseOrderProdItemDto(orderDetail);
            result.Item.BuyCount = orderRefund.Count;

            switch ((RefundTypeEnum)result.RefundType)
            {
                case RefundTypeEnum.FastRefund:
                    result.Rty3 = new RefundDetailDto_Rty3();
                    result.UpTime = orderRefund.RefundTime ?? orderRefund.ModifyDateTime ?? DateTime.Now;
                    break;

                case RefundTypeEnum.Refund:
                    result.Rty1 = new RefundDetailDto_Rty1();
                    {
                        var rtyDto = result.Rty1;
                        rtyDto.Status = orderRefund.Status ?? 0;
                        rtyDto.Cause = orderRefund.Cause ?? (int)RefundCauseEnum.C08;
                        rtyDto.CauseDesc = ((RefundCauseEnum)orderRefund.Cause).GetDesc();
                        rtyDto.NotOkReason = orderRefund.Status == (int)RefundStatusEnum.RefundSuccess ? null
                            : orderRefund.Status == (int)RefundStatusEnum.RefundAuditFailed ? $"未通过原因: {orderRefund.StepOneAuditRecord}"
                            : null;
                        result.UpTime = (orderRefund.Status == (int)RefundStatusEnum.RefundSuccess ? orderRefund.RefundTime
                            : orderRefund.Status == (int)RefundStatusEnum.RefundAuditFailed ? orderRefund.StepOneTime
                            : orderRefund.ModifyDateTime) ?? orderRefund.CreateTime ?? DateTime.Now;
                    }
                    break;

                case RefundTypeEnum.Return:
                    result.Rty2 = new RefundDetailDto_Rty2();
                    {
                        var rtyDto = result.Rty2;
                        rtyDto.Status = orderRefund.Status ?? 0;
                        rtyDto.Cause = orderRefund.Cause ?? (int)RefundCauseEnum.C15;
                        rtyDto.CauseDesc = ((RefundCauseEnum)orderRefund.Cause).GetDesc();
                        rtyDto.NotOkReason = orderRefund.Status == (int)RefundStatusEnum.ReturnSuccess ? null
                            : orderRefund.Status == (int)RefundStatusEnum.ReturnAuditFailed ? $"未通过原因: {orderRefund.StepOneAuditRecord}"
                            : orderRefund.Status == (int)RefundStatusEnum.InspectionFailed ? $"未通过原因: {orderRefund.StepTwoAuditRecord}"
                            : orderRefund.Status == (int)RefundStatusEnum.CancelByExpired ? "您未在规定时间内填写退货物流信息，系统已自动为您取消售后申请。"
                            : null;
                        result.UpTime = (orderRefund.Status == (int)RefundStatusEnum.ReturnSuccess ? orderRefund.RefundTime
                            : orderRefund.Status == (int)RefundStatusEnum.ReturnAuditFailed ? orderRefund.StepOneTime
                            : orderRefund.Status == (int)RefundStatusEnum.SendBack ? orderRefund.StepOneTime
                            : orderRefund.Status == (int)RefundStatusEnum.Receiving ? orderRefund.SendBackTime
                            : orderRefund.Status == (int)RefundStatusEnum.InspectionFailed ? orderRefund.StepTwoTime
                            : orderRefund.ModifyDateTime) ?? orderRefund.CreateTime ?? DateTime.Now;

                        if (orderRefund.Status == (int)RefundStatusEnum.SendBack)
                        {
                            rtyDto.AddressDto = new RecvAddressDto
                            {
                                Address = orderRefund.SendBackAddress,
                                RecvMobile = orderRefund.SendBackMobile,
                                RecvUsername = orderRefund.SendBackUserName,
                            };
                        }
                        if (rtyDto.NotOkReason == null && orderRefund.Status > (int)RefundStatusEnum.SendBack && !orderRefund.SendBackExpressCode.IsNullOrEmpty())
                        {
                            rtyDto.ExpressNu = orderRefund.SendBackExpressCode;                           
                            var kd = await _mediator.Send(new GetRefundBillKdDetailQuery { OrderRefund = orderRefund });
                            rtyDto.ExpressCompanyName = kd.CompanyName;
                            rtyDto.LastExpressTime = DateTime.TryParse(kd.Items?.FirstOrDefault().Time, out var _time) ? _time : (DateTime?)null;
                            rtyDto.LastExpressDesc = kd.Items?.FirstOrDefault().Desc;
                        }
                    }
                    break;

                case RefundTypeEnum.BgRefund:
                    result.Rty4 = new RefundDetailDto_Rty4();                    
                    result.UpTime = orderRefund.RefundTime ?? orderRefund.ModifyDateTime ?? DateTime.Now;
                    break;

                default:
                    result.UpTime = DateTime.Now;
                    break;
            }

            // 小助手二维码
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), _config[$"AppSettings:org_assistant"]);
                var bys = await System.IO.File.ReadAllBytesAsync(path);
                result.Qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }
            return result;
        }

    }
}
