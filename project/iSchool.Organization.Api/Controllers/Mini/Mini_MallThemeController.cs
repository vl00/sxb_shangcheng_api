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
    /// 商城主题和专题
    /// </summary>
    [Area("mini")]
    [Route("/api/malltheme")]
    [ApiController]
    public class Mini_MallThemeController : Controller
    {
        private readonly IMediator _mediator;

        public Mini_MallThemeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 主题商城-主题列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(MallThemeLsQryResult), 200)]
        [HttpGet("ls")]        
        public async Task<Res2Result> GetLs([FromQuery] MallThemeLsQuery query)
        {
            var r = await _mediator.Send(query);
            return Res2Result.Success(r);
        }

        /// <summary>
        /// (小程序)主题商城-主页
        /// </summary>
        /// <param name="spid">专题短id</param>
        /// <param name="tid">主题短id</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(MpMallThemeDetailQryResult), 200)]
        [HttpGet("mp/detail")]
        public async Task<Res2Result> Mp_detail(string spid = null, string tid = null)
        {
            var r = await _mediator.Send(new MpMallThemeDetailQuery { Spid = spid, Tid = tid });
            return Res2Result.Success(r);
        }

        /// <summary>
        /// (pc)主题商城-主页
        /// </summary>
        /// <param name="tid">主题短id</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(PcMallThemeDetailQryResult), 200)]
        [HttpGet("pc/detail")]
        public async Task<Res2Result> Pc_detail(string tid = null)
        {
            var r = await _mediator.Send(new PcMallThemeDetailQuery { Tid = tid });
            return Res2Result.Success(r);
        }

        /// <summary>
        /// (pc)点击弹出商品详情小程序码
        /// </summary>
        /// <param name="id">商品spu短id</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(MallGetCourseQrcodeCmdResult), 200)]
        [HttpGet("mpqrcode")]
        public async Task<Res2Result> Pc_show_mpqrcode(string id)
        {
            var r = await _mediator.Send(new MallGetCourseQrcodeCmd { Id = id });
            return Res2Result.Success(r);
        }
    }
}
