using System;
using System.Collections.Generic;
using System.Linq;
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
    /// 提供给school那边的接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ToSchoolsController : Controller
    {
        IMediator _mediator;

        public ToSchoolsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 热卖课程+推荐机构
        /// </summary>
        /// <param name="minage">最小年龄</param>
        /// <param name="maxage">最大年龄</param>
        /// <returns></returns>
        [HttpGet("hotsell/coursesandorgs")]
        [ProducesResponseType(typeof(HotSellCoursesOrgsForSchoolsQryResult), 200)]
        public async Task<ResponseResult> GetHotSellCoursesAndOrgs(int minage, int maxage)
        {
            var r = await _mediator.Send(new HotSellCoursesOrgsForSchoolsQuery { MinAge = minage, MaxAge = maxage });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 学校pc首页推荐机构
        /// </summary>
        /// <param name="type">品牌类型</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        [HttpGet("SubjRecommendOrgs")]
        [ProducesResponseType(typeof(PcOrgIndexQueryResult), 200)]
        public async Task<ResponseResult> GetSubjRecommendOrgs(int type = 1, int pageIndex = 1, int pageSize = 12)
        {
            var r = await _mediator.Send(new PcGetSubjRecommendOrgsQuery
            {
                Type = type,
                PageIndex = pageIndex,
                PageSize = pageSize,
            });
            return ResponseResult.Success(r);
        }

        #region 根据Id集合，提供机构列表api、课程列表api、评测列表api

        /// <summary>
        /// 机构列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Obsolete, HttpPost("OrgList")]
        [ProducesResponseType(typeof(List<OrgQueryResult>), 200)]
        public async Task<ResponseResult> GetOrgsByIds(OrgsByIDsQuery query)
        {
            
            var request = await _mediator.Send(query);
            return request;
        }

        /// <summary>
        /// 课程列表
        /// </summary>        
        /// <returns></returns>
        [Obsolete, HttpPost("CourseList")]
        [ProducesResponseType(typeof(List<CoursesQueryResult>), 200)]
        public async Task<ResponseResult> GetCoursesByIds(CoursesByIdsQuery query)
        {
            var request = await _mediator.Send(query);
            return request;

        }

        /// <summary>
        /// 评测列表
        /// </summary>        
        /// <returns></returns>
        [HttpPost("EvltList")]
        [ProducesResponseType(typeof(List<EvltQueryResult>), 200)]
        public async Task<ResponseResult> GetEvltsByIds(EvltsByIdsQuery query)
        {
            var request = await _mediator.Send(query);
            return request;

        }

        #endregion


        /// <summary>
        /// 根据长id或短id批量查课程infos
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("infos/courses")]
        [ProducesResponseType(typeof(GetCourseInfosForSchoolsQryResult), 200)]
        public async Task<ResponseResult> GetCoursesInfos(GetCourseInfosForSchoolsQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 根据长id或短id批量查机构/品牌infos
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("infos/orgs")]
        [ProducesResponseType(typeof(GetOrgInfosForSchoolsQryResult), 200)]
        public async Task<ResponseResult> GetOrgsInfos(GetOrgInfosForSchoolsQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 用于广告 返回21个课程|好物
        /// </summary>        
        /// <returns></returns>
        [HttpPost("gg21")]
        [ProducesResponseType(typeof(GG21QryResult), 200)]
        public async Task<ResponseResult> GG21(GG21Query query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// RwInviteActivity - 根据unionID查询下单数
        /// </summary>        
        /// <returns></returns>
        [HttpPost("rwInviteActivity/OrderCount")]
        [ProducesResponseType(typeof(GetRwInviteActivityOrderCountQryResultItem[]), 200)]
        public async Task<ResponseResult> GetRwInviteActivityOrderCounts(GetRwInviteActivityOrderCountQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }
    }


}
