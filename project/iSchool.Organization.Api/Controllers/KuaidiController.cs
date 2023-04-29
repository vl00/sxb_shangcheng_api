using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using iSchool.Api.ModelBinders;
using iSchool.Api.Swagger;
using iSchool.Organization.Api.Conventions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Organization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 快递
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public partial class KuaidiController : Controller
    {
        IMediator _mediator;

        public KuaidiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 获取快递公司s和编码s
        /// </summary>
        /// <returns></returns>
        [HttpGet("companys")]
        [ProducesResponseType(typeof(KeyValuePair<string, string>[]), 200)]
        public async Task<ResponseResult> GetKuaidiCompanys()
        {
            var r = (await _mediator.Send(KuaidiServiceArgs.GetCompanyCodes())).GetResult<KeyValuePair<string, string>[]>();
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// [1.6+] 获取订单的物流信息
        /// </summary>
        /// <param name="orderId">订单id</param>
        /// <returns></returns>
        [HttpGet("order")]
        [ProducesResponseType(typeof(OrderKuaidiDetailDto), 200)]
        public async Task<ResponseResult> GetOrderKuaidiInfo([ApiDocParameter(Required = true)] Guid orderId)
        {
            var r = await _mediator.Send(new GetOrderKuaidiDetailQuery { OrderId = orderId });
            return ResponseResult.Success(r);
        }


        /// <summary>
        /// 获取多个快递
        /// </summary>
        /// <returns></returns>
        [HttpGet("multiple")]
        public ResponseResult GetOrderMultipleKuaidis()
        {

            return ResponseResult.Success();
        }

    }

    [Route("api/test/kuaidi")]
    [ApiController]
    public partial class Test_KuaidiController
    {
        IMediator _mediator;

        public Test_KuaidiController(IMediator mediator)
        {
            _mediator = mediator;
        }

#if DEBUG

        /// <summary>
        /// [DEBUG] 查询快递单号详情 by baidu api
        /// </summary>
        /// <param name="nu">快递运单号</param>
        /// <param name="com">快递公司编号.可选</param>
        /// <returns></returns>
        [HttpGet("details/v1")]
        [ProducesResponseType(typeof(KuaidiNuDataDto), 200)]
        public async Task<ResponseResult> GetKuaidiDetails([ApiDocParameter(Required = true)] string nu, string com = null)
        {
            var r = await _mediator.Send(new GetKuaidiDetailsByBaiduExprApiQuery { Nu = nu, Com = com });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// [DEBUG] 查询快递单号详情 by baidu api
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("details/v1")]
        [ProducesResponseType(typeof(KuaidiNuDataDto), 200)]
        public async Task<ResponseResult> PostGetKuaidiDetails(GetKuaidiDetailsByBaiduExprApiQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }


        /// <summary>
        /// [DEBUG] 查询快递单号详情v2 by 腾讯云-17972-全国物流快递查询
        /// </summary>
        /// <param name="nu">快递运单号</param>
        /// <param name="com">
        /// 快递公司编号.可选 <br/>
        /// 自动识别不能100%准确.一个单号可对应多个快递公司.
        /// </param>
        /// <param name="customer">
        /// 第三方接口要求的参数,可选. <br/>
        /// 例如: 当快递为SF时,需要收件人或寄件人手机号后四位
        /// </param>
        /// <returns></returns>
        [HttpGet("details/v2")]
        [ProducesResponseType(typeof(KuaidiNuDataDto), 200)]
        public async Task<ResponseResult> GetKuaidiDetails_v2([ApiDocParameter(Required = true)] string nu, string com = null, string customer = null)
        {
            if (!string.IsNullOrEmpty(customer)) customer = HttpUtility.UrlDecode(customer);
            var r = await _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery { Nu = nu, Com = com, Customer = customer });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// [DEBUG] 查询快递单号详情v2 by 腾讯云-17972-全国物流快递查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("details/v2")]
        [ProducesResponseType(typeof(KuaidiNuDataDto), 200)]
        public async Task<ResponseResult> PostGetKuaidiDetails_v2(GetKuaidiDetailsByTxc17972ApiQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// [DEBUG] 查询快递单号详情v3 by 腾讯云-20590-快递鸟
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("details/v3")]
        [ProducesResponseType(typeof(KuaidiNuDataDto), 200)]
        public async Task<ResponseResult> PostGetKuaidiDetails_v3(GetKuaidiDetailsByTxcKdniaoApiQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }

#endif
    }
}
