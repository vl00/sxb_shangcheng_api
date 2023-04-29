using CSRedis;
using Dapper;
using iSchool;
using iSchool.BgServices;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.wx;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MediatR;
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
    public class HdDrpFxReplyGzhPicCommandHandler : IRequestHandler<HdDrpFxReplyGzhPicCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        Openid_WXOrgUnitOfWork _openid_WXOrgUnitOfWork;
        IMediator _mediator;
        IConfiguration _config;
        CSRedisClient _redis;
        IHttpClientFactory _httpClientFactory;
        RabbitMQConnectionForPublish _rabbit;
        HttpContext HttpContext;

        public HdDrpFxReplyGzhPicCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IOpenid_WXUnitOfWork openid_WXOrgUnitOfWork,
            IHttpClientFactory httpClientFactory, RabbitMQConnectionForPublish rabbit, IHttpContextAccessor httpContextAccessor,
            IConfiguration config, CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._config = config;
            this._redis = redis;
            _openid_WXOrgUnitOfWork = (Openid_WXOrgUnitOfWork)openid_WXOrgUnitOfWork;
            _httpClientFactory = httpClientFactory;
            _rabbit = rabbit;
            HttpContext = httpContextAccessor.HttpContext;
        }

        public async Task<bool> Handle(HdDrpFxReplyGzhPicCommand cmd, CancellationToken cancellation)
        {
#if DEBUG
            if (cmd != null)
            {
                using var channel = _rabbit.OpenChannel();
                channel.ConfirmSelect();
                channel.BasicPublish("amq.fanout", "iSchool.Organization.Appliaction.RequestModels.HdDrpFxReplyGzhPicCommand", false, null,
                    Encoding.UTF8.GetBytes(new 
                    { 
                        BaseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}",
                        cmd.OpenId, cmd.PicIndex,
                    }.ToJsonString(camelCase: true)));
                channel.WaitForConfirms(TimeSpan.FromSeconds(2));
            }
#endif

            // find userId by openId
            var sql = "select OpenId,UserId,AppName from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and OpenId=@OpenId";
            var (openId, userId, appName) = await _openid_WXOrgUnitOfWork.QueryFirstOrDefaultAsync<(string, Guid, string)>(sql, new { cmd.OpenId });
            if (appName == default) return false;

            // find media_id
            var media_id = _config.GetSection("AppSettings:hd_drpfx01:gz_gzh_pic_media_id").Get<string[]>()[cmd.PicIndex];

            // send image
            var err = await _mediator.Send(new WxGzhSendImageToUserCommand
            {
                GzhAppName = _config["AppSettings:SxbWxGzhAppName"],
                OpenIdToUser = openId,
                MediaId = media_id,
            });

            return err == null;
        }

        
    }
}
