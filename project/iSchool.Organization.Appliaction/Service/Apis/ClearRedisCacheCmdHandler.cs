using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class ClearRedisCacheCmdHandler : IRequestHandler<ClearRedisCacheCmd>
    {
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public ClearRedisCacheCmdHandler(IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<Unit> Handle(ClearRedisCacheCmd cmd, CancellationToken cancellation)
        {
            Task t = null;
            if (cmd.Keys?.Any() == true)
            {
                t = _redis.BatchDelAsync(cmd.Keys, cmd.ExecSec);
            }
            if (cmd.Keys2?.Any() == true)
            {
                t = Task.WhenAll(t ?? Task.CompletedTask, _redis.BatchDelAsync(cmd.Keys2, cmd.ExecSec));
            }
            if (t != null)
            {
                await Task.WhenAny(t, Task.Delay(TimeSpan.FromSeconds(cmd.WaitSec)));
            }
            return default;
        }

    }
}
