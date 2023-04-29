using iSchool.Api.Swagger;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Api.Conventions;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Activity.Appliaction;
using iSchool.Organization.Activity.Appliaction.RequestModels;
using iSchool.Organization.Activity.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ViewModels;

namespace iSchool.Organization.Activity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckActivityCode("promocode", Order = 9999)]
    public partial class EvaluationsController : ControllerBase
    {
        IMediator _mediator;

        public EvaluationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// (活动首页)点赞排行及其分页
        /// </summary>
        /// <param name="activityInfo"></param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">页码</param>
        /// <returns></returns>
        [HttpGet("{pageIndex}/{pageSize}")]
        [ProducesResponseType(typeof(EvaluationLikePageResult), 200)]            
        public async Task<ResponseResult> Index(int pageIndex = 1, int pageSize = 10,
            [FromQuery][ApiDocParameter("promocode", typeof(string), Desc = "推广码")]
            ActivityInfo activityInfo = default)
        {            
            var r = await _mediator.Send(new EvaluationLikePageQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                ActivityInfo = activityInfo,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 优秀案例及其分页
        /// </summary>
        /// <returns></returns>
        [HttpGet("excc")]
        [ProducesResponseType(typeof(ExcellentCasesEvltPageResult), 200)]
        public async Task<ResponseResult> ExcellentCases(
            [FromQuery][ApiDocParameter("promocode", typeof(string), Desc = "推广码")]
            ActivityInfo activityInfo = default)
        {
            var r = await _mediator.Send(new ExcellentCasesEvltPageQuery 
            {
                ActivityInfo = activityInfo,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 发评测用回原来的接口,原来的接口增加的参数可以在本接口中查看
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(EvaluationAddedResult), 200)]
        public Task<ResponseResult> AddEvaluation(Organization.Appliaction.RequestModels.AddEvaluationCommand cmd)
        {
            throw new CustomResponseException("发评测用回原来的接口,原来的接口增加的参数可以在本接口中查看");
        }
    }
}
