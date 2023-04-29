using iSchool.Api.ModelBinders;
using iSchool.Api.Swagger;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Api.Conventions;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Appliaction;
using iSchool.Organization.Appliaction.Mini.RequestModels.Courses;
using iSchool.Organization.Appliaction.Mini.ResponseModels.Courses;
using iSchool.Organization.Appliaction.Mini.ResponseModels.MaterialLibrary;
using iSchool.Organization.Appliaction.Mini.Services.Courses;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Course;
using iSchool.Organization.Domain.Security;
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
    /// 小程序-课程
    /// </summary>
    [Area("mini")]
    [Route("api/[area]/Courses")]
    [ApiController]
    public class Mini_CoursesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public Mini_CoursesController(IMediator mediator)
        {
            _mediator = mediator;
        }


        #region 精选课程列表
        /// <summary>
        ///  [精选课程列表]
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("List")]
        [ProducesResponseType(typeof(List<CoursesByOrgIdQueryResponse>), 200)]
        public ResponseResult GetCoursesByInfo([FromQuery] MiniCoursesByInfoQuery request)
        {

            return _mediator.Send(request).Result;

        }
        /// <summary>
        ///  [精选课程列表条件多选]
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("ListMutiFilter")]
        [ProducesResponseType(typeof(List<CoursesByOrgIdQueryResponse>), 200)]
        public ResponseResult ListMutiFilter([FromQuery] MiniCoursesByInfoMutiFilterQuery request)
        {

            return _mediator.Send(request).Result;

        }
        /// <summary>
        ///  [精选课程列表条件多选总商品数量]
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("ListMutiFilterTotalCount")]
        public ResponseResult ListMutiFilterTotalCount([FromQuery] MiniCoursesByInfoMutiFilterCountQuery request)
        {

            return _mediator.Send(request).Result;

        }

        
        #endregion
        #region 好物精选
        /// <summary>
        ///  [好物精选,爆款]
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("RecommendGoodThing")]
        [ProducesResponseType(typeof(CoursesByOrgIdQueryResponse), 200)]
        public ResponseResult GetGoodThingByInfo([FromQuery] MiniGoodThingRecommendQuery request)
        {

            return _mediator.Send(request).Result;

        }
        #endregion


        /// <summary>
        /// 根据id批量查询课程or好物
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("Search")]
        [ProducesResponseType(typeof(List<MiniCourseItemDto>), 200)]
        public async Task<ResponseResult> Search([FromBody] MiniCourseSearchQuery query)
        {
            if (query == null || query.Ids == null || query.Ids.Count() == 0)
            {
                return ResponseResult.Success(new List<MiniCourseItemDto>());
            }

            var res = await _mediator.Send(query);

            return ResponseResult.Success(res);
        }


        /// <summary>
        /// 获取种草的扩展字段
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>

        [HttpGet("Extend/{no}")]

        public async Task<ResponseResult> CourseDetailExtend([CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path)] long no)
        {
            var res = await _mediator.Send(new MiniCourseDetailExtendQuery { No = no });
            return ResponseResult.Success(res);
        }


        /// <summary>
        /// RwInviteActivity 获取隐形商品s的详情
        /// </summary>
        /// <param name="city">城市code.</param>
        /// <returns></returns>

        [HttpGet("rwInviteActivity/courses")]
        [ProducesResponseType(typeof(MiniRwInviteActivityCoursesQryResult), 200)]
        public async Task<ResponseResult> RwInviteActivityCoursesQuery(int? city = null)
        {
            var res = await _mediator.Send(new MiniRwInviteActivityCoursesQuery { City = city });
            return ResponseResult.Success(res);
        }


        /// <summary>
        /// 首页推荐的限时低价与新人专享
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<CoursesData>), 200)]
        [HttpGet("LowPriceRecomend/courses")]
        public async Task<ResponseResult> LowPriceRecomend()
        {
            //var res = await _mediator.Send(new MiniLowPriceRecommendQuery { });
            //return ResponseResult.Success(res);

            return ResponseResult.Success(ResponseResult.Success(new List<CoursesData>()));
        }

        /// <summary>
        /// 新人专享，限时低价课程分页查询
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<CoursesData>), 200)]
        [HttpGet("ActivityList/courses")]
        public  ResponseResult ActivityList([FromQuery] MiniActivityCourseListQuery request)
        {
            return _mediator.Send(request).Result;

        }
        /// <summary>
        ///素材圈
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(MaterialLibraryQueryResponse), 200)]
        [HttpGet("MaterialLibraryList")]
        public ResponseResult MaterialLibraryList([FromQuery] MiniMaterialLibraryListQuery request)
        {
            return _mediator.Send(request).Result;

        }
        /// <summary>
        /// 素材详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(MiniMaterialLibraryItemDto), 200)]
        [HttpGet("MaterialLibraryDetail")]
        public ResponseResult MaterialLibraryDetail([FromQuery] MiniMaterialLibraryDetailQuery request)
        {
            return _mediator.Send(request).Result;

        }
        /// <summary>
        /// 增加下载素材数
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(AddDownloadCourseMaterialCount))]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> AddDownloadCourseMaterialCount(MiniAddDownloadCourseMaterialCountCmd cmd)
        {
            await _mediator.Send(cmd);
            return ResponseResult.Success(true);
        }


        /// <summary>
        /// 积分商品
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("pointsGoods")]
        [ProducesResponseType(typeof(IEnumerable<MiniCoursePointsGoods>), 200)]
        public async Task<ResponseResult> GetPointsGoods([FromQuery] MiniCoursePointsGoodsQuery request)
        {
           var pointsGoods  = await _mediator.Send(request);

            return ResponseResult.Success(pointsGoods, "OK");

        }
    }
}
