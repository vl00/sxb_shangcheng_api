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
    public partial class CompanyPayToUserWalletCmdHandler : IRequestHandler<CompanyPayToUserWalletCmd, JToken>
    {
        IMediator _mediator;
        IServiceProvider services;
        IConfiguration config;

        public CompanyPayToUserWalletCmdHandler(IMediator mediator,
            IConfiguration config,
            IServiceProvider services)
        {
            this._mediator = mediator;
            this.services = services;
            this.config = config;
        }

        public async Task<JToken> Handle(CompanyPayToUserWalletCmd cmd, CancellationToken cancellation)
        {
            var httpClientFactory = services.GetService<IHttpClientFactory>();
            var log = services.GetService<NLog.ILogger>();
            var url = config["AppSettings:wxpay:baseUrl"] + "/api/Wallet/CompanyOperate";// 调用秀彬api[公司打款入账个人]
            var paykey = config["AppSettings:wxpay:paykey"];
            var system = config["AppSettings:wxpay:system"];
            await default(ValueTask);

            using var http = httpClientFactory.CreateClient(string.Empty);

            var body = (new
            {
                cmd.ToUserId,
                cmd.OrderId,
                amount = cmd.Money,
                cmd.Remark,
                orderType = 3,
                system = 2,
                cmd._others
            }).ToJsonString(camelCase: true, ignoreNull: true);

            var r = await new HttpApiInvocation(log, log?.GetNLogMsg(null).SetUserId(cmd.ToUserId)).SetAllowLogOnDebug(true)
                .SetApiDesc("公司打款入账个人-" + cmd.Remark)
                .SetMethod(HttpMethod.Post).SetUrl(url)
                .SetBodyByJsonStr(body)
                .OnBeforeRequest(req =>
                {
                    req.SetFinanceSignHeader(paykey, body, system);
                })
                .InvokeByAsync(http);

            return r.Succeed ? r.Data : null;
        }
    }
}
