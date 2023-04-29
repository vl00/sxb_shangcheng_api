using CSRedis;
using iSchool.Domain.Modles;
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
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class RefundCmdHandler : IRequestHandler<RefundCmd, RefundCmdResult>
    {
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;        
        NLog.ILogger _log;

        public RefundCmdHandler(IHttpClientFactory httpClientFactory, IConfiguration config,             
            IServiceProvider services)
        {
            this._httpClientFactory = httpClientFactory;
            this._config = config;            
            this._log = services.GetService<NLog.ILogger>();
        }

        public async Task<RefundCmdResult> Handle(RefundCmd cmd, CancellationToken cancellation)
        {
            var url = _config["AppSettings:wxpay:baseUrl"] + "/api/PayOrder/PartRefund";// 调用秀彬退款api
            var paykey = _config["AppSettings:wxpay:paykey"];
            var system = _config["AppSettings:wxpay:system"];
            await default(ValueTask);

            var msg = new NLog.LogEventInfo();
            msg.Properties["Level"] = "错误";            

            using var http = _httpClientFactory.CreateClient(string.Empty);

            var body = (cmd).ToJsonString(camelCase: true, ignoreNull: true);

            var r = await new HttpApiInvocation(_log, msg).SetAllowLogOnDebug(true)
                .SetApiDesc("调用退款api - " + cmd.Remark)
                .SetMethod(HttpMethod.Post).SetUrl(url)
                .SetBodyByJsonStr(body)
                .OnBeforeRequest(req =>
                {
                    req.SetFinanceSignHeader(paykey, body, system);
                })
                .InvokeByAsync<RefundCmdResult>(http);

            var data = r.Data ?? new RefundCmdResult();
            //if (data != null) data.Succeed = r.Succeed;
            return data;
        }

    }
}

