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
    public class ClearLikesCachesCommandHandler : IRequestHandler<ClearLikesCachesCommand>
    {
        IMediator mediator;
        CSRedisClient redis;      

        public ClearLikesCachesCommandHandler(IMediator mediator, CSRedisClient redis)
        {
            this.mediator = mediator;
            this.redis = redis;
        }

        public async Task<Unit> Handle(ClearLikesCachesCommand cmd, CancellationToken cancellation)
        {
            if (cmd.Ids?.Any() != true) return default;            
            switch (cmd.Type)
            {
                default:
                case 1: // 评测
                    {
                        var keys = new List<(string, string)>(20);
                        keys.AddRange(cmd.Ids.Select(id => (CacheKeys.EvaluationLikeAction, $"{id}|*")));
                        keys.AddRange(cmd.Ids.Select(id => (CacheKeys.EvaluationLikesCount.FormatWith(id), "shamlikes")));
                        // 不能删除cache-我点赞过的评测
                        //keys.AddRange(cmd.Ids.Select(id => (CacheKeys.MyEvaluationLikes.FormatWith("*"), id.ToString())));
                        await redis.BatchDelAsync(keys, cmd.TimeoutSeconds);
                        keys.Clear();
                        keys.Add(("org:evltsMain:*", null));
                        keys.Add(("org:spcl:id_*", null));
                        await redis.BatchDelAsync(keys, cmd.TimeoutSeconds);
                    }
                    break;
                case 2: // 评论
                    {
                        var keys = new List<(string, string)>(20);
                        keys.AddRange(cmd.Ids.Select(id => (CacheKeys.CommentLikeAction, $"*|{id}|*")));                        
                        keys.AddRange(cmd.Ids.Select(id => (CacheKeys.EvaluationCommentLikesCount.FormatWith("*"), $"{id}")));
                        //顺便
                        keys.Add((CacheKeys.Del_EvltCommentTopN, null));
                        keys.Add((CacheKeys.EvaluationLikesCount.FormatWith("*"), null));
                        await redis.BatchDelAsync(keys, cmd.TimeoutSeconds);
                    }
                    break;
            }
            return default;
        }
    }
}
