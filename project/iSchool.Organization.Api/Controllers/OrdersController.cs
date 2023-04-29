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
using iSchool.Organization.Appliaction.OrgService_bg.Order;
using Microsoft.Extensions.Configuration;
using iSchool.Organization.Appliaction.ResponseModels.Orders;
using iSchool.Organization.Appliaction.Service.Orders;
using iSchool.Organization.Domain;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 订单管理
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : Controller
    {
        IMediator _mediator;
        private readonly IConfiguration config;
        public OrdersController(IMediator mediator, IConfiguration _config)
        {
            config = _config;
            _mediator = mediator;
        }

        /// <summary>
        /// 用户订单列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<OrdersByUserIdQueryResponse>), 200)]
        [HttpGet("{userId}")]
        public ResponseResult GetOrdersByUserId(int pageIndex, int pageSize, Guid userId)
        {
            var res = _mediator.Send(new OrdersByUserIdQuery()
            {
                //PageInfo = new PageInfo() { PageIndex=pageIndex, PageSize=pageSize },
                PageIndex = pageIndex,
                PageSize = pageSize,
                Userid = userId
            }).Result;

            return ResponseResult.Success(res);
        }

        /// <summary>
        /// 【课程分销后台专用】根据收货手机号，查询(收货用户信息及其小课订单信息)列表
        /// [测试数据 mobile=13232720105 ]
        /// </summary>
        /// <param name="recvMobile">收货手机号</param>
        /// <param name="orderMobile">下单人手机号</param>
        /// <returns></returns>
        [AllowAnonymous]//暂时不需要登录
        [ProducesResponseType(typeof(List<OrdersByMobileQueryResponse>), 200)]
        [HttpGet("UsersInfo")]
        public ResponseResult GetOrdersByMobile(string recvMobile, string orderMobile)
        {
            var response = _mediator.Send(new OrdersByMobileQuery() { RecvMobile = recvMobile, OrderMobile = orderMobile }).Result;
            return ResponseResult.Success(response);
        }

        ///// <summary>
        ///// [1.4-] 获取课程基本信息
        ///// </summary>
        ///// <param name="id">课程id(长短都可以)</param>
        ///// <returns></returns>
        //[HttpGet("course/info/{id}")]
        //[ProducesResponseType(typeof(CourseOrderProdInfoQueryResult), 200)]
        //[Obsolete]
        //public async Task<ResponseResult> GetCourseOrderInfo(string id)
        //{
        //    //var no = Guid.TryParse(id, out var gid) ? 0L : UrlShortIdUtil.Base322Long(id);
        //    //var r = await _mediator.Send(new CourseOrderProdInfoQuery() { CourseId = gid, CourseNo = no });
        //    //return ResponseResult.Success(r);
        //    throw new NotSupportedException();
        //}

        /// <summary>
        /// 购买课程选择套餐
        /// </summary>
        /// <param name="id">课程id(长短都可以)</param>
        /// <param name="isFromPoints">是否来自积分兑换</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("course/props")]
        [ProducesResponseType(typeof(CourseGoodsPropsDto), 200)]
        public async Task<ResResult> GetCourseGoodsMealpropsInfo([ApiDocParameter(Required = true)] string id,bool isFromPoints= false)
        {
            var no = Guid.TryParse(id, out var gid) ? 0L : UrlShortIdUtil.Base322Long(id);
            var r = await _mediator.Send(new CourseGoodsPropsQuery { CourseId = gid, CourseNo = no , IsFromPoints =isFromPoints });
            return ResResult.Success(r);
        }

        /// <summary>
        /// 根据选择的课程套餐(属性)项,查询商品信息.
        /// </summary>
        /// <param name="query">课程属性项的id</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("course/goods/selected")]
        [ProducesResponseType(typeof(ApiCourseGoodsSimpleInfoDto), 200)]
        public async Task<ResResult> GetCourseGoodsInfo(CourseGoodsSimpleInfoByPropItemsQuery query)
        {
            var r = await _mediator.Send(query);
            return ResResult.Success(r);
        }

        /// <summary>
        /// 课程商品结算页查询商品信息
        /// </summary>
        /// <param name="id">商品id</param>
        /// <param name="buyAmount">购买数量</param>
        /// <returns></returns>
        [HttpGet("course/goods/settle")]
        [ProducesResponseType(typeof(CourseGoodsSettleInfoQryResult), 200)]
        public async Task<ResResult> GetCourseGoodsSettInfo([ApiDocParameter(Required = true)] Guid id,
            [ApiDocParameter(Required = true)] int buyAmount = 1)
        {
            var r = await _mediator.Send(new CourseGoodsSettleInfoQuery { Id = id, BuyCount = buyAmount });
            return ResResult.Success(r);
        }

        /// <summary>
        /// 购买课程（微信）-- h5下单
        /// </summary>
        /// <param name="me"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [Authorize, CheckBindMobile]
        [Obsolete, HttpPost("course/wx/create")]
        [ProducesResponseType(typeof(CourseWxCreateOrderCmdResult), 200)]
        public async Task<ResponseResult> CreateOrderOnWx([FromServices] IUserInfo me, CourseWxCreateOrderCommand cmd)
        {
            //if (me.IsAuthenticated) cmd.UserId = me.UserId;
            //var r = await _mediator.Send(cmd, new CancellationTokenSource(/*1000 * 5*/).Token);
            //return ResponseResult.Success(r);

            return ResponseResult.Failed("请到小程序里进行购买", Consts.Err.AppIsUpdated);
        }


        /// <summary>
        /// 我的订单列表
        /// </summary>
        /// <param name="me"></param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="status">
        /// 订单状态<br/>
        /// 不传|0 = 全部 <br/>
        /// </param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("v2/ls/my")]
        [ProducesResponseType(typeof(OrderPglistQueryResult), 200)]
        public async Task<ResponseResult> ListMyOrders([FromServices] IUserInfo me, int pageIndex = 1, int pageSize = 10, int status = 0)
        {
            var r = await _mediator.Send(new OrderPglistQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Status = status,
                UserId = me.IsAuthenticated ? me.UserId : (Guid?)null,
            });
            return ResponseResult.Success(r);
        }
        [Authorize]
        [HttpPost("v2/statement/detail")]
        [ProducesResponseType(typeof(StatementDetailResponseDto), 200)]
        public async Task<ResponseResult> GetStatementDetail([FromServices] IUserInfo me, OrgRelStatementDetailCommand cmd)
        {
            if(me.UserId!=cmd.UserId) return ResponseResult.Failed("非法操作");
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }
        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="me"></param>
        /// <param name="id">订单id</param>
        [Authorize]
        [HttpGet("v2/detail/{id}")]
        [ProducesResponseType(typeof(OrderDetailQueryResult), 200)]
        public async Task<ResponseResult> GetOrderDetail_v2([FromServices] IUserInfo me, Guid id)
        {
            var r = await _mediator.Send(new OrderDetailQuery { OrderId = id, UserId = me.UserId });
            return ResponseResult.Success(r);
        }

