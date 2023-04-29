using iSchool.Api.Swagger;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Api.Conventions;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Appliaction;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Controllers
{    
    [Area("PC")]
    [Route("/api/[area]/Evaluations")]
    [ApiController]
    public partial class PC_EvaluationsController : ControllerBase
    {
        readonly IMediator _mediator;

        public PC_EvaluationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// pc首页+分页
        /// </summary>
        /// <param name="subj">科目,多选用“,”隔开</param>        
        /// <param name="stick">
        /// 是否只要精华.<br/>
        /// 1=只要精华<br/>
        /// 0=全部
        /// </param>
        /// <param name="orgid">机构短id. 用于机构页面跳转过来显示机构信息.</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="r">
        /// 表示科目栏是否显示全部.<br/>
        /// 例如从某个详情页里的相关评测跳转过来.<br/>
        /// 0(或默认)=不显示 <br/>
        /// 1=显示
        /// </param>
        /// <returns></returns>
        [HttpGet("ls")]
        [ProducesResponseType(typeof(PcEvaluationIndexQueryResult), 200)]
        public async Task<ResponseResult> Index(int pageIndex = 1, int pageSize = 20, string subj = null, int? stick = null, int r = 0,
            [CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Query)] 
            long orgid = -1)
        {
            var rr = await _mediator.Send(new PcEvaluationIndexQuery 
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Subj = subj,
                Stick = stick ?? 0,
                OrgNo = orgid > 0 ? orgid : (long?)null, //参数写long?会变成0
                R = r,
            });
            return ResponseResult.Success(rr);
        }

        /// <summary>
        /// 评测详情
        /// </summary>
        /// <param name="id" type="string">评测短id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PcEvltDetailDto), 200)]
        public async Task<ResponseResult> GetEvaluation(
            [CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path, Required = true)]
            long id)
        {
            var r = await _mediator.Send(new PcEvltDetailQuery { No = id });
            return ResponseResult.Success(r);
        }

    }

    public partial class PC_EvaluationsController
    {
        /// <summary>
        /// 分页查询某个评测里的评论列表
        /// </summary>
        /// <param name="id">评测id</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小，默认10</param>
        /// <returns></returns>
        [HttpGet("{id}/comments/{pageIndex}")]
        [ProducesResponseType(typeof(PagedList<PcEvaluationCommentDto>), 200)]
        public async Task<ResponseResult> GetEvaluationComments(Guid id, int pageIndex, int pageSize = 10)
        {
            var r = await _mediator.Send(new PcEvltCommentsQuery
            {
                EvltId = id,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Naf = DateTime.Now,
            });
            return ResponseResult.Success(r);
        }
    }
}
