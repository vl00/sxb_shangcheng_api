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
    /// pc机构
    /// </summary>
    [Area("PC")]
    [Route("api/[area]/Organizations")]
    [ApiController]
    public class PC_OrganizationsController : ControllerBase
    {
        IMediator _mediator;

        public PC_OrganizationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 机构列表+分页
        /// </summary>
        /// <param name="type">品牌类型</param>
        /// <param name="authentication">品牌认证(展示所有认证的品牌)</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        [HttpGet("ls")]
        [ProducesResponseType(typeof(PcOrgIndexQueryResult), 200)]
        public async Task<ResponseResult> Index(int? type = null, [BindBoolean] bool? authentication = null, int pageIndex = 1, int pageSize = 20)
        {
            var r = await _mediator.Send(new PcOrgIndexQuery
            {
                Type = type,
                Authentication = authentication,
                PageIndex = pageIndex,
                PageSize = pageSize,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// pc机构详情
        /// </summary>
        /// <param name="id" type="string">机构短id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PcOrgDetailDto), 200)]
        public async Task<ResponseResult> GetOrganization(
            [CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path, Required = true)]
            long id)
        {
            var r = await _mediator.Send(new PcOrgDetailQuery { No = id });
            return ResponseResult.Success(r);
        }

    }
}
