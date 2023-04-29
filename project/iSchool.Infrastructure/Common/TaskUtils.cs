using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Infrastructure
{
    public static partial class TaskUtils
    {
        /// <summary>
        /// await $task.AwaitNoErr(); // no-throw
        /// </summary>
        public static Task AwaitNoErr(this Task task)
        {
            if (task?.IsCompletedSuccessfully != false) return task;
            return task.ContinueWith(t => _ = t.Exception,
                CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        #region AwaitResOrErr
        /// <summary>
        /// var (_, ex) = await $task.AwaitCaptureResult(); // no-throw
        /// </summary>
        public static Task<(object Result, Exception Error)> AwaitResOrErr(this Task task)
        {
            return task.ContinueWith<(object, Exception)>(t => (null, (t.Exception?.InnerExceptions?.FirstOrDefault() ?? t.Exception)),
                CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
        /// <summary>
        /// var (r, ex) = await $task.AwaitResOrErr(); // no-throw
        /// </summary>
        public static Task<(T Result, Exception Error)> AwaitResOrErr<T>(this Task<T> task)
        {
            return task.ContinueWith(t => ((t.Exception == null ? t.Result : default), (t.Exception?.InnerExceptions?.FirstOrDefault() ?? t.Exception)),
                CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
        #endregion AwaitResOrErr
    }

}
