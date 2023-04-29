using System;
using System.Threading.Tasks;

namespace iSchool.Domain.Modles
{
    /// <summary>
    /// await using var disposable = new DisposableSlim<T>(obj, o => o.XXX());
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct DisposableSlim<T> where T : class
    {
        readonly Action<T> fa;
        readonly Func<T, Task> ft;
        readonly Func<T, ValueTask> fvt;
        readonly T obj;

        public T Obj => obj;

        public DisposableSlim(T obj, Action<T> fa) : this(obj, fa, null, null) { }
        public DisposableSlim(T obj, Func<T, Task> ft) : this(obj, null, null, ft) { }
        public DisposableSlim(T obj, Func<T, ValueTask> fvt) : this(obj, null, fvt, null) { }

        private DisposableSlim(T obj, Action<T> fa, Func<T, ValueTask> fvt, Func<T, Task> ft)
        {
            this.obj = obj;
            this.fa = fa;
            this.ft = ft;
            this.fvt = fvt;
        }

        public async ValueTask DisposeAsync()
        {
            if (fa != null)
            {
                fa(obj);
            }
            else if (fvt != null)
            {
                var t = fvt(obj);
                await t;
            }
            else if (ft != null)
            {
                var t = ft(obj);
                if (t != null) await t;
            }
        }
    }
}