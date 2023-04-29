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
using System.Diagnostics;

namespace iSchool.Organization.Appliaction.Service
{
    public class CheckEvltEditableQueryHandler : IRequestHandler<CheckEvltEditableQuery, CheckEvltEditableQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;        
        CSRedisClient redis;        

        public CheckEvltEditableQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator,
            CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;            
            this.redis = redis;            
        }

        public async Task<CheckEvltEditableQueryResult> Handle(CheckEvltEditableQuery query, CancellationToken cancellation)
        {
            var result = new CheckEvltEditableQueryResult { EvltId = query.EvltId };
            Debugger.Break();

            var ttl = await redis.TtlAsync(CacheKeys.Editdisable_evlt.FormatWith(query.EvltId));
            if (ttl > -2)
            {
                result.Enable = false;
                result.Aeb = query.Aeb;
                result.DisableTtl = ttl > -1 ? TimeSpan.FromSeconds(ttl) : Timeout.InfiniteTimeSpan;
            }
            else
            {
                result.Aeb = query.Aeb ?? await mediator.Send(new ActivityEvltLatestQuery { EvltId = query.EvltId });
                if (result?.Aeb?.Status == (byte)ActiEvltAuditStatus.Ok)
                {
                    var sql = $@"
-- select top 1 * from ActivityRule where IsValid=1 and type=@type and activityid=@hdid 

select ar.* from ActivityEvalMoneyOrder o 
left join ActivityDataHistory h on o.adataid=h.id
outer apply openjson(h.rules) with(ruleid uniqueidentifier '$')r
left join ActivityRule ar on r.ruleid=ar.id
where o.aebid=@aebid and ar.type=@type
";
                    var rule = await unitOfWork.QueryFirstOrDefaultAsync<ActivityRule>(sql, new { aebid = result.Aeb.Id, type = ActivityRuleType.OperationNotAllowed.ToInt(), hdid = result.Aeb.Activityid });
                    result.Rule = rule;
                    if (rule?.Number > 0)
                    {
                        result.DisableTtl = result.Aeb.AuditTime.Value.AddDays(rule.Number.Value) - DateTime.Now;
                        if (result.DisableTtl > TimeSpan.Zero)
                        {
                            await redis.SetAsync(CacheKeys.Editdisable_evlt.FormatWith(query.EvltId), "1", (int)result.DisableTtl.Value.TotalSeconds, RedisExistence.Nx);
                        }
                        else
                        {
                            result.Enable = true;
                            result.DisableTtl = null;
                        }
                    }
                    else
                    {
                        result.Enable = true;
                        result.DisableTtl = null;
                    }
                }
                else
                {
                    result.Enable = true;
                    result.DisableTtl = null;
                }
            }

            return result;
        }        
    }
}
