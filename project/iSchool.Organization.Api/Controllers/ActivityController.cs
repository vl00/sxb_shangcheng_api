using iSchool.Api.Swagger;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Api.Conventions;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Appliaction;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 活动
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        IMediator _mediator;

        public ActivityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// (新)活动信息
        /// </summary>
        /// <param name="acd">活动码.可包含推广号</param>
        /// <returns></returns>
        [HttpGet("info")]
        [Authorize, CheckBindMobile]
        [ProducesResponseType(typeof(SlmActivityInfoDto), 200)]
        public async Task<ResponseResult> Info([ApiDocParameter(Required = true)] string acd)
        {
            var a = await _mediator.Send(new SlmActivityInfoQuery { Code = acd });
            return ResponseResult.Success(a);
        }

        /// <summary>
        /// 分销活动微信公众号二维码
        /// </summary>
        /// <returns></returns>
        [HttpGet("HdDrpFxQrcode")]
        [Authorize]
        [ProducesResponseType(typeof(HdDrpFxQrcodeQryResult), 200)]
        public async Task<ResponseResult> GetHdDrpFxQrcode()
        {
            var a = await _mediator.Send(new HdDrpFxQrcodeQuery { });
            return ResponseResult.Success(a);
        }

        /// <summary>
        /// (后端用)分销活动回复微信公众号图片
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="picIndex"></param>
        /// <returns></returns>
        [HttpPost(nameof(HdDrpFxReplyGzhPic))]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> HdDrpFxReplyGzhPic([FromForm] string openId, [FromQuery] int picIndex)
        {
            var r = await _mediator.Send(new HdDrpFxReplyGzhPicCommand { OpenId = openId, PicIndex = picIndex });
            return ResponseResult.Success(r);
        }
    }
}
