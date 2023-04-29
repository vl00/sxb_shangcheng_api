using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 小程序-Home
    /// </summary>
    [Area("mini")]
    [Route("/api/[area]/Home/[action]")]
    [ApiController]
    public class Mini_HomeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public Mini_HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }


        /// <summary>
        /// 首页数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MiniIndexDataDto), 200)]
        public async Task<ResponseResult> Data()
        {
            var res = await _mediator.Send(new MiniIndexDataQuery());
            return ResponseResult.Success(res);
        }

        /// <summary>
        /// 首页宝妈精选
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MiniIndexEvalts), 200)]
        public async Task<ResponseResult> Evaluations([FromQuery] MiniIndexEvaltQuery query)
        {
            if (query.PageIndex < 1 || query.PageSize < 1)
            {
                throw new CustomResponseException("请求参数有误！");
            }

            var res = await _mediator.Send(query);
            return ResponseResult.Success(res);
        }

        /// <summary>
        /// 首页运营专区4大项s
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MpMallOperateAreaQryResult), 200)]
        public async Task<ResponseResult> OperateArea()
        {
            var res = await _mediator.Send(new MpMallOperateAreaQuery { });
            return ResponseResult.Success(res);
        }

        /// <summary>
        ///  商城首页新的商品列表(含网课和好物)
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CoursesByOrgIdQueryResponse), 200)]
        public async Task<Res2Result> CoursePageLs(int pageIndex = 1, int pageSize = 10)
        {
            var ignoreCourse = UserAgentUtils.IsIos(HttpContext);

            var r = await _mediator.Send(new MpMallHomeCoursePageLsQuery { PageIndex = pageIndex, PageSize = pageSize, ExcludeCourseType1 = ignoreCourse });
            return Res2Result.Success(r);
        }
    }
}
