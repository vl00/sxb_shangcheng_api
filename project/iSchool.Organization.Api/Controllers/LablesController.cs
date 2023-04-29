using System;
using System.Collections.Generic;
using iSchool.Organization.Api.Conventions;
using iSchool.Api.Swagger;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Course;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.DependencyInjection;//获取用户信息需要引入
using Microsoft.AspNetCore.Authorization;
using iSchool.Organization.Appliaction.ResponseModels.Courses;
using System.Net.Http;
using System.IO;
using System.Linq;
using iSchool.Organization.Api.Filters;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.Service.Lables;
using iSchool.Organization.Appliaction.ResponseModels.Lables;
using iSchool.Organization.Appliaction.RequestModels.Orders;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// PC官网相关标签管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LablesController : Controller
    {
        IMediator _mediator;        
        public LablesController(IMediator mediator)
        {
            _mediator = mediator;            
        }

        #region 机构评测卡片

        /// <summary>
        /// 机构评测卡片
        /// </summary>
        /// <param name="evalUrl">评测详情页Url</param>
        /// <returns></returns>
        [HttpGet("/EvalLable")]
        [ProducesResponseType(typeof(EvalDetailsLable), 200)]
        public async Task<ResponseResult> GetEvaltDetailsByUrl(string evalUrl)
        {
            return await _mediator.Send(new EvalLableByUrlQuery() { EvalDetailUrl = evalUrl });
        }

        /// <summary>
        /// 【长Id集合】机构评测列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("/EvalsLablesByIds")]
        [ProducesResponseType(typeof(List<EvalDetailsLable>), 200)]
        public async Task<ResponseResult> GetEvaltsByIds(EvaltsLableByIdsQuery request)
        {
            return await _mediator.Send(request);
        }

        /// <summary>
        /// 【短Id集合】机构评测列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("/EvalsLablesById_ss")]
        [ProducesResponseType(typeof(List<EvalDetailsLable>), 200)]
        public async Task<ResponseResult> GetEvaltsById_ss(EvaltsLableById_ssQuery request)
        {
            return await _mediator.Send(request);
        }


        #endregion

        #region 课程卡片列表
        /// <summary>
        /// 【多条件分页】,课程卡片列表
        /// </summary>
        /// <param name="title">课程标题</param>
        /// <param name="orgName">机构名称</param>  
        /// <param name="subjectId">科目Id</param>    
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小(默认加载15条)</param>        
        /// <returns></returns>
        [HttpGet("/CoursesLables")]
        [ProducesResponseType(typeof(List<CoursesLablesResponse>), 200)]
        public ResponseResult GetCoursesLablesByInfo(string title,string orgName, int? subjectId, int pageIndex=1, int pageSize=15 )
        {
            var request = new CoursesLablesByInfoQuery
            {
                PageInfo = new PageInfo() { PageIndex = pageIndex, PageSize = pageSize },
                Title = title,
                OrgName = orgName,
                SubjectId = subjectId
            };
            return _mediator.Send(request).Result;

        }


        /// <summary>
        /// 【长Id集查询】,课程卡片列表
        /// </summary>     
        /// <returns></returns>
        [HttpPost("/CoursesLablesByIds")]
        [ProducesResponseType(typeof(List<CourseLable>), 200)]
        public ResponseResult GetCoursesLablesByIds(CoursesLablesByIdsQuery request)
        {           
            return _mediator.Send(request).Result;

        }

        /// <summary>
        /// 【短Id集查询】,课程卡片列表
        /// </summary>     
        /// <returns></returns>
        [HttpPost("/CoursesLablesById_ss")]
        [ProducesResponseType(typeof(List<CourseLable>), 200)]
        public ResponseResult GetCoursesLablesById_ss(CoursesLablesById_ssQuery request)
        {
            return _mediator.Send(request).Result;

        }


        #endregion

        #region 机构卡片列表
        /// <summary>
        /// 机构卡片列表
        /// </summary>
        /// <param name="orgName">机构名称</param>
        /// <param name="type">学科Id</param>  
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns></returns>
        [HttpGet("/OrgsLables")]
        [ProducesResponseType(typeof(OrgsLablesResponse), 200)]
        public async Task<ResponseResult> GetOrgsByInfo(string orgName, int? type, int pageIndex = 1, int pageSize = 10)
        {
            return await _mediator.Send(new OrgsLablesByInfoQuery
            {
                PageInfo = new PageInfo() { PageIndex = pageIndex, PageSize = pageSize },
                Type = type,
                OrgName=orgName
            });
        }

        /// <summary>
        /// 【长ID集合】机构卡片列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("/OrgsLablesByIds")]
        [ProducesResponseType(typeof(OrgsLablesResponse), 200)]
        public async Task<ResponseResult> GetOrgsByIds(OrgsLablesByIdsQuery request)
        {
            return await _mediator.Send(request);
        }

        /// <summary>
        /// 【短Id集合】机构卡片列表
        /// </summary>
        /// <param name="orgName">机构名称</param>
        /// <param name="type">学科Id</param>  
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns></returns>
        [HttpPost("/OrgsLablesById_ss")]
        [ProducesResponseType(typeof(OrgsLablesResponse), 200)]
        public async Task<ResponseResult> GetOrgsById_ss(OrgsLablesById_ssQuery request)
        {
            return await _mediator.Send(request);
        }

        /// <summary>
        /// 机构卡片列表
        /// </summary>
        /// <param name="orgId">机构Id</param>
        /// <returns></returns>
        [HttpGet("/SearchOrgById")]
        [ProducesResponseType(typeof(OrgsLablesResponse), 200)]
        public async Task<ResponseResult> SearchOrgById(Guid orgId)
        {
            return await _mediator.Send(new OrgsLablesByInfoQuery
            {
                PageInfo = new PageInfo() { PageIndex = 1, PageSize = 10 },
                OrgId = orgId
            });
        }
        #endregion


        /// <summary>
        /// 查询订单封面
        /// </summary>     
        /// <returns></returns>
        [HttpPost("/GetOrderBanner")]
        [ProducesResponseType(typeof(List<string>), 200)]
        public ResponseResult OrderProductBannerQuery(OrderProductBannerQuery request)
        {
            return _mediator.Send(request).Result;

        }
    }
}
