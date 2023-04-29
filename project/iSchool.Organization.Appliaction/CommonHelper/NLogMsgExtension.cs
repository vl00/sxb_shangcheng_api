using iSchool.Infrastructure;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    public static class NLogMsgExtension
    {
        /**
             log.Info(msg);
             log.Log(LogLevel.Info, msg);
         */

        [DebuggerStepThrough]
        public static NLog.LogEventInfo GetNLogMsg(this NLog.ILogger log, string desc, IServiceProvider services = null)
        {
            return GetNLogMsg(services, desc);
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo GetNLogMsg(this IServiceProvider services, string desc)
        {
            var msg = new NLog.LogEventInfo();
            //msg.Properties["BusinessId"] = Guid.NewGuid();
            msg.Properties["Time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            msg.Properties["ThreadId"] = Thread.CurrentThread.ManagedThreadId;
            msg.Properties["Caption"] = desc;
            if (services != null)
            {
                msg.Properties["UserId"] = services.GetService<IUserInfo>() is IUserInfo user && user.IsAuthenticated ? user.UserId.ToString() : null;
            }
            return msg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetLevel(this NLog.LogEventInfo logMsg, string level)
        {
            logMsg.Properties["Level"] = level;
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetUrl(this NLog.LogEventInfo logMsg, string url)
        {
            logMsg.Properties["Url"] = url;
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetTime(this NLog.LogEventInfo logMsg, DateTime time)
        {
            logMsg.Properties["Time"] = time.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetUserId(this NLog.LogEventInfo logMsg, Guid userId)
        {
            logMsg.Properties["UserId"] = userId;
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetParams(this NLog.LogEventInfo logMsg, object _params)
        {
            logMsg.Properties["Params"] = _params is string str ? str : _params?.ToJsonString(camelCase: true);
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetCaption(this NLog.LogEventInfo logMsg, string caption)
        {
            logMsg.Properties["Caption"] = caption;
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetClass(this NLog.LogEventInfo logMsg, string type)
        {
            logMsg.Properties["Class"] = type;
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetMethod(this NLog.LogEventInfo logMsg, string med)
        {
            logMsg.Properties["Method"] = med;
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetContent(this NLog.LogEventInfo logMsg, string content)
        {
            logMsg.Properties["Content"] = content;
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetError(this NLog.LogEventInfo logMsg, Exception ex, int errcode = 500)
        {
            logMsg.Properties["Error"] = ex?.Message;
            logMsg.Properties["StackTrace"] = ex?.StackTrace;
            logMsg.Properties["ErrorCode"] = ex == null ? 0 : errcode;
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetError(this NLog.LogEventInfo logMsg, string errmsg, int errcode = 500)
        {
            logMsg.Properties["Error"] = errmsg;
            //logMsg.Properties["StackTrace"] = null;
            logMsg.Properties["ErrorCode"] = errmsg == null ? 0 : errcode;
            return logMsg;
        }

        [DebuggerStepThrough]
        public static NLog.LogEventInfo SetError(this NLog.LogEventInfo logMsg, Exception ex, string errmsg, int errcode = 500)
        {
            logMsg.Properties["Error"] = $"{(string.IsNullOrEmpty(errmsg) ? "" : $"{errmsg}")}{(errmsg?.EndsWith(".") == false ? "." : "")}err={ex.Message}";
            logMsg.Properties["StackTrace"] = ex?.StackTrace;
            logMsg.Properties["ErrorCode"] = ex == null ? 0 : errcode;
            return logMsg;
        }
    }
}
