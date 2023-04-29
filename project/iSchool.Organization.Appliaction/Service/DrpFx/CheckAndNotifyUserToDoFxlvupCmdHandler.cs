using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Appliaction.Wechat;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class CheckAndNotifyUserToDoFxlvupCmdHandler : IRequestHandler<CheckAndNotifyUserToDoFxlvupCmd>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;        
        IConfiguration _config;
        NLog.ILogger _log;

        public CheckAndNotifyUserToDoFxlvupCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            NLog.ILogger log,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;            
            this._redis = redis;            
            this._config = config;
            this._log = log;
        }

        public async Task<Unit> Handle(CheckAndNotifyUserToDoFxlvupCmd cmd, CancellationToken cancellation)
        {
            // 判断是否顾问
            var headInfo = ((await _mediator.Send(new ApiDrpFxRequest { Ctn = new ApiDrpFxRequest.GetConsultantRateQry { UserId = cmd.UserId } }))
                .Result as ApiDrpFxResponse.GetConsultantRateQryResult);

            if (headInfo?.IsConsultant == true || headInfo?.IsHighConsultant == true)
            {
                return default;
            }

            // is 普通粉丝
            //

            var teamInfo = await _mediator.Send(new GetHeaderFxTeamSetupQuery { UserId = cmd.UserId });
            if (teamInfo.IsCondition1Ok)
            {
                var openid = await _orgUnitOfWork.QueryFirstOrDefaultAsync<string>($@" select openID from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and userID='{cmd.UserId}'; ");
                if (string.IsNullOrEmpty(openid))
                {
                    LogError(cmd.UserId, "通知升级顾问", null, new Exception("no openid"));
                    return default;
                }
                var wechatNotify = new WechatTemplateSendCmd()
                {
                    KeyWord1 = $"恭喜您，您已经满足升级初级顾问条件，点击去升级吧",
                    KeyWord2 = DateTime.Now.ToDateTimeString(),
                    OpenId = openid,
                    Remark = "点击去升级",
                    MsyType = WechatMessageType.升级顾问通知,
                };
                try { await _mediator.Send(wechatNotify); }
                catch (Exception ex)
                {
                    LogError(cmd.UserId, "通知升级顾问", wechatNotify.ToJsonString(camelCase: true), ex);
                }
            }

            return default;
        }

        void LogError(Guid userid, string errdesc, string paramsStr, Exception ex, int errcode = 500)
        {
            if (_log != null)
            {
                _log.Error(_log.GetNLogMsg(nameof(OrderPayedOkEvent))
                    .SetUserId(userid)
                    .SetParams(paramsStr)
                    .SetLevel("错误")
                    .SetError(ex, errdesc, errcode));
            }
        }
    }
}
