using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.Apis;
using iSchool.Organization.Appliaction.ViewModels.wx;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace iSchool.Organization.Appliaction.Service
{
    public class SendMobileMessageCommandHandler : IRequestHandler<SendMobileMessageCommand, bool>
    {
        IHttpClientFactory httpClientFactory;
        IConfiguration config;
        CSRedisClient redis;
      

        public SendMobileMessageCommandHandler(IHttpClientFactory httpClientFactory, IConfiguration config, CSRedisClient redis)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;
            this.redis = redis;
           
        }

        public  Task<bool> Handle(SendMobileMessageCommand cmd, CancellationToken cancellation)
        {
            var appid =Convert.ToInt32(config.GetSection("AppSettings:QcloudAppId").Value);
            var apptoken = config.GetSection("AppSettings:QcloudAppKey").Value;
            TXSMSHelper smsHelper = new TXSMSHelper();
            SmsSingleSenderResult res = smsHelper.SendWithParam(appid, apptoken, cmd.NationCode, cmd.Mobile,cmd.TemplateId,cmd.TempalteParam,"上学帮",null,null );
            return Task.FromResult(res.result == 0 ? true : false);
        }

        
    }
}
