using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Apis;
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
    public class LongUrlToShortHandler : IRequestHandler<LongUrlToShortUrlRequest, LongUrlToShortUrlResult>
    {
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;

        NLog.ILogger _log;

        public LongUrlToShortHandler(IHttpClientFactory httpClientFactory, IConfiguration config,

            IServiceProvider services)
        {
            this._httpClientFactory = httpClientFactory;
            this._config = config;

            this._log = services.GetService<NLog.ILogger>();
        }

        public async Task<LongUrlToShortUrlResult> Handle(LongUrlToShortUrlRequest q, CancellationToken cancellation)
        {

            var msg = new NLog.LogEventInfo();

            msg.Properties["Level"] = "错误";
            var url = _config["AppSettings:OperationBaseUrl"] + $"/Api/ToolsApi/ToShortUrl";
            using var http = _httpClientFactory.CreateClient(string.Empty);

            //var r = await new HttpApiInvocation(_log, msg)

            //    .SetMethod(HttpMethod.Post)
            //    .SetBodyByJson(q)
            //    .SetUrl(url)
            //    .SetApiDesc("长连接转短连接")
            //    .SetHeader("X-Requested-With", "XMLHttpRequest")
            //    .InvokeByAsync<LongUrlToShortUrlResult>(http);
            //return r.Succeed ? r.Data : new LongUrlToShortUrlResult { data = q.OriginUrl };

            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.SetHttpHeader("X-Requested-With", "XMLHttpRequest");
            req.SetContent(new StringContent((q).ToJsonString(true), Encoding.UTF8, "application/json"));
            HttpResponseMessage res = null;
            try
            {
                res = await http.SendAsync(req);
                res.EnsureSuccessStatusCode();
                var str = await res.Content.ReadAsStringAsync();
                return str.ToObject<LongUrlToShortUrlResult>();
            }
            catch (Exception ex)
            {
                return new LongUrlToShortUrlResult { data = q.OriginUrl };
            }


        }
    }
}

