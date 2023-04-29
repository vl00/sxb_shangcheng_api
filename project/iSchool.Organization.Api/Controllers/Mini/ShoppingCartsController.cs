using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.Service.Order;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iSchool.Organization.Domain.Security;
using iSchool.Organization.Api.Filters;
using iSchool.Infrastructure.Extensions;
using System.Threading;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Domain.Modles;
using iSchool.Api.Swagger;
using iSchool.Organization.Domain.Enum;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 购物车
    /// </summary>
    [Authorize]
    [Area("mini")]
    [Route("/api/ShoppingCarts")]
    [ApiController]
    public class ShoppingCartsController : Controller
    {
        private readonly IMediator _mediator;

        public ShoppingCartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 获取用户购物车(含合并临时)
        /// </summary>
        /// <param name="me"></param>
        /// <param name="temps">
        /// 需要合并的临时项s <br/>
        /// 没临时项可以`不传 或 传null 或 传空数组` 
        /// </param>
        /// <returns></returns>        
        [ProducesResponseType(typeof(CourseShoppingCartDto), 200)]
        [HttpPost("load")]
        public async Task<ResponseResult> GetUserCourseShoppingCart([FromServices] IUserInfo me, CourseShoppingCartItem[] temps = null)
        {
            var r = await _mediator.Send(new GetUserCourseShoppingCartQuery { UserId = me.UserId, Temps = temps });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 更新修改用户购物车
        /// </summary>
        /// <param name="me"></param>
        /// <param name="actions">操作s. 每种操作各填一种action</param>
        /// <returns></returns>        
        [ProducesResponseType(typeof(UpUserCourseShoppingCartCmdResult), 200)]
        [HttpPost("up")]
        public async Task<ResponseResult> UpUserCourseShoppingCart([FromServices] IUserInfo me, List<UpCourseShoppingCartCmdAction> actions)
        {
            var r = await _mediator.Send(new UpUserCourseShoppingCartCmd { UserId = me.UserId, Actions = actions });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 从course详情页里添加到购物车
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), 200)]
        [HttpPost("addfromCourse")]
        public async Task<ResponseResult> AddFromCourseDetail([FromServices] IUserInfo me, AddCourseToShoppingCartCmd cmd)
        {
            cmd.UserId = me.UserId;
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }
    }
}
