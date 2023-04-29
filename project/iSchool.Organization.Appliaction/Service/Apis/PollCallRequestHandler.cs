using CSRedis;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class PollCallRequestHandler : IRequestHandler<PollCallRequest, PollCallResponse>
    {
        private readonly CSRedisClient redis;
        private readonly IServiceProvider services;

        public PollCallRequestHandler(CSRedisClient redis, IServiceProvider services)
        {
            this.redis = redis;
            this.services = services;
        }

        public async Task<PollCallResponse> Handle(PollCallRequest req, CancellationToken cancellation)
        {
            var res = new PollCallResponse();
            if (req.Query != null)
            {
                if (req.Query.DelayMs == -1) res.PollQryResult = await OnHandle(req.Query, cancellation);
                else
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                    cts.CancelAfter(req.Query.DelayMs);
                    while (!cts.IsCancellationRequested)
                    {
                        try
                        {
                            res.PollQryResult = await OnHandle(req.Query, cts.Token);
                            if (res.PollQryResult.HasResult) break;
                        }
                        catch when (cts.IsCancellationRequested)
                        {
                            res.PollQryResult = new PollResult { Id = req.Query?.Id };
                        }
                    }
                }
            }
            else if (req.SetResultCmd != null)
                res.IsSetResultOk = await OnHandle(req.SetResultCmd);
            else if (req.PreSetCmd != null)
                res.IsPreSetOk = await OnHandle(req.PreSetCmd);
            return res;
        }

        private async Task<PollResult> OnHandle(PollQuery query, CancellationToken cancellation)
        {
            var result = new PollResult { Id = query?.Id };
            if (string.IsNullOrEmpty(query?.Id))
            {
                return result;
            }

            await Task.Delay(500, cancellation);

            var k = query.Id.StartsWith("org:poll:") ? query.Id : $"org:poll:{query.Id}";
            var dict = await redis.HGetAllAsync(k);
            if (dict?.Count < 1)
            {
                return result;
            }

            var rrc = !dict.TryGetValue("rrc", out var _rrc) ? -2 :
                int.TryParse(_rrc, out var _rrc1) ? _rrc1 : -2;

            if (!query.IgnoreRrc)
            {
                if (rrc < -1) // -2
                {
                    return result;
                }
                if (rrc != -1)
                {
                    if ((rrc -= 1) <= 0) await redis.DelAsync(k);
                    else await redis.HIncrByAsync(k, "rrc", -1);
                }
            }

            result.Rrc = rrc;
            result.ResultType = Type.GetType(dict.GetValueEx("result-type", ""));
            result.ResultStr = dict.GetValueEx("result");
            result.HasResult = true;
            return result;
        }

        private async Task<bool> OnHandle(PollSetResultCommand cmd)
        {
            var k = cmd.Id.StartsWith("org:poll:") ? cmd.Id : $"org:poll:{cmd.Id}";
            var r = cmd.Result;
            var rty = cmd.Result?.GetType();
            if (rty == null) return false;

            try
            {
                if (cmd.CheckIfExists)
                {
                    if (rty.FullName != await redis.HGetAsync(k, "result-type"))
                    {
                        //throw new CustomResponseException($"设置轮询结果失败,k='{k}'不存在", 501);
                        return false;
                    }
                }

                if (true)
                {
                    using var pipe = redis.StartPipe();
                    pipe.HMSet(k, new object[]
                    {
                        "result", r.ToJsonString(camelCase: true),
                        "result-type", rty.FullName,
                        "rrc", cmd.Rrc,
                    });
                    pipe.Expire(k, cmd.ExpSec);
                    await pipe.EndPipeAsync();
                }
            }
            catch
            {
                throw new CustomResponseException($"设置轮询结果失败,k={k}", 500);
            }

            return true;
        }

        private async Task<bool> OnHandle(PollPreSetCommand cmd)
        {
            var k = cmd.Id.StartsWith("org:poll:") ? cmd.Id : $"org:poll:{cmd.Id}";
            var rty = cmd.ResultType;
            if (rty == null) return false;

            try
            {
                using var pipe = redis.StartPipe();                
                pipe.HMSet(k, new[]
                {
                    "result", cmd.ResultStr,
                    "result-type", rty,
                    //"rrc", "0",
                });
                pipe.Expire(k, cmd.ExpSec);
                await pipe.EndPipeAsync();
            }
            catch
            {
                throw new CustomResponseException($"设置轮询失败,k={k}", 500);
            }

            return true;
        }
    }
}
