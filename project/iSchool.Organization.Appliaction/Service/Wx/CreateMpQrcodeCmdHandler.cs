using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Infras;
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
    public class CreateMpQrcodeCmdHandler : IRequestHandler<CreateMpQrcodeCmd, CreateMpQrcodeCmdResult>
    {
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;        
        NLog.ILogger _log;
        CSRedisClient _redis;

        public CreateMpQrcodeCmdHandler(IHttpClientFactory httpClientFactory, IConfiguration config, CSRedisClient redis,
            IServiceProvider services)
        {
            this._httpClientFactory = httpClientFactory;
            this._config = config;            
            this._log = services.GetService<NLog.ILogger>();
            this._redis = redis;
        }

        public async Task<CreateMpQrcodeCmdResult> Handle(CreateMpQrcodeCmd cmd, CancellationToken cancellation)
        {
            var qstr = $"AppName={cmd.AppName}&Page={cmd.Page}&Scene={cmd.Scene}";
            var mpQrUrl = $"Page={cmd.Page}&Scene={cmd.Scene}";

            var k = CacheKeys.CreateMpQrcode.FormatWith(HashAlgmUtil.Encrypt(qstr, "md5"));
            var jtk = await _redis.GetAsync<JToken>(k);
            if (jtk?.SelectToken("mpqrcode")?.ToString() is string mpqrcode && mpqrcode != string.Empty)
            { 
                return new CreateMpQrcodeCmdResult { MpQrcode = mpqrcode, MpQrUrl = mpQrUrl };
            }

            using var http = _httpClientFactory.CreateClient(string.Empty);

            var r = await new HttpApiInvocation(_log)                
                .SetApiDesc("CreateMpQrcodeCmd")
                .SetMethod(HttpMethod.Get)
                .SetUrl(_config["BaseUrls:DrpfxBaseUrl"] + "/api/mp/GetMinAppQRCode?" + qstr)            
                .OnAfterResponse(async res => 
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        var hr = ResponseResult<string>.Failed(await res.Content.ReadAsStringAsync());
                        hr.status = (ResponseCode)res.StatusCode.ToInt();
                        return hr;
                    }
                    var bys = await res.Content.ReadAsByteArrayAsync();
                    return ResponseResult<string>.Success("data:image/png;base64," + Convert.ToBase64String(bys), null);
                })
                .InvokeByAsync<string>(http);

            if (!r.Succeed)
            {
                throw new CustomResponseException($"创建微信小程序二维码失败: {r.Msg}");
            }
            await _redis.SetAsync(k, new { qstr, mpqrcode = r.Data }, 60 * 60 * 3);

            return new CreateMpQrcodeCmdResult { MpQrcode = r.Data , MpQrUrl= mpQrUrl };
        }

    }
}

