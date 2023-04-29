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

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 退款退货
    /// </summary>
    [Authorize]
    [Area("mini")]
    [Route("/api/refundg")]
    [ApiController]
    public class Mini_RefundingController : Controller
    {
        private readonly IMediator _mediator;

        public Mini_RefundingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 申请退款-进入流程页面获取数据
        /// </summary>
        /// <param name="odid">
        /// 订单详情OrderDetail id <br/>
        /// `$OrderProdItemDto.orderDetailId`
        /// </param>
        /// <returns></returns>
        [HttpGet("apply")]
        [ProducesResponseType(typeof(RefundApplyQryResult), 200)]
        public async Task<Res2Result> ApplyRefund(Guid odid /*, int rty = 0*/)
        {
            var r = await _mediator.Send(new RefundApplyQry { OrderDetailId = odid });
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 提交退款退货申请(网课不能退)
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("doapply")]
        [ProducesResponseType(typeof(RefundApplyCmdResult), 200)]
        public async Task<Res2Result> DoApplyRefund(RefundApplyCmd cmd)
        {
            var r = await _mediator.Send(cmd);
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 退款退货详情
        /// </summary>
        /// <param name="id">退款单id</param>
        /// <returns></returns>
        [HttpGet("detail")]
        [ProducesResponseType(typeof(RefundDetailDto), 200)]
        public async Task<Res2Result> RefundDetail(string id)
        {
            var r = await _mediator.Send(new GetRefundDetailQuery { Id = id });
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 用户取消退款退货申请
        /// </summary>
        [HttpPost("user/cancel")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<Res2Result> UserCancel(RefundCancelCmd cmd)
        {
            var r = await _mediator.Send(cmd);
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 退货申请通过后,用户提交退货物流
        /// </summary>
        [HttpPost("user/sendback/kdwl")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<Res2Result> UserSendbackKdwl(RefundUserSendbackKdwlCmd cmd)
        {
            var r = await _mediator.Send(cmd);
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 快递公司list
        /// </summary>
        [HttpGet("kdcoms")]
        [ProducesResponseType(typeof(KeyValuePair<string, string>[]), 200)]
        public async Task<Res2Result> Api_kdcoms()
        {
            await default(ValueTask);
            var r = (await _mediator.Send(KuaidiServiceArgs.GetCompanyCodes())).GetResult<KeyValuePair<string, string>[]>();
            //var r = Get_kdcoms();
            IEnumerable<KeyValuePair<string, string>> Get_kdcoms()
            {
                yield return KeyValuePair.Create("YTO", "圆通速递");
                yield return KeyValuePair.Create("ZTO", "中通快递");
                yield return KeyValuePair.Create("YD", "韵达速递");
                yield return KeyValuePair.Create("STO", "申通快递");
                yield return KeyValuePair.Create("SF", "顺丰速运");
                //yield return KeyValuePair.Create("BTWL", "百世快运");
                yield return KeyValuePair.Create("EMS", "EMS");
                yield return KeyValuePair.Create("HTKY", "百世快递");
                yield return KeyValuePair.Create("JD", "京东物流");
                yield return KeyValuePair.Create("UC", "优速快递");
            }
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 查询退款单物流快递详情
        /// </summary>
        /// <param name="config"></param>
        /// <param name="id">退款单id</param>
        [HttpGet("kd")]
        [ProducesResponseType(typeof(RefundBillKdDetailDto), 200)]
        public async Task<Res2Result> RefundBill_kd([FromServices] IConfiguration config, string id)
        {
            var r = await _mediator.Send(new GetRefundBillKdDetailQuery { Id = id });
            // 小助手二维码
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), config[$"AppSettings:org_assistant"]);
                var bys = await System.IO.File.ReadAllBytesAsync(path);
                r.HelperQrcodeUrl = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }
            return Res2Result.Success(r);
        }
    }
}
