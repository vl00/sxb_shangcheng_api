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
    [Route("api/[area]/Courses")]
    [ApiController]
    public class PC_CoursesController : ControllerBase
    {
        IMediator _mediator;

        public PC_CoursesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 课程列表+分页
        /// </summary>
        /// <param name="subj">科目</param>
        /// <param name="authentication">品牌认证(展示所有认证的品牌)</param>
        /// <param name="orgId">机构短id</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        [HttpGet("ls")]
        [ProducesResponseType(typeof(PcCourseIndexQueryResult), 200)]
        public async Task<ResponseResult> Index(int? subj = null, [BindBoolean] bool? authentication = null, int pageIndex = 1, int pageSize = 20,
            [CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Query)] 
            long orgId = -1)
        {
            var r = await _mediator.Send(new PcCourseIndexQuery
            {
                OrgNo = orgId > 0 ? orgId : (long?)null,
                Subj = subj,
                Authentication = authentication,
                PageIndex = pageIndex,
                PageSize = pageSize,
            });
            return ResponseResult.Success(r);
        }


        /// <summary>
        /// pc课程详情
        /// </summary>
        /// <param name="id" type="string">课程短id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PcCourseDetailDto), 200)]
        public async Task<ResponseResult> GetCourse(
            [CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path, Required = true)]
            long id)
        {
            var r = await _mediator.Send(new PcCourseDetailQuery { No = id });
            return ResponseResult.Success(r);
        }


    }
}
