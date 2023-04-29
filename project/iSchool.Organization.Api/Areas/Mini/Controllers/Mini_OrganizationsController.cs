using iSchool.Api.ModelBinders;
using iSchool.Api.Swagger;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Api.Conventions;
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
    /// 小程序-机构
    /// </summary>
    [Area("mini")]
    [Route("api/[area]/Organizations")]
    [ApiController]
    public class Mini_OrganizationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public Mini_OrganizationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 品牌机构
        /// </summary>
        /// <returns></returns>
        [HttpGet("brand")]
        [ProducesResponseType(typeof(List<MiniOrganizationItemDto>), 200)]
        public async Task<ResponseResult> OrganizationBrand()
        {
            var res = await _mediator.Send(new OrganizationBrandQuery { });
            return ResponseResult.Success(res);
        }


        /// <summary>
        /// get 品牌s的商品数s
        /// </summary>
        /// <returns></returns>
        [HttpPost("goodscounts")]
        [ProducesResponseType(typeof(MiniGetOrgsGoodsCountsQryResult), 200)]
        public async Task<ResponseResult> GetGoodsCounts([FromBody] Guid[] orgIds)
        {
            var res = await _mediator.Send(new MiniGetOrgsGoodsCountsQuery { OrgIds = orgIds });
            return ResponseResult.Success(res);
        }

    }
}
