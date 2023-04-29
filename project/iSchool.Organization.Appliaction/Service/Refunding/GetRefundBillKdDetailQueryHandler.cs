using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetRefundBillKdDetailQueryHandler : IRequestHandler<GetRefundBillKdDetailQuery, RefundBillKdDetailDto>
    {
        IConfiguration _config;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;

        public GetRefundBillKdDetailQueryHandler(IConfiguration config, 
            IOrgUnitOfWork orgUnitOfWork,
            IMediator mediator)
        {
            this._config = config;
            this._mediator = mediator;
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<RefundBillKdDetailDto> Handle(GetRefundBillKdDetailQuery query, CancellationToken cancellation)
        {
            var orderRefund = query.OrderRefund;
            if (orderRefund == null) 
            {
                var id = Guid.TryParse(query.Id, out var _id) ? _id : default;
                var code = id == default ? query.Id : default;

                orderRefund = await _orgUnitOfWork.QueryFirstOrDefaultAsync<OrderRefunds>($@"
                    select * from OrderRefunds where IsValid=1 {"and id=@id".If(id != default)} {"and code=@code".If(code != default)}
                ", new { id, code });

                if (orderRefund == null)
                {
                    throw new CustomResponseException("无效的退款单", Consts.Err.Kuaidi_OrderIsNotValid);
                }
            }

            if (orderRefund.SendBackExpressCode.IsNullOrEmpty())
            {
                return null;
            }

            var result = new RefundBillKdDetailDto
            {
                Id = orderRefund.Id,
                Code = orderRefund.Code,
                RecvAddressDto = new RecvAddressDto
                {
                    RecvMobile = orderRefund.SendBackMobile,
                    Address = orderRefund.SendBackAddress,
                    RecvUsername = orderRefund.SendBackUserName,
                    //Province = order.RecvProvince,
                    //City = order.RecvCity,
                    //Area = order.RecvArea,
                },
            };

            var customer = orderRefund.SendBackExpressType == "SF" && (orderRefund.SendBackMobile?.Length ?? 0) >= 4 ? orderRefund.SendBackMobile[^4..] : null;
            if (customer.IsNullOrEmpty() || !int.TryParse(customer, out _)) customer = null;
            var dto = await _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery { Nu = orderRefund.SendBackExpressCode, Com = orderRefund.SendBackExpressType, Customer = customer });
            if (dto.Errcode == 0)
            {
                result.Nu = dto.Nu;
                result.Items = dto.Items;
                result.IsCompleted = dto.IsCompleted;
                result.CompanyName = dto.CompanyName;
                result.CompanyCode = dto.CompanyCode;
            }
            else
            {
                result.Nu = orderRefund.SendBackExpressCode;
                result.IsCompleted = false;

                var kdcom = (await _mediator.Send(KuaidiServiceArgs.GetCode(orderRefund.SendBackExpressType))).GetResult<KdCompanyCodeDto>();
                result.CompanyCode = kdcom?.Code;
                result.CompanyName = kdcom?.Com;
            }
            if (result.Items?.Any() != true)
            {
                result.Items = new[]
                {
                    new KuaidiNuDataItemDto
                    {
                        Time = (orderRefund.SendBackTime ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                        Desc = "正在等待快递员上门揽收",
                    }
                };
            }

            // 小助手二维码
            {
                //var path = Path.Combine(Directory.GetCurrentDirectory(), _config[$"AppSettings:org_assistant"]);
                //var bys = await File.ReadAllBytesAsync(path);
                //result.HelperQrcodeUrl = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }

            return result;
        }

    }
}

