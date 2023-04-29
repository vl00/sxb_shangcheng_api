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
    public class GetOrderKuaidiDetailQueryHandler : IRequestHandler<GetOrderKuaidiDetailQuery, OrderKuaidiDetailDto>
    {
        IConfiguration _config;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;
        IRepository<Domain.Order> _orderRepo;

        public GetOrderKuaidiDetailQueryHandler(IConfiguration config, IRepository<Domain.Order> orderRepo,
            IOrgUnitOfWork orgUnitOfWork,
            IMediator mediator)
        {
            this._config = config;
            this._mediator = mediator;
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._orderRepo = orderRepo;
        }

        public async Task<OrderKuaidiDetailDto> Handle(GetOrderKuaidiDetailQuery query, CancellationToken cancellation)
        {
            var order = _orderRepo.Get(query.OrderId);
            if (order == null || order.IsValid != true || order.Type != (int)OrderType.BuyCourseByWx)
            {
                throw new CustomResponseException("无效的订单", Consts.Err.Kuaidi_OrderIsNotValid);
            }

            if (order.ExpressCode.IsNullOrEmpty())
            {
                return null;
            }

            var result = new OrderKuaidiDetailDto
            {
                OrderId = order.Id,
                OrderNo = order.Code,
                RecvAddressDto = new RecvAddressDto
                {
                    RecvUsername = order.RecvUsername,
                    RecvMobile = order.Mobile,
                    Address = order.Address,
                    Postalcode = order.RecvPostalcode,
                    Province = order.RecvProvince,
                    City = order.RecvCity,
                    Area = order.RecvArea,
                },
            };

            var customer = order.ExpressType == "SF" && (order.Mobile?.Length ?? 0) >= 4 ? order.Mobile[^4..] : null;
            if (customer.IsNullOrEmpty() || !int.TryParse(customer, out _)) customer = null;
            var dto = await _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery { Nu = order.ExpressCode, Com = order.ExpressType, Customer = customer });
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
                result.Nu = order.ExpressCode;
                result.IsCompleted = false;

                var kdcom = (await _mediator.Send(KuaidiServiceArgs.GetCode(order.ExpressType))).GetResult<KdCompanyCodeDto>();
                result.CompanyCode = kdcom?.Code;
                result.CompanyName = kdcom?.Com;
            }
            if (result.Items?.Any() != true)
            {
                result.Items = new[]
                {
                    new KuaidiNuDataItemDto
                    {
                        Time = order.SendExpressTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                        Desc = "正在等待快递员上门揽收",
                    }
                };
            }

            // 小助手二维码
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), _config[$"AppSettings:org_assistant"]);
                var bys = await File.ReadAllBytesAsync(path);
                result.HelperQrcodeUrl = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }

            return result;
        }

    }
}

