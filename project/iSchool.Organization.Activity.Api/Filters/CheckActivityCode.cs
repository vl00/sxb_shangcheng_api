using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Filters
{
    /// <summary>
    /// 检查活动(推广)码
    /// </summary>
    public class CheckActivityCodeAttribute : ActionFilterAttribute, IAsyncActionFilter
    {
        readonly string fd_promocode;

        public CheckActivityCodeAttribute(string fd_promocode)
        {
            this.fd_promocode = fd_promocode;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var HttpContext = context.HttpContext;
            var services = context.HttpContext.RequestServices;
            //var mediator = services.GetService<IMediator>();
            //var config = services.GetService<IConfiguration>();
            var activityHelper = services.GetService<IActivityHelper>();

            string originCode = null;
            if (!string.IsNullOrEmpty(fd_promocode))
            {
                originCode = context.RouteData.Values[fd_promocode]?.ToString();
            }
            if (!string.IsNullOrEmpty(fd_promocode))
            {
                originCode = HttpContext.Request.Query[fd_promocode];
            }

            if (!activityHelper.TryGetInfo(originCode, out var activityInfo))
            {
                throw new CustomResponseException("推广码错误");
            }

            HttpContext.Items["activity-info"] = activityInfo;
            if (context.ActionArguments.ContainsKey("activityInfo"))
            {
                context.ActionArguments["activityInfo"] = activityInfo;
            }

            var ar = await next();
            if (ar.ExceptionDispatchInfo != null)
            {
                ar.ExceptionDispatchInfo.Throw();
                return;
            }
            if (ar.Exception != null)
            {
                throw ar.Exception;
            }
            context.Result = ar.Result;
        }
    }
}
