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
    public class CheckIsFxHeadQueryHandler : IRequestHandler<CheckIsFxHeadQuery, CheckIsFxHeadQryResult>
    {
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;
        IUserInfo _me;
        NLog.ILogger _log;

        public CheckIsFxHeadQueryHandler(IHttpClientFactory httpClientFactory, IConfiguration config, 
            IUserInfo me,
            IServiceProvider services)
        {
            this._httpClientFactory = httpClientFactory;
            this._config = config;
            this._me = me;
            this._log = services.GetService<NLog.ILogger>();
        }

        public async Task<CheckIsFxHeadQryResult> Handle(CheckIsFxHeadQuery q, CancellationToken cancellation)
        {
            if (q.UserId == default)
            {
                return new CheckIsFxHeadQryResult { IsHead = false };
            }

            var msg = new NLog.LogEventInfo();
            msg.Properties["UserId"] = q.UserId;
            msg.Properties["Level"] = "错误";

            using var http = _httpClientFactory.CreateClient(string.Empty);

            var r = await new HttpApiInvocation(_log, msg)
                .SetMethod(HttpMethod.Get)
                .SetUrl(_config["AppSettings:DrpfxBaseUrl"] + $"/api/FxCenter/CheckIsHeadService?userid={q.UserId}")
                .SetApiDesc("检查是否是顾问")
                .SetHeader("X-Requested-With", "XMLHttpRequest")
                .InvokeByAsync<CheckIsFxHeadQryResult>(http);

            return r.Succeed ? r.Data : new CheckIsFxHeadQryResult { IsHead = false };
        }
    }
}

