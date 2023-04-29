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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class HdDrpFxQrcodeQueryHandler : IRequestHandler<HdDrpFxQrcodeQuery, HdDrpFxQrcodeQryResult>
    {
        OrgUnitOfWork _unitOfWork;
        IMediator _mediator;
        IConfiguration _config;
        CSRedisClient _redis;
        HttpContext HttpContext;

        public HdDrpFxQrcodeQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IHttpContextAccessor httpContextAccessor,
            IConfiguration config, CSRedisClient redis)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._config = config;
            this._redis = redis;
            HttpContext = httpContextAccessor.HttpContext;
        }

        public async Task<HdDrpFxQrcodeQryResult> Handle(HdDrpFxQrcodeQuery query, CancellationToken cancellation)
        {
            var result = new HdDrpFxQrcodeQryResult();

            var allPics = _config.GetSection("AppSettings:hd_drpfx01:gz_gzh_pic_media_id").Get<string[]>();
            var picIndex = new Random(DateTime.Now.Millisecond).Next(0, allPics.Length);
            var cacheKey = CacheKeys.Hddrpfx01_gzhhf_pic.FormatWith(picIndex);
            
            var backUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/Activity/HdDrpFxReplyGzhPic?picIndex={picIndex}";
            await _redis.SetAsync(cacheKey, backUrl, 60 * 60 * 24 * 30, RedisExistence.Nx);

            result.Url = await _mediator.Send(new CreateWxGzhQrCodeCommand
            {
                GzhAppName = _config["AppSettings:SxbWxGzhAppName"],
                CacheKey = cacheKey,
            });            

            return result;
        }

        
    }
}
