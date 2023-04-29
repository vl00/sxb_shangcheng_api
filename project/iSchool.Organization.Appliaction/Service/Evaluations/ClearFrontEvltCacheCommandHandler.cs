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
    public class ClearFrontEvltCacheCommandHandler : IRequestHandler<ClearFrontEvltCacheCommand>
    {
        IMediator mediator;
        CSRedisClient redis;      

        public ClearFrontEvltCacheCommandHandler(IMediator mediator, CSRedisClient redis)
        {
            this.mediator = mediator;
            this.redis = redis;
        }

        public async Task<Unit> Handle(ClearFrontEvltCacheCommand cmd, CancellationToken cancellation)
        {
            await redis.BatchDelAsync(new[]
            {
                string.Format(CacheKeys.simpleevaluation, cmd.EvltId),
                CacheKeys.Evlt.FormatWith(cmd.EvltId),
                CacheKeys.Evlt.FormatWith(cmd.EvltId) + ":*",                
            });
            if (cmd.SpclId != null)
            {
                _ = redis.BatchDelAsync(new[] 
                {
                    CacheKeys.Rdk_spcl.FormatWith(cmd.SpclId),
                    $"{CacheKeys.Rdk_spcl.FormatWith(cmd.SpclId)}:*"
                }, 3);
            }
            
            var cacheKeys = new List<string>();
            cacheKeys.Add(CacheKeys.Del_evltMain); //评测首页缓存|分页列表|课程评测
            cacheKeys.Add("org:organization:evlts:*"); //机构详情-相关评测
            cacheKeys.Add("org:*:relatedEvlts:*"); //详情-相关评测s
            cacheKeys.Add(CacheKeys.Rdk_spcl.FormatWith("*")); //大小专题
            cacheKeys.Add(CacheKeys.CourseDetails.FormatWith("*")); //放最后,课程里有评测
            AsyncUtils.StartNew((_0, _1) => redis.BatchDelAsync(cacheKeys, 30));

            return default;
        }
    }
}
