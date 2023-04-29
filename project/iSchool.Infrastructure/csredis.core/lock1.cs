using CSRedis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Infras.Locks
{
    public class Lock1Option
    {
        public Lock1Option(string ck)
            : this(ck, 30000)
        { }

        public Lock1Option(string ck, TimeSpan exp)
            : this(ck, (int)exp.TotalMilliseconds)
        { }

        public Lock1Option(string ck, int expMs)
        {
            this.CK = ck;
            this.ExpMs = expMs;
            this.Retry = 2;
            this.RetryDelayMs = 1000;
        }

        public string CK { get; set; }
        public int ExpMs { get; set; }
        /// <summary>
        /// 重试次数.<br/>
        /// 0 = 不重试<br/>
        /// -1 = 无限重试<br/>
        /// </summary>
        public int Retry { get; set; }
        public int RetryDelayMs { get; set; }
        public bool RetryDelayIsRandom { get; set; } = true;
        public bool IsLongLck { get; set; }

        public Lock1Option SetExpSec(int sec)
        {
            ExpMs = sec * 1000;
            return this;
        }

        public Lock1Option SetRetry(int retry, int delayMs = 1000, bool delayIsRandom = true)
        {
            Retry = retry;
            RetryDelayMs = delayMs;
            RetryDelayIsRandom = delayIsRandom;
            return this;
        }

        public Lock1Option SetIsLongLck(bool isLongLck = true)
        {
            IsLongLck = isLongLck;
            return this;
        }
    }
    public interface ILock1Factory
    {
        Task<ILock1> LockAsync(string ck, int expMs = 30000, int retry = 2, int retryDelayMs = 1000);
        Task<ILock1> LockAsync(Lock1Option opt);
    }
    public interface ILock1 : IAsyncDisposable
    {
        string CK { get; }
        string ID { get; }
        bool IsAvailable { get; }

        Task<bool> ExtendAsync(int? expMs = null);
    }

    public sealed class CSRedisCoreLock1Factory : ILock1Factory
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
        readonly CSRedisClient redis;

        public CSRedisCoreLock1Factory(CSRedisClient redis)
        {
            this.redis = redis;
        }

        public Task<ILock1> LockAsync(string ck, int expMs = 30000, int retry = 2, int retryDelayMs = 1000)
        {
            return LockAsync(new Lock1Option(ck) { ExpMs = expMs, Retry = retry, RetryDelayMs = retryDelayMs });
        }

        public async Task<ILock1> LockAsync(Lock1Option opt)
        {
            var lckid = await redis.LockExAcquireAsync(opt.CK, opt.ExpMs, opt.Retry, opt.RetryDelayMs, opt.RetryDelayIsRandom).ConfigureAwait(false);
            return lckid != null ? new Lock1(redis, opt.CK, lckid, opt.ExpMs, opt.IsLongLck) : new Lock1(null, null, null, default, false);
        }

        sealed class Lock1 : ILock1
        {
            readonly int _expMs;
            volatile int _avail;
            string _lckid;
            CSRedisClient redis;
            DateTime _lckAtTime;
            bool _isLongLck;

            public string CK { get; }
            public string ID => _lckid;

            public Lock1(CSRedisClient redis, string ck, string lckid, int expMs, bool isLongLck)
            {
                CK = ck;
                _lckid = lckid;
                _expMs = expMs;
                this.redis = redis;
                _isLongLck = isLongLck;

                if (lckid != null)
                {
                    _avail = 1;
                    _lckAtTime = DateTime.Now;
                }
            }

            public bool IsAvailable
            {
                get
                {
                    if (_lckid == null || redis == null) return false;
                    if (_avail == 0) return false;
                    return _lckAtTime.AddMilliseconds(_expMs) > DateTime.Now;
                }
            }

            public async Task<bool> ExtendAsync(int? expMs = null)
            {
                if (_avail == 0) return false;
                try
                {
                    var pttl = expMs ?? _expMs;
                    var startTimestamp = Stopwatch.GetTimestamp();
                    if (await redis.LockExExtendAsync(CK, _lckid, pttl))
                    {
                        if (Interlocked.CompareExchange(ref _avail, 1, 1) == 0)
                            return false;

                        _lckAtTime = GetRemainingValidityTicks(pttl, startTimestamp, Stopwatch.GetTimestamp());
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }

            public ValueTask DisposeAsync() => new ValueTask(CoreDisposeAsync());

            async Task CoreDisposeAsync()
            {
                if (_avail == 0 || Interlocked.CompareExchange(ref _avail, 0, 1) == 0)
                    return;

                var lckid = _lckid;
                _lckid = null;
                try
                {
                    if (_isLongLck) await redis.DelAsync(CK);
                    else await redis.LockExReleaseAsync(CK, lckid);
                }
                catch { }
                finally { redis = null; }
            }
        }

        static DateTime GetRemainingValidityTicks(int expMs, long startTimestamp, long endTimestamp)
        {
            var swTicks = (long)(TimestampToTicks * (endTimestamp - startTimestamp));
            var expiryTime = TimeSpan.FromMilliseconds(expMs);
            var driftTicks = (long)(expiryTime.Ticks * 0.01) + TimeSpan.FromMilliseconds(2).Ticks;
            return DateTime.Now.AddTicks(-1 * swTicks - driftTicks);
        }
    }
}
