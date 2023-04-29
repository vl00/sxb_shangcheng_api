using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetSpecialInfoQueryHandler : IRequestHandler<GetSpecialInfoQuery, Special>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;        

        public GetSpecialInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;            
        }

        public async Task<Special> Handle(GetSpecialInfoQuery req, CancellationToken cancellation)
        {
            var id = req.SpecialId;
            string rdkNo = null, rdk = null;
            Special dy = null;

            if (id == default)
            {
                rdkNo = CacheKeys.SpclNo.FormatWith(req.No);
                var str_id = await redis.GetAsync<string>(rdkNo);
                if (str_id != null)
                {
                    id = Guid.Parse(str_id);
                }
            }
            if (id != default)
            {
                rdk = CacheKeys.Rdk_spcl.FormatWith(id);
                dy = await redis.GetAsync<Special>(rdk);
            }
            if (dy == null)
            {
                var sql = $@"
select * from [Special] where IsValid=1 and status={SpecialStatusEnum.Ok.ToInt()}
{"and Id=@Id".If(id != default)} {"and No=@No".If(id == default)}
";
                dy = await unitOfWork.QueryFirstOrDefaultAsync<Special>(sql, new { req.No, Id = id });
                if (dy == null) throw new CustomResponseException($"无效专题no={req.No}");
                id = dy.Id;

                rdkNo ??= CacheKeys.SpclNo.FormatWith(dy.No);
                rdk ??= CacheKeys.Rdk_spcl.FormatWith(id);
                await redis.StartPipe()
                    .Set(rdkNo, id, 60 * 60 * 1)
                    .Set(rdk, dy, 60 * 100)
                    .EndPipeAsync();
            }
            if (req.No == default) req.No = dy.No;

            return dy;
        }
    }
}
