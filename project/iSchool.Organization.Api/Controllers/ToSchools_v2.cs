using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iSchool.Api.ModelBinders;
using iSchool.Api.Swagger;
using iSchool.Organization.Api.Conventions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Organization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 提供给b站那边的接口
    /// </summary>
    [Route("api/ToSchools/v2")]
    [ApiController]
    public class ToSchools_v2Controller : Controller
    {
        IMediator _mediator;

        public ToSchools_v2Controller(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 首页热卖课程
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        [HttpGet("hotsell/courses")]
        [ProducesResponseType(typeof(GetHotSellCoursesQueryResult), 200)]
        public async Task<ResponseResult> GetHotSellCourses(int pageIndex = 1, int pageSize = 10)
        {
            // 2
            var r = await _mediator.Send(new GetHotSellCoursesQuery 
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                MinAge = 0,
                MaxAge = 0,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 首页推荐机构(按pv排序)
        /// </summary> 
        /// <param name="orgType">品牌类型</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        [HttpGet("recommendorgs")]
        [ProducesResponseType(typeof(GetRecommendOrgsQueryResult), 200)]
        public async Task<ResponseResult> GetRecommendOrgs(int orgType = 0, int pageIndex = 1, int pageSize = 10)
        {            
            var r = await _mediator.Send(new GetRecommendOrgsQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Type = orgType,
            });
            return ResponseResult.Success(r);
        }


        /// <summary>
        /// 学校详情里的热卖课程
        /// </summary>
        /// <param name="grade">年级 1=幼儿园 2=小学 3=初中 4=高中</param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpGet("hotsell/courses/grade")]
        [ProducesResponseType(typeof(HotSellCoursesForSchoolV2QryResult), 200)]
        public async Task<ResponseResult> GetSchoolGradeHotSellCourses(/*int count,*/ int grade)
        {
            var (minage, maxage) = ToSchoolsHelper.GetAgesBySchoolGrade((iSchool.Domain.Enum.SchoolGrade)grade);
            var r = await _mediator.Send(new HotSellCoursesForSchoolV2Query { MinAge = minage, MaxAge = maxage });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 学校详情里的推荐机构(按销量排序)
        /// </summary>
        /// <param name="grade">年级 1=幼儿园 2=小学 3=初中 4=高中</param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpGet("hotsell/orgs/grade")]
        [ProducesResponseType(typeof(HotSellOrgsForSchoolV2QryResult), 200)]
        public async Task<ResponseResult> GetSchoolGradeHotSellOrgs(/*int count,*/ int grade)
        {
            var (minage, maxage) = ToSchoolsHelper.GetAgesBySchoolGrade((iSchool.Domain.Enum.SchoolGrade)grade);
            var r = await _mediator.Send(new HotSellOrgsForSchoolV2Query { MinAge = minage, MaxAge = maxage });
            return ResponseResult.Success(r);
        }
    }


}
