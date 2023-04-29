using CSRedis;
using Dapper;
using iSchool.Infras;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public partial class FinanceCheckOrderPayStatusQueryHandler : IRequestHandler<FinanceCheckOrderPayStatusQuery, FinanceCheckOrderPayStatusQryResult>
    {
        IMediator _mediator;
        IServiceProvider services;
        IConfiguration config;
        NLog.ILogger log;

        public FinanceCheckOrderPayStatusQueryHandler(IMediator mediator,
            IConfiguration config, NLog.ILogger log,
            IServiceProvider services)
        {
            this._mediator = mediator;
            this.services = services;
            this.config = config;
            this.log = log;
        }

        public async Task<FinanceCheckOrderPayStatusQryResult> Handle(FinanceCheckOrderPayStatusQuery query, CancellationToken cancellation)
        {
            var httpClientFactory = services.GetService<IHttpClientFactory>();
            var url = config["AppSettings:wxpay:baseUrl"] + "/api/PayOrder/CheckPayStatus";
            var paykey = config["AppSettings:wxpay:paykey"];
            var system = config["AppSettings:wxpay:system"];
            await default(ValueTask);

            using var http = httpClientFactory.CreateClient(string.Empty);
            var body = query.ToJsonString(true);
            
            var rr = await new HttpApiInvocation(log).SetAllowLogOnDebug(true)
                .SetApiDesc("查询wx支付状态")
                .SetMethod(HttpMethod.Post).SetUrl(url)
                .SetBodyByJsonStr(body)
                .OnBeforeRequest(req => 
                {
                    req.SetHttpHeader("X-Requested-With", "XMLHttpRequest");
                    req.SetFinanceSignHeader(paykey, body, system);
                })
                .InvokeByAsync<FinanceCheckOrderPayStatusQryResult>(http);

            if (!rr.Succeed)
            {
                throw new CustomResponseException(rr.Msg, (int)rr.status);
            }
            return rr.Data;
        }
    }
}
