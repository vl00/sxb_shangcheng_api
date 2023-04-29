using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace iSchool.Organization.Appliaction.Service

{
    public class GetKuaidiDetailQueryHandle : IRequestHandler<GetKuaidiDetailQuery, KuaidiDetailDto>
    {
        IConfiguration _config;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;

        public GetKuaidiDetailQueryHandle(IConfiguration config, IMediator mediator, IOrgUnitOfWork orgUnitOfWork)
        {
            this._config = config;
            this._mediator = mediator;
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<KuaidiDetailDto> Handle(GetKuaidiDetailQuery request, CancellationToken cancellationToken)
        {
            if (request.ExpressCode.IsNullOrEmpty())
            {
                throw new CustomResponseException("快递单号不能为空");
            }


            var res = new KuaidiDetailDto
            {
                RecvAddressDto = new RecvAddressDto
                {
                    RecvUsername = request.RecvUsername,
                    Address = request.Address,
                    Area = request.Area,
                    City = request.City,
                    Province = request.Province,
                    Postalcode = request.Postalcode,
                    RecvMobile = request.RecvMobile,
                }
            };


            var customer = request.ExpressType == "SF" && (request.RecvMobile?.Length ?? 0) >= 4 ? request.RecvMobile[^4..] : null;
            if (customer.IsNullOrEmpty() || !int.TryParse(customer, out _)) customer = null;
            var dto = await _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery { Nu = request.ExpressCode, Com = request.ExpressType, Customer = customer });


            if (dto.Errcode == 0)
            {
                res.Nu = dto.Nu;
                res.Items = dto.Items;
                res.IsCompleted = dto.IsCompleted;
                res.CompanyName = dto.CompanyName;
                res.CompanyCode = dto.CompanyCode;
            }
            else
            {
                res.Nu = request.ExpressCode;
                res.IsCompleted = false;

                var kdcom = (await _mediator.Send(KuaidiServiceArgs.GetCode(request.ExpressType))).GetResult<KdCompanyCodeDto>();
                res.CompanyCode = kdcom?.Code;
                res.CompanyName = kdcom?.Com;
            }


            if (res.Items?.Any() != true)
            {
                res.Items = new[]
                {
                    new KuaidiNuDataItemDto
                    {
                        Time = (request.SendExpressTime ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                        Desc = "正在等待快递员上门揽收",
                    }
                };
            }


            // 小助手二维码
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), _config[$"AppSettings:org_assistant"]);
                var bys = await File.ReadAllBytesAsync(path);
                res.HelperQrcodeUrl = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }

            return res;
        }
    }
}
