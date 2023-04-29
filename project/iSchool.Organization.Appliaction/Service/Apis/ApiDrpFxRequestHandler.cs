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
    public class ApiDrpFxRequestHandler : IRequestHandler<ApiDrpFxRequest, ApiDrpFxResponse>
    {
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;        
        NLog.ILogger _log;

        public ApiDrpFxRequestHandler(IHttpClientFactory httpClientFactory, IConfiguration config,             
            IServiceProvider services)
        {
            this._httpClientFactory = httpClientFactory;
            this._config = config;            
            this._log = services.GetService<NLog.ILogger>();
        }

        public async Task<ApiDrpFxResponse> Handle(ApiDrpFxRequest req, CancellationToken cancellation)
        {
            var res = new ApiDrpFxResponse();
            switch (req.Ctn)
            {
                case ApiDrpFxRequest.BecomSecondCmd becomSecondCmd:
                    res.Result = await Handle_BecomSecondCmd(becomSecondCmd, cancellation);
                    break;
                case ApiDrpFxRequest.AddFxOrderCmd addFxOrderCmd:
                    res.Result = await Handle_AddFxOrderCmd(addFxOrderCmd, cancellation);
                    break;
                case ApiDrpFxRequest.BecomHeadUserInHdCmd becomHeadUserInHdCmd:
                    await Handle_BecomHeadUserInHdCmd(becomHeadUserInHdCmd, cancellation);
                    break;
                case ApiDrpFxRequest.GetConsultantRateQry _GetConsultantRateQry:
                    res.Result = await Handle_GetConsultantRateQry(_GetConsultantRateQry, cancellation);
                    break;
                case ApiDrpFxRequest.OrgOrderSettleCmd _OrgOrderSettleCmd:
                    res.Result = await Handle_OrgOrderSettleCmd(_OrgOrderSettleCmd, cancellation);
                    break;
            }
            return res;
        }

        private async Task<bool> Handle_BecomSecondCmd(ApiDrpFxRequest.BecomSecondCmd cmd, CancellationToken cancellation)
        {
            if (cmd.HeadUserId == default && cmd.UserId == default)
            {
                return false;
            }

            var msg = new NLog.LogEventInfo();
            msg.Properties["UserId"] = cmd.UserId;
            msg.Properties["Level"] = "错误";            

            using var http = _httpClientFactory.CreateClient(string.Empty);

            var r = await new HttpApiInvocation(HttpMethod.Post, _config["AppSettings:DrpfxBaseUrl"] + "/api/FxCenter/BeComSecondService", _log, msg)
                .SetAllowLogOnDebug(true)
                .SetApiDesc("call成为下线api" + (cmd.HeadUserId == default ? "(预锁粉)" : ""))
                .SetBodyByJson(cmd)                
                .InvokeByAsync(http);

            return r.Succeed;
        }

        private async Task<ApiDrpFxResponse.AddFxOrderCmdResult> Handle_AddFxOrderCmd(ApiDrpFxRequest.AddFxOrderCmd cmd, CancellationToken cancellation)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["UserId"] = cmd.UserId;
            msg.Properties["Level"] = "错误";

            using var http = _httpClientFactory.CreateClient(string.Empty);

            var r = await new HttpApiInvocation(_log, msg).SetAllowLogOnDebug(true)
                .SetMethod(HttpMethod.Post)
                .SetUrl(_config["AppSettings:DrpfxBaseUrl"] + (!cmd.IsMp ? "/api/FxOrder/AddOrder" : "/api/FxOrder/AddOrderMp"))
                .SetApiDesc("[后台]新增分销记录")
                .SetBodyByJson(cmd)
                .InvokeByAsync<ApiDrpFxResponse.AddFxOrderCmdResult>(http);

            return r.Succeed ? r.Data : null;
        }

        private async Task Handle_BecomHeadUserInHdCmd(ApiDrpFxRequest.BecomHeadUserInHdCmd cmd, CancellationToken cancellation)
        {
            if (cmd.UserId == default)
            {
                return;
            }

            var msg = new NLog.LogEventInfo();
            msg.Properties["UserId"] = cmd.UserId;
            msg.Properties["Level"] = "错误";

            using var http = _httpClientFactory.CreateClient(string.Empty);

            var r = await new HttpApiInvocation(_log, msg).SetAllowLogOnDebug(true)
                .SetApiDesc("call活动期间新下线可能成为顾问api")
                .SetMethod(HttpMethod.Post)
                .SetUrl(_config["AppSettings:DrpfxBaseUrl"] + "/api/FxCenter/BeComeHeadService")
                .SetBodyByJson(cmd)
                .InvokeByAsync(http);
        }

        private async Task<ApiDrpFxResponse.GetConsultantRateQryResult> Handle_GetConsultantRateQry(ApiDrpFxRequest.GetConsultantRateQry query, CancellationToken cancellation)
        {
            if (query.UserId == default)
            {
                return null;
            }

            var msg = new NLog.LogEventInfo();
            msg.Properties["UserId"] = query.UserId;
            msg.Properties["Level"] = "错误";

            using var http = _httpClientFactory.CreateClient(string.Empty);

            var r = await new HttpApiInvocation(_log, msg)
                .SetApiDesc("call获取用户是顾问时的系数api")
                .SetMethod(HttpMethod.Get)
                .SetUrl(_config["AppSettings:DrpfxBaseUrl"] + "/api/MP/CheckUserRole" + "?userid=" + query.UserId)                
                .InvokeByAsync<ApiDrpFxResponse.GetConsultantRateQryResult>(http);

            if (!r.Succeed) return new ApiDrpFxResponse.GetConsultantRateQryResult();
            return r.Data;
        }

        private async Task<ResponseResult<JToken>> Handle_OrgOrderSettleCmd(ApiDrpFxRequest.OrgOrderSettleCmd cmd, CancellationToken cancellation)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["UserId"] = cmd.UserId;
            msg.Properties["Level"] = "错误";

            using var http = _httpClientFactory.CreateClient(string.Empty);

            var r = await new HttpApiInvocation(_log, msg).SetAllowLogOnDebug(true)
                .SetApiDesc("确认收货确定佣金有效api")
                .SetMethod(HttpMethod.Post)
                .SetUrl(_config["AppSettings:DrpfxBaseUrl"] + "/api/FxOrder/OrgOrderSettle")
                .SetBodyByJson(cmd)
                .InvokeByAsync(http);
            return r;
        }
    }

    public class ApiDrpFxRequestHandler2 : IRequestHandler<ApiDrpFxRequest2, ApiDrpFxResponse>
    {
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;
        NLog.ILogger _log;

        public ApiDrpFxRequestHandler2(IHttpClientFactory httpClientFactory, IConfiguration config,
            IServiceProvider services)
        {
            this._httpClientFactory = httpClientFactory;
            this._config = config;
            this._log = services.GetService<NLog.ILogger>();
        }

        public async Task<ApiDrpFxResponse> Handle(ApiDrpFxRequest2 req, CancellationToken cancellation)
        {
            var res = new ApiDrpFxResponse();
            switch (1)
            {
                case int _ when req.AddFxOrder != null:
                    res.Result = await Handle_AddFxOrderCmd(req.AddFxOrder, cancellation);
                    break;
            }
            return res;
        }

        private async Task<ApiDrpFxResponse.AddFxOrderCmdResult> Handle_AddFxOrderCmd(ApiDrpFxRequest.AddFxOrderCmd cmd, CancellationToken cancellation)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["UserId"] = cmd.UserId;
            msg.Properties["Level"] = "错误";

            using var http = _httpClientFactory.CreateClient(string.Empty);

            var r = await new HttpApiInvocation(_log, msg).SetAllowLogOnDebug(true)
                .SetMethod(HttpMethod.Post)
                .SetUrl(_config["AppSettings:DrpfxBaseUrl"] + (!cmd.IsMp ? "/api/FxOrder/AddOrder" : "/api/FxOrder/AddOrderMp"))
                .SetApiDesc("[后台]新增分销记录")
                .SetBodyByJson(cmd)
                .InvokeByAsync<ApiDrpFxResponse.AddFxOrderCmdResult>(http);

            return r.Succeed ? r.Data : throw new CustomResponseException(r.Msg, (int)r.status);
        }
    }
}

