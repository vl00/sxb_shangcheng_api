using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using System.IO;
using Microsoft.Extensions.Configuration;
using iSchool.Organization.Domain;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 新版订单支付
    /// </summary>
    [Authorize]
    [Area("mp")]
    [Route("/api/mp/orders")]
    [ApiController]
    public class Mp_OrdersController : Controller
    {
        private readonly IMediator _mediator;

        public Mp_OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 此接口决定wx支付流程方式(是原支付还是新的跳转支付)
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("wxpfty")]
        [ProducesResponseType(typeof(GetWxPayFlowTypeQryResult), 200)]
        public async Task<Res2Result> GetWxPayFlowType()
        {
            var r = await _mediator.Send(new GetWxPayFlowTypeQuery { });
            return Res2Result.Success(r);
        }

        #region 下单
        //
        // Mini_OrdersController.cs :
        //      /api/orders/v4/course/wx/create
        //      /api/orders/v4/wx/create3
        //

        /// <summary>
        /// 购买课程（微信小程序）-- 购物车下单
        /// </summary>
        /// <param name="v"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [Authorize, CheckBindMobile]
        [HttpPost("wx/create4")]
        [ProducesResponseType(typeof(CourseWxCreateOrderCmdResult_v4), 200)]
        public async Task<Res2Result> CreateOrderOnWx_v4(CourseWxCreateOrderCmd_v4 cmd, [FromQuery] string v = null)
        {
            cmd.Ver = "v4";
            cmd.IsOnlyCreateOrder = true;
            var r = await _mediator.Send(cmd, new CancellationTokenSource(/*1000 * 120*/).Token);
            return r;
        }

        /// <summary>
        /// 购买课程（微信小程序）-- 直接下单
        /// </summary>
        /// <param name="v"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [Authorize, CheckBindMobile]
        [HttpPost("wx/create3")]
        [ProducesResponseType(typeof(CourseWxCreateOrderCmdResult_v4), 200)]
        public async Task<Res2Result> CreateOrderOnWx_v4_on3(CourseWxCreateOrderCmd_v4 cmd, [FromQuery] string v = null)
        {
            cmd.Ver = "v3";
            cmd.IsOnlyCreateOrder = true;
            var r = await _mediator.Send(cmd, new CancellationTokenSource(/*1000 * 120*/).Token);
            return r;
        }

        /// <summary>
        /// (新小程序)调用wx预支付接口
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [AllowAnonymous] //[Authorize, CheckBindMobile]
        [HttpPost("wx/pullpreaddorder")]
        [ProducesResponseType(typeof(CourseWxCreateOrderCmdResult_v4), 200)]
        public async Task<Res2Result> PullWxPreAddPayOrder(MiniOrderRepayCmd cmd)
        {
            cmd.IsNewMpPay = true;
            var r = await _mediator.Send(cmd);
            return r;
        }

        #endregion 下单


    }
}
