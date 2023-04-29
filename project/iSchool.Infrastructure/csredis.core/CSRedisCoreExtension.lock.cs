using CSRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool
{
    public static partial class CSRedisCoreExtension
    {
        const string lua_delay = @"";
        const string lua_unlock = @"";

        public static async Task<string> LockExAcquireAsync(this CSRedisClient redis, string ck, int expMs = 30000,
            int retry = 2, int perRetryDelayMs = 1000, bool perRetryDelayIsRandom = true, bool throwLastError = false)
        {
            
            return null; // lck fail
        }

        public static async Task<bool> LockExExtendAsync(this CSRedisClient redis, string ck, string lckid, int expMs = 20000)
        {
            var b = await redis.EvalAsync(lua_delay, ck, lckid, expMs).ConfigureAwait(false);
            return b?.ToString() == "1";
        }

        public static async Task<bool> LockExReleaseAsync(this CSRedisClient redis, string ck, string lckid)
        {
            var b = await redis.EvalAsync(lua_unlock, ck, lckid).ConfigureAwait(false);
            return b?.ToString() == "1";
        }
    }
}
