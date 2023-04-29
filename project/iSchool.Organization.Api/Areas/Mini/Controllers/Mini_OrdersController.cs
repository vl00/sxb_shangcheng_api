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
using iSchool.Organization.Domain;
using iSchool.Organization.Appliaction.Queries;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 订单管理
    /// </summary>
    [Authorize]
    [Area("mini")]
    [Route("/api/orders/v3")]
    [ApiController]
    public class Mini_OrdersController : Controller
    {
        private readonly IMediator _mediator;
        public Mini_OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 我的订单列表, 全部不会显示退款单, 退款单是另外接口
        /// </summary>
        /// <param name="me"></param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="status">
        /// 订单状态<br/>
        /// 不传|0 = 全部 <br/>
        /// </param>
        /// <returns></returns>
        [HttpGet("ls/my")]
        [ProducesResponseType(typeof(MiniOrderPglistQryResult), 200)]
        public async Task<ResponseResult> ListMyOrders([FromServices] IUserInfo me, int pageIndex = 1, int pageSize = 10, int status = 0)
        {
            var r = await _mediator.Send(new MiniOrderPglistQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Status = status,
                UserId = me.IsAuthenticated ? me.UserId : (Guid?)null,
            });
            return ResponseResult.Success(r);
        }
        /// <summary>
        /// 积分兑换订单列表
        /// </summary>
        /// <param name="me"></param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">页大小</param>
        /// 订单状态<br/>
        /// 不传|0 = 全部 <br/>
        /// </param>
        /// <returns></returns>
        [HttpGet("ls/points/my")]
        [ProducesResponseType(typeof(MiniOrderPglistQryResult), 200)]
        public async Task<ResponseResult> ListMyPointsExchangeOrders([FromServices] IUserInfo me, int pageIndex = 1, int pageSize = 10)
        {
            var r = await _mediator.Send(new MiniPointsExchangeOrderPglistQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                UserId = me.IsAuthenticated ? me.UserId : (Guid?)null,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="me"></param>
        /// <param name="id">订单id</param>

        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(MiniAdvanceOrderDetailQryResult), 200)]
        public async Task<ResponseResult> GetOrderDetail([FromServices] IUserInfo me, string id)
        {
            if (Guid.TryParse(id, out var guid))
            {
                var rr = await _mediator.Send(new MiniAdvanceOrderDetailQuery { UserId = me.UserId, OrderId = guid });
                return ResponseResult.Success(rr);
            }

            var r = await _mediator.Send(new MiniAdvanceOrderDetailQuery { UserId = me.UserId, AdvanceOrderNo = id });
            return ResponseResult.Success(r);

        }

        /// <summary>
        /// 我的退款单列表
        /// </summary>
        /// <param name="me"></param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">页大小</param>        
        /// <returns></returns>
        [HttpGet("ls/my/refund")]
        [ProducesResponseType(typeof(MiniOrderPglistQryResult), 200)]
        public async Task<ResponseResult> ListMyOrders_Refund([FromServices] IUserInfo me, int pageIndex = 1, int pageSize = 10)
        {
            var r = await _mediator.Send(new MiniOrderRefundPglistQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                //Status = (int)OrderStatusV2.RefundOk,
                UserId = me.IsAuthenticated ? me.UserId : (Guid?)null,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 获取订单退款数
        /// </summary>
        /// <returns></returns>
        [HttpGet("resfundcount")]
        [ProducesResponseType(typeof(List<OrderRefudCountDto>), 200)]
        public async Task<ResponseResult> OrderRefundCount([FromQuery] Guid orderid)
        {
            //差只能查询自己的订单（候补）

            //补充退款中数量   已退款数量
            var resfunds = await _mediator.Send(new OrderOrderRefundsByOrderIdsQuery
            {
                OrderIds = new Guid[] { orderid }
            });
            var groupData = resfunds.GroupBy(p => new { OrderDetailId = p.OrderDetailId, GoodId = p.ProductId });

            if (groupData.Count() == 0)
            {
                return ResponseResult.Success(new List<OrderRefudCountDto>());
            }
            else
            {
                var res = new List<OrderRefudCountDto>();
                foreach (var item in groupData)
                {
                    var detailRefunds = resfunds.Where(p => p.OrderDetailId == item.Key.OrderDetailId);
                    if (detailRefunds.Count() == 0)
                        continue;
                    //已退款
                    var refundc = detailRefunds.Where(p =>
                     p.Type == (int)RefundTypeEnum.FastRefund
                     || p.Type == (int)RefundTypeEnum.BgRefund
                     || (p.Type == (int)RefundTypeEnum.Refund && p.Status == (int)RefundStatusEnum.RefundSuccess)
                     || (p.Type == (int)RefundTypeEnum.Return && p.Status == (int)RefundStatusEnum.ReturnSuccess));

                    //退款中
                    //--查询审核失败数量
                    var FailedCount = detailRefunds.Where(p =>
                    p.Status == (int)RefundStatusEnum.InspectionFailed
                      || p.Status == (int)RefundStatusEnum.RefundAuditFailed
                      || p.Status == (int)RefundStatusEnum.ReturnAuditFailed
                      || p.Status == (int)RefundStatusEnum.Cancel
                      || p.Status == (int)RefundStatusEnum.CancelByExpired).Sum(p => p.Count);

                    res.Add(new OrderRefudCountDto
                    {
                        OrderId = orderid,
                        OrderDetailId = item.Key.OrderDetailId,
                        GoodId = item.Key.GoodId,
                        RefundedCount = refundc.Sum(p => p.Count),
                        RefundingCount = detailRefunds.Sum(p => p.Count) - FailedCount - refundc.Sum(p => p.Count)
                    }
                    );
                }
                return ResponseResult.Success(res);
            }
        }


        #region 功能已更新,请升级小程序

        /// <summary>
        /// [1.9-] 购买课程（微信小程序）-- 直接下单
        /// </summary>
        /// <param name="me"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [Authorize, CheckBindMobile]
        [Obsolete, HttpPost("course/wx/create")]
        [ProducesResponseType(typeof(CourseWxCreateOrderCmdResult), 200)]
        public async Task<object> CreateOrderOnWx([FromServices] IUserInfo me, CourseWxCreateOrderCommand cmd)
        {
            //if (me.IsAuthenticated) cmd.UserId = me.UserId;
            //cmd.Ver = "v3";
            //var r = await _mediator.Send(cmd, new CancellationTokenSource(/*1000 * 120*/).Token);
            //return Res2Result.Success(r);

#if DEBUG
            return Res2Result.Fail("接口改用为/api/orders/v4/wx/create3");
#else
            return Res2Result.Fail("功能已更新,请升级小程序", Consts.Err.AppIsUpdated);
#endif
        }

        #endregion 功能已更新,请升级小程序

        /// <summary>
        ///  修改收货地址
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="me">当前用户</param>
        /// <returns></returns>
        [HttpPost("uprecvaddress")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> UpRecvAddress([FromServices] IUserInfo me, MiniOrderUpdateAddressCmd cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 确定收货
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("doshipped")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> DoShipped(MiniOrderShippedCmd cmd)
        {
            cmd.IsFromAuto = false;
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }


        /// <summary>
        /// 用于订单兑换码页面
        /// </summary>
        /// <param name="id">订单id</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("redeemdesc")]
        [ProducesResponseType(typeof(GetOrderRedeemDescQryResult), 200)]
        public async Task<ResponseResult> GetOrderRedeemDesc(Guid id)
        {
            var r = await _mediator.Send(new GetOrderRedeemDescQuery { OrderStr = id.ToString() });
            return ResponseResult.Success(r);
        }

        #region 下单

        /// <summary>
        /// 订单重新支付
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("repay")]
        [ProducesResponseType(typeof(CourseWxCreateOrderCmdResult_v4), 200)]
        public async Task<Res2Result<CourseWxCreateOrderCmdResult_v4>> OrderRepay(MiniOrderRepayCmd cmd)
        {
            var r = await _mediator.Send(cmd);
            return r;
        }

        /// <summary>
        /// 取消待支付的订单
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("cancel")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> CancelOrder(MiniOrderCancelCmd cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// [mp1.6*] 多课程商品结算info
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="v">版本号,必传,固定为`mp1.6`</param>
        /// <param name="goods">商品s</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/orders/v4/course/goods/settle/multi")]
        [ProducesResponseType(typeof(CourseMultiGoodsSettleInfosQryResult), 200)]
        public async Task<Res2Result> GetCourseMultiGoodsSettleInfos_v4(
            [FromServices] IUserInfo userInfo
            ,[FromQuery] string v
            , CourseMultiGoodsSettleInfos_Sku[] goods)
        {
            if (Check_mpver(v) is Res2Result r0 && r0.Status == Consts.Err.AppIsUpdated)
            {
                return r0;
            }
            var query = new CourseMultiGoodsSettleInfosQuery
            {
                Goods = goods,
                UseQrcode = true,
                AllowNotValid = true,
                UserId = userInfo.UserId
            };
            var r = await _mediator.Send(query);
            return Res2Result.Success(r);
        }



        /// <summary>
        /// [mp1.6*] 购买课程（微信小程序）-- 购物车下单
        /// </summary>
        /// <param name="v">版本号,必传,固定为`mp1.6`</param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [Authorize, CheckBindMobile]
        [HttpPost("/api/orders/v4/course/wx/create")]
        [ProducesResponseType(typeof(CourseWxCreateOrderCmdResult_v4), 200)]
        public async Task<Res2Result> CreateOrderOnWx_v4([FromQuery] string v, CourseWxCreateOrderCmd_v4 cmd)
        {
            if (Check_mpver(v) is Res2Result r0)
            {
                return r0;
            }
            cmd.Ver = "v4";
            var r = await _mediator.Send(cmd, new CancellationTokenSource(/*1000 * 120*/).Token);
            return r;
        }

        /// <summary>
        /// 购买课程（微信小程序）-- 直接下单
        /// </summary>
        /// <param name="v">版本号,必传,固定为`mp2.2`</param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [Authorize, CheckBindMobile]
        [HttpPost("/api/orders/v4/wx/create3")]
        [ProducesResponseType(typeof(CourseWxCreateOrderCmdResult_v4), 200)]
        public async Task<Res2Result> CreateOrderOnWx_v4_on3([FromQuery] string v, CourseWxCreateOrderCmd_v4 cmd)
        {
            if (Check_mpver(v) is Res2Result r0)
            {
                return r0;
            }
            cmd.Ver = "v3";
            var r = await _mediator.Send(cmd, new CancellationTokenSource(/*1000 * 120*/).Token);
            return r;
        }

        internal static Res2Result Check_mpver(string v)
        {
            v = (v ?? "").Trim();
            var mpver = v.IsNullOrEmpty() ? 0 : double.TryParse(v.Replace("mp", ""), out var _iv) ? _iv : -1;
            switch (mpver)
            {
#if DEBUG
                case double _ when !mpver.In(1.6, 1.7):
                    return Res2Result.Fail("功能已更新,请升级小程序", Consts.Err.AppIsUpdated);
#else
                case double _ when mpver == 1.6:
                    return Res2Result.Fail("系统正在维护中！");

                case double _ when !mpver.In(1.7):
                    return Res2Result.Fail("功能已更新,请升级小程序", Consts.Err.AppIsUpdated);
#endif
            }
            return null;
        }

#endregion 下单


        /// <summary>
        /// [mp1.6*] 结算时根据地址省市区查找商品的运费
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize, CheckBindMobile]
        [HttpPost("/api/orders/v4/GetFreights")]
        [ProducesResponseType(typeof(GetFreightsByRecvAddressQryResult), 200)]
        public async Task<Res2Result> GetFreightsByRecvAddress(GetFreightsByRecvAddressQuery query)
        {
            var r = await _mediator.Send(query);
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 查询多物流的详情
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("OrderMultipleKuaidis")]
        [ProducesResponseType(typeof((IEnumerable<OrderItemDto> OrderItems, string Qrcode)), 200)]
        public async Task<ResponseResult> OrderMultipleKuaidis([FromQuery] GetOrderMultipleKuaidisQuery query)
        {
            var res = await _mediator.Send(query);
            return ResponseResult.Success(res);
        }



        /// <summary>
        /// 查询多物流快递详情
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("Logistics")]
        [ProducesResponseType(typeof(KuaidiDetailDto), 200)]
        public async Task<ResponseResult> Logistics([FromQuery] GetOrderLogisticsDetailQuery query)
        {
            var res = await _mediator.Send(query);
            return ResponseResult.Success(res);
        }
    }
}
