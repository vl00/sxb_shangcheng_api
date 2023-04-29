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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class HdDataInfoQueryHandler : IRequestHandler<HdDataInfoQuery, HdDataInfoDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;        
        CSRedisClient redis;        

        public HdDataInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator,
            CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;            
            this.redis = redis;            
        }

        public async Task<HdDataInfoDto> Handle(HdDataInfoQuery query, CancellationToken cancellation)
        {            
            string code, promoNo = null;
            Guid id = query.Id;            
            string rdkNo = null, rdk = null;
            var dy = new HdDataInfoDto();
            await default(ValueTask);

            if (id == default && query.Code == null)
            {
                return dy;
            }

            // try分析出活动码和推广编号
            Try_resolve_acode_and_promoNo(query.Code, out code, out promoNo);

            if (query.CacheMode == 0 && code != null)
            {
                rdkNo = CacheKeys.Acd_id.FormatWith(code);
                var str_id = await redis.GetAsync<string>(rdkNo);
                if (str_id != null) id = Guid.Parse(str_id);
            }
            if (query.CacheMode == 0 && id != default)
            {
                rdk = CacheKeys.ActivitySimpleInfo.FormatWith(id);
                dy.Data = await redis.GetAsync<Activity>(rdk);
            }
            if (dy.Data == null)
            {
                if (code != null) id = default;                

                var sql = $@"select top 1 a.* from Activity a where 1=1 {"and a.acode=@code".If(code != null)} {"and a.id=@id".If(id != default)} ";
                dy.Data = await unitOfWork.QueryFirstOrDefaultAsync<Activity>(sql, new { code, id });
                id = dy.Id;

                if (query.CacheMode.In(0, 1))
                {
                    rdkNo ??= CacheKeys.Acd_id.FormatWith(code);
                    rdk ??= CacheKeys.ActivitySimpleInfo.FormatWith(id);
                    using var pipe = redis.StartPipe();
                    await pipe.Set(rdkNo, id, 60 * 60 * 24)
                       .Set(rdk, dy.Data, 60 * 60 * 24)
                       .EndPipeAsync();
                }
            }

            dy.PromoNo = promoNo;
            return dy;
        }

        void Try_resolve_acode_and_promoNo(string code, out string acode, out string promoNo)
        {
            acode = promoNo = null;
            if (string.IsNullOrEmpty(code)) return;
            acode = code;
            //
            // ...解析出推广no
            //

        }
    }
}
