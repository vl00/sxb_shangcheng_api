using iSchool.Api.Swagger;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
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
    /// 专题
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialsController : ControllerBase
    {
        IMediator _mediator;

        public SpecialsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 专题列表页 -- 暂无原型
        /// </summary>
        /// <param name="orderby"></param>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<ResponseResult> Index0(int orderby = 1)
        {
            await Task.CompletedTask;
            throw new NotSupportedException("暂无原型");
        }

        #region v1
        /// <summary>
        /// 某个专题页,主要用于首次打开
        /// </summary>
        /// <param name="shortId">专题短id</param>
        /// <param name="orderby">排序类型 1=最热 2=最新</param>
        /// <returns></returns>
        [HttpGet("{shortId}/{orderby}")]
        [ProducesResponseType(typeof(SpecialResEntity), 200)]
        public async Task<ResponseResult> Index1([CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path)] long shortId,
            int orderby = 1)
        {
            var r = await _mediator.Send(new SpecialReqQuery
            {
                No = shortId,
                OrderBy = orderby,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 分页查询某专题下的评测列表
        /// </summary>
        /// <param name="id">专题id</param>
        /// <param name="orderby">排序类型 1=最热 2=最新</param>
        /// <param name="pageIndex">页码</param>        
        /// <returns></returns>
        [HttpGet("{id}/{orderby}/{pageIndex}")]
        [ProducesResponseType(typeof(LoadMoreResult<EvaluationItemDto>), 200)]
        public async Task<ResponseResult> LoadMoreEvaluations(Guid id, int orderby, int pageIndex)
        {
            var r = await _mediator.Send(new SpecialLoadMoreEvaluationsQuery
            {
                Id = id,
                OrderBy = orderby,
                PageIndex = pageIndex,
            });
            return ResponseResult.Success(r);
        }
        #endregion v1

        /// <summary>
        /// 某个专题页,首次+分页
        /// </summary>
        /// <param name="shortId">专题短id</param>
        /// <param name="smallShortId">小专题Id(查询大专题下某个小专题，默认查全部，不用传)</param>
        /// <param name="orderby">排序类型 1=最热 2=最新</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        [HttpGet("v2/{shortId}/{orderby}/{pageIndex}")]
        [ProducesResponseType(typeof(SpecialResEntity2), 200)]
        public async Task<ResponseResult> Index2([CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path)] long shortId,
            [CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Query)] long smallShortId = -1,
            int orderby = 1, int pageIndex = 1)
        {
            var r = await _mediator.Send(new SpecialReqQuery2
            {
                No = shortId,
                OrderBy = orderby,
                PageIndex = pageIndex,
                SmallShortId = smallShortId > 0 ? smallShortId : (long?)null,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 专题列表
        /// </summary>
        /// <param name="acd">活动码</param>
        /// <returns></returns>
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<SimpleSpecialDto>), 200)]
        public async Task<ResponseResult> List(string acd)
        {
            var r = await _mediator.Send(new SimpleSpecialQuery
            {
                Code = acd,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 评测加入专题
        /// </summary>
        /// <param name="specialId">专题id</param>
        /// <param name="evltId">评测id</param>
        /// <returns></returns>
        [Authorize, CheckBindMobile]
        [HttpPost("{id}/evaluation/{evltId}")]
        [ProducesResponseType(typeof(AddEvaluationToSpecialsCommandResult), 200)]
        public async Task<ResponseResult> EvaluationAddToSpecial(
            [FromRoute(Name = "id")] Guid specialId,
            [FromRoute(Name = "evltId")] Guid evltId)
        {
            var r = await _mediator.Send(new AddEvaluationToSpecialsCommand
            {
                EvltId = evltId,
                SpecialId = specialId,
            });
            return ResponseResult.Success(r);
        }
    }
}
