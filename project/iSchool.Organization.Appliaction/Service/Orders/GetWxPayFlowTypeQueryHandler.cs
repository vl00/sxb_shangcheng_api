using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
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
    public class GetWxPayFlowTypeQueryHandler : IRequestHandler<GetWxPayFlowTypeQuery, GetWxPayFlowTypeQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public GetWxPayFlowTypeQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<GetWxPayFlowTypeQryResult> Handle(GetWxPayFlowTypeQuery query, CancellationToken cancellation)
        {
            await default(ValueTask);

            var result = await _redis.GetAsync<GetWxPayFlowTypeQryResult>(CacheKeys.WxPayFlowType);
            if (result == null || result.Type.ToInt() <= 0)
            {
                result = new GetWxPayFlowTypeQryResult();

                result.Type = (WxPayFlowTypeEnum)(int.TryParse(_config["AppSettings:wxpay:payflowtype:type"], out var _ty) ? _ty : -1);
                result.AppId = _config["AppSettings:wxpay:payflowtype:appid"];

                await _redis.SetAsync(CacheKeys.WxPayFlowType, result.ToJsonString(camelCase: true));
            }

            return result;
        }

    }
}
