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
    /// pc专题
    /// </summary>
    [Area("PC")]
    [Route("api/[area]/Specials")]
    [ApiController]
    public class PC_SpecialsController : ControllerBase
    {
        IMediator _mediator;

        public PC_SpecialsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 某个专题页+分页
        /// </summary>
        /// <param name="shortId">专题短id</param>
        /// <param name="orderby">排序类型 1=最热 2=最新</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        [HttpGet("{shortId}/{orderby}")]
        [ProducesResponseType(typeof(PcSpecialIndexQueryResult), 200)]
        public async Task<ResponseResult> Index([CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path, Required = true)] long shortId,
            int orderby = 1, int pageIndex = 1, int pageSize = 20)
        {
            var r = await _mediator.Send(new PcSpecialIndexQuery
            {
                No = shortId,
                OrderBy = orderby,
                PageIndex = pageIndex,
                PageSize = pageSize,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// all专题
        /// </summary>
        /// <param name="a">接口请用回原来的接口 '/api/specials/list'</param>
        /// <returns></returns>
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<SimpleSpecialDto>), 200)]
        public ResponseResult List(string a)
        {
            _ = nameof(SpecialsController.List);
            return ResponseResult.Failed("接口请用回原来的接口 '/api/specials/list'");
        }

       


    }
}
