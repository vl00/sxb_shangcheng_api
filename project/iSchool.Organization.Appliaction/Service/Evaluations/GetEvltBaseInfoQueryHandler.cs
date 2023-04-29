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
    public class GetEvltBaseInfoQueryHandler : IRequestHandler<GetEvltBaseInfoQuery, EvltBaseInfoDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;

        const int cache_exp = 60 * 30;

        public GetEvltBaseInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis, IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
        }

        public async Task<EvltBaseInfoDto> Handle(GetEvltBaseInfoQuery req, CancellationToken cancellation)
        {
            var evltId = req.EvltId;
            string sql = null;
            string rdkNo = null, rdk = null;  // redis key
            EvltBaseInfoDto dy = null;

            if (evltId == default)
            {
                rdkNo = CacheKeys.EvltNo.FormatWith(req.No);
                var str_evltId = await redis.GetAsync<string>(rdkNo);
                if (str_evltId != null)
                {
                    evltId = Guid.Parse(str_evltId);
                }
            }
            if (evltId != default)
            {
                rdk = CacheKeys.Evlt.FormatWith(evltId);
                dy = await redis.HGetAsync<EvltBaseInfoDto>(rdk, "base");                
            }
            if (dy == null)
            {
                sql = $@"
select evlt.Id,evlt.No,evlt.title,evlt.stick,evlt.isplaintext,evlt.hasvideo,evlt.cover,evlt.userid as AuthorId,evlt.CreateTime,evlt.mode,evlt.Mtime,
evlt.CollectionCount,evlt.CommentCount,evlt.Likes as LikeCount,evlt.ViewCount,
s.id as SpecialId,s.title as SpecialName,s.No as SpecialNo
from Evaluation evlt
left join SpecialBind sb on sb.evaluationid=evlt.id and sb.IsValid=1
left join Special s on s.id=sb.SpecialId and s.IsValid=1 and s.status={SpecialStatusEnum.Ok.ToInt()}
where evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()}
{"and evlt.Id=@Id".If(evltId != default)} {"and evlt.no=@no".If(evltId == default)}
";
                dy = await unitOfWork.QueryFirstOrDefaultAsync<EvltBaseInfoDto>(sql, new { no = req.No, Id = evltId });
                if (dy == null) throw new CustomResponseException($"无效的评测no={req.No}", 404);
                evltId = dy.Id;

                rdkNo ??= CacheKeys.EvltNo.FormatWith(dy.No);
                rdk ??= CacheKeys.Evlt.FormatWith(evltId);
                await redis.StartPipe()
                    .Set(rdkNo, evltId, 60 * 60 * 1)
                    .HSet(rdk, "base", dy)
                    .Expire(rdk, cache_exp)
                    .EndPipeAsync();
            }
            if (req.No == default) req.No = dy.No;
            
            return dy;
        }

        
    }
}
