using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace iSchool.Organization.Appliaction.Service
{
    public class SlmActivityInfoQueryHandler : IRequestHandler<SlmActivityInfoQuery, SlmActivityInfoDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;        
        CSRedisClient redis;
        IUserInfo me;
        IConfiguration config;

        public SlmActivityInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me,
            IConfiguration config,
            CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;            
            this.redis = redis;
            this.me = me;
            this.config = config;
        }

        public async Task<SlmActivityInfoDto> Handle(SlmActivityInfoQuery query, CancellationToken cancellation)
        {                        
            var result = new SlmActivityInfoDto();
            await default(ValueTask);

            var info = await mediator.Send(new HdDataInfoQuery { Code = query.Code, CacheMode = 0 });
            result.Data = info.Data;

            if (!me.IsAuthenticated) throw new CustomResponseException("no login", ResponseCode.NoLogin.ToInt());

            // 判断是否到达每日上限
            if (result.Status == (int)ActivityFrontStatus.Ok)
            {
                var uc = await mediator.Send(new UserHd2ActiQuery 
                { 
                    ActivityId = info.Id,
                    UserId = me.UserId 
                });
                if (info.Data.Limit != null && uc.Allcount_now >= info.Data.Limit)
                {
                    result.Status = ActivityFrontStatus.DayLimited.ToInt();
                    result.EvltCount = uc.Allcount_now;
                }
                if (uc.Ocount > 0)
                    result.Ustatus = UserAccountInvalidType.MobileExcp.ToInt();
            }

            // qrcode 暂不需要根据推广号'info.PromoNo'返回不同的qrcode
            var astatus = (ActivityFrontStatus)result.Status;
            switch (astatus)
            {                
                case ActivityFrontStatus.DayLimited:
                    {
                        var bys = await File.ReadAllBytesAsync(Path.Combine(Directory.GetCurrentDirectory(), config["AppSettings:hd2:hd_daylimited_qrcode"]));
                        result.Qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
                    }
                    break;
                default:
                    //if (astatus != ActivityFrontStatus.Ok)
                    //if (result.Ustatus != 0)
                    {
                        var bys = await File.ReadAllBytesAsync(Path.Combine(Directory.GetCurrentDirectory(), config["AppSettings:hd2:hd_tile_qrcode"]));
                        result.Qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
                    }
                    break;
            }
            
            return result;
        }

        
    }
}