#if DEBUG
        /// <summary>
        /// (后端专用)支付平台回调微信回调
        /// </summary>
        /// <param name="notify"></param>
        /// <returns></returns>
        [HttpPost("wxpay/callback")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ResponseResult> WxPayCallback(WxPayCallbackNotifyMessage notify)
        {
            var r = await _mediator.Send(new WxPayRequest { WxPayCallback = notify });
            return ResponseResult.Success(r);
        }
#endif

        [AllowAnonymous]
        [HttpPost("order/handlerefund")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ResponseResult> HandleRefund(string orderNo)
        {
            var d = await _mediator.Send(new MiniOrderDetailQuery { OrderNo = orderNo });

            var request = new RefundCommand { OrdId = d.OrderId };
            request.RefundApiUrl = $"{config["AppSettings:wxpay:baseUrl"]}/api/PayOrder/Refund";
            var paykey = config["AppSettings:wxpay:paykey"];
            var system = config["AppSettings:wxpay:system"];
            request.UserId = Guid.Parse("88888888-8888-8888-8888-888888888888");
            request.PayKey = paykey;
            request.System = system;
            request.AdvanceOrderId = d.AdvanceOrderId;
            var r = await _mediator.Send(request);
            return ResponseResult.Success(r);
        }


        /// <summary>
        /// 查询用户购买rw活动商品的情况
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("rw/ls/user/storestatus")]
        [ProducesResponseType(typeof(RwInviteActivityUserOrderStoreStatusLsItem[]), 200)]
        public async Task<ResponseResult> WxPayCallback(RwInviteActivityUserOrderStoreStatusLsQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }
    }
}
