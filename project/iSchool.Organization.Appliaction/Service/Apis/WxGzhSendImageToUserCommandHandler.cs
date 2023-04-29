using CSRedis;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.wx;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace iSchool.Organization.Appliaction.Service
{
    public class WxGzhSendImageToUserCommandHandler : IRequestHandler<WxGzhSendImageToUserCommand, string>
    {
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;
        IMediator _mediator;
        NLog.ILogger _log;

        public WxGzhSendImageToUserCommandHandler(IHttpClientFactory httpClientFactory, IConfiguration config, 
            NLog.ILogger log,
            IMediator mediator)
        {
            this._httpClientFactory = httpClientFactory;
            this._config = config;
            this._mediator = mediator;
            _log = log;
        }

        public async Task<string> Handle(WxGzhSendImageToUserCommand cmd, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(cmd.GzhAppName)) throw new CustomResponseException("公众号appname为空");

            var accessTokenInfo = await _mediator.Send(new GetWxGzhAccessTokenQuery { GzhAppName = cmd.GzhAppName });
            var msgUrl = cmd.CustomerServiceApiUrl + accessTokenInfo.Token;

            using var http = _httpClientFactory.CreateClient(string.Empty);

            // 正式要跟正式公众号48小时有交互才能发送成功, 否则会报 45015 'response out of time limit or subscription is canceled rid'
            var r = await new HttpApiInvocation(HttpMethod.Post, msgUrl, _log)
                .SetApiDesc("wx公众号发送客服消息-图片给用户")
                .SetBodyByJson(new
                {
                    touser = cmd.OpenIdToUser,
                    msgtype = "image",
                    image = new { media_id = cmd.MediaId },
                })
                .SetResBodyParser(json =>
                {
                    var jtk = JToken.Parse(json);
                    var code = (int?)jtk["errcode"] ?? 0;
                    if (code == 0) return ResponseResult<bool>.Success(true);
                    var r = ResponseResult<bool>.Failed(jtk["errmsg"]?.ToString());
                    r.status = (ResponseCode)code;
                    return r;
                })
                .InvokeByAsync<bool>(http);

            return r.Succeed ? null : r.Msg;
        }

        
    }
}
