using CSRedis;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.wx;
using iSchool.Organization.Domain;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
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
    public class CreateWxGzhQrCodeCommandHandler : IRequestHandler<CreateWxGzhQrCodeCommand, string>
    {
        IHttpClientFactory httpClientFactory;
        IConfiguration config;
        CSRedisClient redis;
        IMediator mediator;

        public CreateWxGzhQrCodeCommandHandler(IHttpClientFactory httpClientFactory, IConfiguration config, CSRedisClient redis,
            IMediator mediator)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;
            this.redis = redis;
            this.mediator = mediator;
        }

        public async Task<string> Handle(CreateWxGzhQrCodeCommand cmd, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(cmd.GzhAppName)) return null;
            cmd.AccessTokenApiUrl = !string.IsNullOrEmpty(cmd.AccessTokenApiUrl) ? cmd.AccessTokenApiUrl : config["AppSettings:AccessTokenApi"];
            
            var tokenInfo = await mediator.Send(new GetWxGzhAccessTokenQuery { GzhAppName = cmd.GzhAppName });

            using var http = httpClientFactory.CreateClient(string.Empty);

            WxQRCodeUrlResultResponse res = null;
            var hres = await http.PostAsync(cmd.CreateQRCodeUrl.FormatWith(tokenInfo.Token), new StringContent(WxQRCodeCreateRequest.New(cmd.CacheKey, cmd.Expsec)));
            try
            {
                hres.EnsureSuccessStatusCode();
                res = (await hres.Content.ReadAsStringAsync()).ToObject<WxQRCodeUrlResultResponse>();
            }
            catch (Exception ex)
            {
                throw new CustomResponseException("获取二维码生成ticket意外失败\n" + ex.Message);
            }

            return cmd.GetQRCodeUrl.FormatWith(HttpUtility.UrlEncode(res.Ticket));
        }

        
    }
}
