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
    /// 检查当前登录用户是否绑定手机号
    /// </summary>
    public class CheckBindMobileAttribute : ActionFilterAttribute, IAsyncActionFilter
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var HttpContext = context.HttpContext;
            var services = context.HttpContext.RequestServices;
            var mediator = services.GetService<IMediator>();
            var config = services.GetService<IConfiguration>();

            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                var r = ResponseResult.Failed("未登录");
                r.status = Domain.Enum.ResponseCode.NoLogin;                
                context.Result = new JsonResult(r);
                return;
            }

            var b = await mediator.Send(new CheckUserBindMobileCommand());
            if (!b)
            {
                var r = ResponseResult.Failed("未绑定手机号");
                r.status = Domain.Enum.ResponseCode.NotBindMobile;
                r.Data = $"{config["AppSettings:UserCenterBaseUrl"]}/login/login-bind.html";
                context.Result = new JsonResult(r);
                return;
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
                //var r = ResponseResult.Failed("");
                //context.Result = new JsonResult(r);
                //return;
            }
            context.Result = ar.Result;
        }
    }
}
