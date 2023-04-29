using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iSchool.Api.ModelBinders;
using iSchool.Api.Swagger;
using iSchool.Organization.Api.Conventions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Organization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 机构
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class OrganizationsController : Controller
    {
        IMediator _mediator;
        public OrganizationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region 机构列表[All OK]       

        /// <summary>
        /// [机构大全]根据查询条件，获取机构列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="courseOrOrgName">品牌(模糊查询)</param>
        /// <param name="type">机构类型</param>
       
        /// <param name="authentication">品牌认证(展示所有认证的品牌)</param>        
        /// <returns></returns>
        [HttpGet("OrgByContion")]
        [ProducesResponseType(typeof(OrganizationAllResponse), 200)]
        public async Task<ResponseResult> GetOrganizationsByContion(string courseOrOrgName, int? type, [BindBoolean] bool? authentication = null,             
            int pageIndex = 1, int pageSize = 10)
        {
            return await _mediator.Send(new OrganizationAllQuery
            {
                PageInfo = new PageInfo() { PageIndex = pageIndex, PageSize = pageSize },
                Type = type,
                Authentication = authentication,
                CourseOrOrgName = courseOrOrgName,
              
            });
        }
        /// <summary>
        /// 根据商品类型查找品牌
        /// </summary>
        /// <param name="catogryid"></param>
        /// <param name="authentication"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="SearchText"></param>
        /// <returns></returns>
        [HttpGet("OrgByCatogry")]
        [ProducesResponseType(typeof(OrganizationAllResponse), 200)]
        public async Task<ResponseResult> GetOrganizationsByCatogry(int? catogryid, int? catogorylevel, [BindBoolean] bool? authentication = null,
           int pageIndex = 1, int pageSize = 10,string SearchText="")
        {
            return await _mediator.Send(new CatogoryOrganizationQuery
            {
                PageInfo = new PageInfo() { PageIndex = pageIndex, PageSize = pageSize },
                Authentication = authentication,
                CatogoryId= catogryid,
                SearchText= SearchText,
                CatogoryLevel= catogorylevel
            });
        }

        /// <summary>
        /// [评测品牌--]根据机构名称，获取机构列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orgName">品牌(模糊查询)</param>
        /// <returns></returns>
        [HttpGet("OrgByName/{orgName}")]
        [ProducesResponseType(typeof(List<OrganizationByNameResponse>), 200)]
        public ResponseResult GetOrganizationsByName(string orgName, int pageIndex = 1, int pageSize = 5)
        {
            return _mediator.Send(new OrganizationByNameQuery
            {
                PageInfo = new PageInfo() { PageIndex = pageIndex, PageSize = pageSize },
                OrgName = orgName

            }).Result;
        }


        #endregion

        #region 根据机构Id，获取机构信息
        /// <summary>
        /// [机构详情页]--{相关评测、相关课程、非合作-新、非合作-认领机构、无课程无详情}
        /// </summary>
        /// <param name="noId">机构短Id</param>
        /// <returns></returns>
        [HttpGet("{noId}")]
        [ProducesResponseType(typeof(OrganizationByIdQueryResponse), 200)]
        public async Task<ResponseResult> GetOrganizationById(
            [CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path)]
            long noId)
        {
            return await _mediator.Send(new OrganizationByIdQuery { No = noId });
        }
        #endregion

        #region 认领机构[All OK]
        /// <summary>
        /// [认领机构]
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ResponseResult ClaimOrganization([FromBody] ClaimOrganizationRequest request)
        {
            return _mediator.Send(request).Result;
        }
        #endregion
    }


}
