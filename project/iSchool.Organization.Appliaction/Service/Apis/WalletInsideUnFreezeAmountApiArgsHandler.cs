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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class WalletInsideUnFreezeAmountApiArgsHandler : IRequestHandler<WalletInsideUnFreezeAmountApiArgs, WalletInsideUnFreezeAmountApiResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        IHttpClientFactory _httpClientFactory;
        NLog.ILogger log;

        public WalletInsideUnFreezeAmountApiArgsHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IHttpClientFactory httpClientFactory, NLog.ILogger log,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._httpClientFactory = httpClientFactory;
            this.log = log;
        }

        public async Task<WalletInsideUnFreezeAmountApiResult> Handle(WalletInsideUnFreezeAmountApiArgs args, CancellationToken cancellation)
        {
            var url = _config["AppSettings:wxpay:baseUrl"] + "/api/Wallet/InsideUnFreezeAmount";
            var paykey = _config["AppSettings:wxpay:paykey"];
            var system = _config["AppSettings:wxpay:system"];
            await default(ValueTask);

            var body = (args).ToJsonString(camelCase: true, ignoreNull: true);

            using var http = _httpClientFactory.CreateClient(string.Empty);
            var rr = await new HttpApiInvocation(log).SetAllowLogOnDebug(true)
                .SetApiDesc("解冻结金额内部接口调用直接入账api - " + args.Remark)
                .SetMethod(HttpMethod.Post).SetUrl(url)
                .SetBodyByJsonStr(body)
                .OnBeforeRequest(req =>
                {
                    req.SetFinanceSignHeader(paykey, body, system);
                })
                .InvokeByAsync<JObject>(http);

            return rr.Succeed ? new WalletInsideUnFreezeAmountApiResult { Success = true }
                : new WalletInsideUnFreezeAmountApiResult { Success = false, ErrorDesc = rr.Msg };
        }

    }
}
