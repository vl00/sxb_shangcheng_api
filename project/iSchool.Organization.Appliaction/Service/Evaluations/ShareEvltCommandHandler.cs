using CSRedis;
using Dapper;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class ShareEvltCommandHandler : IRequestHandler<ShareEvltCommand, ShareLinkDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;
        IConfiguration config;
        IUserInfo me;

        public ShareEvltCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IConfiguration config, IUserInfo me,
            CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
            this.config = config;
            this.me = me;
        }

        public async Task<ShareLinkDto> Handle(ShareEvltCommand cmd, CancellationToken cancellation)
        {
            var baseInfo = await mediator.Send(new GetEvltBaseInfoQuery { EvltId = cmd.Id });
            var dto = new ShareLinkDto();
            dto.Banner = baseInfo.Cover;
            dto.MainTitle = baseInfo.Title;
            dto.SubTitle = "分享你一篇精彩的课程评测";

            // user为 作者 or 分享者
            {
                //var au = await mediator.Send(new UserSimpleInfoQuery
                //{
                //    UserIds = new[] { baseInfo.AuthorId }
                //});
                //dto.Username = au.FirstOrDefault()?.Nickname;
                //dto.UserHeadImg = au.FirstOrDefault()?.HeadImgUrl;

                dto.Username = me.IsAuthenticated ? me.UserName : null;
                dto.UserHeadImg = me.IsAuthenticated ? me.HeadImg : null;
            }

            var sharedUrl = string.Format(config["AppSettings:shareEvltDetialUrl"], UrlShortIdUtil.Long2Base32(baseInfo.No), cmd.Cnl);
            if (cmd.Cnl == "pc") sharedUrl += "&qrcode=true";
            dto.Base64QRCode = QRCodeHelper.GetLogoQRCode(sharedUrl, Path.Combine("App_Data/images/iSchoollogo.png"), 5);

            return dto;
        }
    }
}
