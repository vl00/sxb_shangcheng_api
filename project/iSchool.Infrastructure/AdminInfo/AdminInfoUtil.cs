using iSchool.Authorization;
using iSchool.Authorization.Models;
using iSchool.Domain.Modles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iSchool.Infrastructure
{
    public static partial class AdminInfoUtil
    {
        const int platformID = 1;

        /// <summary>
        /// 简单获取当前用户id
        /// </summary>
        public static Guid GetUserId(this HttpContext context)
        {
#if DEBUG
            return Guid.Parse(context?.User?.Identity.Name ?? "4CEE6197-801A-B244-B7A3-32FA3A45E3AC");
#else
            return Guid.Parse(context?.User?.Identity.Name ?? throw new FnResultException(401, "未登录 可能访问[AllowAnonymous],同时尝试获取当前用户"));
#endif
        }

        /// <summary>
        /// get当前用户info
        /// </summary>
        public static AdminInfo GetUserInfo(this HttpContext context)
        {
            if (context.Items["__uadminifo__"] is AdminInfo info)
                return info;

            try
            {
                context.Items["__uadminifo__"] = info = new Account().Info(context);
                return info;
            }
            catch
            {
                return null;
            }
        }

        //// get当前用户在当前系统中的all Functions
        //static IEnumerable<Function> GetUserPlatAllFunctions(this HttpContext context)
        //{
        //    var _key = $"__{nameof(GetUserPlatAllFunctions)}__";

        //    if (context.Items[_key] is IEnumerable<Function> fns)
        //        return fns;

        //    var userInfo = GetUserInfo(context);
        //    if (userInfo?.Character == null) return Enumerable.Empty<Function>();

        //    context.Items[_key] = fns = userInfo.Character.SelectMany(_ => _.Function).Where(_ => _?.PlatformId == platformID);

        //    return fns;
        //}

        //public static IEnumerable<Function> GetUserCtrlActFunctions(this HttpContext context, string controller, string action)
        //{
        //    return GetUserPlatAllFunctions(context)
        //        .Where(_ => string.Equals(_.Controller, controller, StringComparison.OrdinalIgnoreCase)
        //            && (action == "*"
        //                || string.Equals(_.Action, action, StringComparison.OrdinalIgnoreCase)
        //                || _.Action.IndexOf($"{{{action}}}", StringComparison.OrdinalIgnoreCase) > -1)
        //        );
        //}

        //public static IEnumerable<Function> GetUserCurrFunctions(this HttpContext context)
        //{
        //    var routeData = context.GetRouteData();
        //    if (routeData == null) return Enumerable.Empty<Function>();

        //    var controller = routeData.Values["controller"].ToString();
        //    var action = routeData.Values["action"].ToString();

        //    return GetUserCtrlActFunctions(context, controller, action);
        //}

        public static IEnumerable<string> GetUserCurrFnQueries(this HttpContext context)
        {
            var routeData = context.GetRouteData();
            if (routeData == null) return Enumerable.Empty<string>();

            var controller = routeData.Values["controller"].ToString();
            var action = routeData.Values["action"].ToString();

            return GetUserCtrlActFnQueries(context, controller, action);
        }

        public static IEnumerable<string> GetUserCtrlActFnQueries(this HttpContext context, string controller, string action)
        {
            var k = $"user-ca-qx/{controller.ToLower()}/{action.ToLower()}";
            if (context.Items[k] is IEnumerable<string> qx)
                return qx;

            qx = !new Permission().Check(platformID, controller, action, context.GetUserId(), out string qs)
                ? Enumerable.Empty<string>()
                : qs.Split(',', ';').Select(_ => _.Trim());

            context.Items[k] = qx;

            return qx;
        }

        public static IEnumerable<string> GetQueries(this Function fn)
        {
            if (fn?.Query == null) return Enumerable.Empty<string>();
            return fn.Query.Select(a => a.Selector.Trim());
        }

        public static IEnumerable<string> GetQueries(this IEnumerable<Function> fns)
        {
            if (fns == null) return Enumerable.Empty<string>();
            return fns.SelectMany(_ => _.GetQueries());
        }

        /// <summary>
        /// context.Items["PageQuery"]?.ToString()
        /// </summary>
        public static string GetCurrQyxStr(this HttpContext context)
        {
            return context.Items["PageQuery"]?.ToString();
        }

        /// <summary>
        /// 检测是否有某些权限
        /// </summary>
        public static bool HasQyx(this IEnumerable<string> queries, params string[] qx)
        {
#if DEBUG
            return true;
#else
            foreach (var q in qx)
            {
                if (!queries.Contains(q))
                    return false;
            }
            return queries.Any();
#endif
        }

        /// <summary>
        /// 检测当前用户在当前/[controller]/[action]是否有某些权限
        /// </summary>
        public static bool HasCurrQyx(this HttpContext context, params string[] qx)
        {
            if (!context.RequestServices.GetService<IOptions<AppSettings>>().Value.IsQxFilterOpened) return true;
            return context.GetUserCurrFnQueries().HasQyx(qx);
        }

        /// <summary>
        /// 检测当前用户在某个/[controller]/[action]是否有某些权限
        /// </summary>
        public static bool HasCtrlActQyx(this HttpContext context, string controller, string action, params string[] qx)
        {
            if (!context.RequestServices.GetService<IOptions<AppSettings>>().Value.IsQxFilterOpened) return true;
            return context.GetUserCtrlActFnQueries(controller, action).HasQyx(qx);
        }

        /// <summary>
        /// 检测当前用户在某个/[controller]/[action]是否有某些权限
        /// </summary>
        public static bool HasCtrlActQyx(this HttpContext context, (string, string) ca, params string[] qx)
        {
            return context.HasCtrlActQyx(ca.Item1, ca.Item2, qx);
        }
    }

    public static partial class AdminInfoUtil
    {
        /// <summary>
        /// 根据用户id获取用户名
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static Dictionary<Guid, string> GetNames(IEnumerable<Guid> userIds)
        {
            return new Account().GetAdmins(new List<Guid>(userIds.Distinct())).ToDictionary(_ => _.Id, _ => _.Displayname);
        }

        /// <summary>
        /// 根据用户ids获取用户信息(名称,账号等)
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static Dictionary<Guid, AdminInfo> GetUsers(IEnumerable<Guid> userIds)
        {
            return new Account().GetAdmins(new List<Guid>(userIds.Distinct())).ToDictionary(_ => _.Id, _ => _);
        }

        public static IEnumerable<Character> GetUserRoles(this HttpContext context)
        {
            return context.GetUserInfo().Character;
        }
    }
}
