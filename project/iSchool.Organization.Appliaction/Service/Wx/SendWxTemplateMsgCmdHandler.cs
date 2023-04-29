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
    public class SendWxTemplateMsgCmdHandler : IRequestHandler<SendWxTemplateMsgCmd, SendWxTemplateMsgCmdResult>
    {
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;        
        NLog.ILogger _log;
        CSRedisClient _redis;
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;

        public SendWxTemplateMsgCmdHandler(IHttpClientFactory httpClientFactory, IConfiguration config, CSRedisClient redis,
            IOrgUnitOfWork orgUnitOfWork, IMediator mediator,
            IServiceProvider services)
        {
            this._httpClientFactory = httpClientFactory;
            this._config = config;            
            this._log = services.GetService<NLog.ILogger>();
            this._redis = redis;
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
        }

        public async Task<SendWxTemplateMsgCmdResult> Handle(SendWxTemplateMsgCmd cmd, CancellationToken cancellation)
        {
            var result = new SendWxTemplateMsgCmdResult();

            if (string.IsNullOrEmpty(cmd.WechatTemplateSendCmd.OpenId))
            {
                var openid = await _orgUnitOfWork.QueryFirstOrDefaultAsync<string>($@"
                    select top 1 openID from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and userID=@UserId; 
                ", new { cmd.UserId });

                if (string.IsNullOrEmpty(openid))
                {
                    throw new CustomResponseException($"用户({cmd.UserId})没openid,可能没关注公众号.", 404);
                }
                cmd.WechatTemplateSendCmd.OpenId = openid;
            }

            result.Succeed = await _mediator.Send(cmd.WechatTemplateSendCmd);

            return result;
        }

    }
}

