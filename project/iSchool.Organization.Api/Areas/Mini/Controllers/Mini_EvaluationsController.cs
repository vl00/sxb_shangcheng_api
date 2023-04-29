using iSchool.Api.Swagger;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
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
    /// <summary>
    /// 小程序-种草(评测)
    /// </summary>
    [Area("mini")]
    [Route("/api/[area]/Evaluations")]
    [ApiController]
    public partial class Mini_EvaluationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public Mini_EvaluationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 种草圈
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderby">(综合)排序</param>
        /// <param name="brand">品牌.(机构id)</param>
        /// <param name="CatogryId">商品分类ID</param>
        /// <param name="ctt">内容形式</param>
        /// <returns></returns>
        [HttpGet("grass/ls")]
        [ProducesResponseType(typeof(MiniEvltGrassIndexQryResult), 200)]
        public async Task<ResponseResult> Grass_Index(string CatogryId, int pageIndex = 1, int pageSize = 10,
            int orderby = 0, Guid? brand = null, int ctt = 0)
        {
            var r = await _mediator.Send(new MiniEvltGlassIndexQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Orderby = orderby,
                Brand = brand,
                CatogoryId = CatogryId,
                Ctt = ctt,
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 大家的种草 -- 具体某个课程进去的那个课程下面的种草内容
        /// </summary>
        /// <param name="str_courseid">课程id(长度都行)</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderby">(综合)排序</param>
        /// <param name="ctt">内容形式</param>
        /// <returns></returns>
        [HttpGet("grass/everybody/ls")]
        [ProducesResponseType(typeof(MiniEvltGrassIndexQryResult), 200)]
        public async Task<ResponseResult> Grass_Index_Everybody([FromQuery(Name = "courseid"), ApiDocParameter("courseid", Required = true)] string str_courseid,
            int pageIndex = 1, int pageSize = 10,
            int orderby = 0, int ctt = 0)
        {
            var courseNo = Guid.TryParse(str_courseid, out var courseId) ? 0L : UrlShortIdUtil.Base322Long(str_courseid);

            var courseInfo = await _mediator.Send(new CourseBaseInfoQuery { CourseId = courseId, No = courseNo });

            var r = await _mediator.Send(new MiniEvltGlassIndexQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                CourseId = courseInfo.Id,
                Orderby = orderby,
                Ctt = ctt,
            });

            return ResponseResult.Success(r);
        }

        /// <summary>
        /// [2.0*] 种草详情
        /// </summary>
        /// <param name="id">评测id（长短都行）</param>
        /// <param name="allowIosNodisplay">
        /// 1=ios会屏蔽网课 <br/>
        /// 0=ios不会屏蔽网课
        /// </param>
        /// <returns></returns>
        [HttpGet("grass/{id}")]
        [ProducesResponseType(typeof(MiniEvltDetailDto), 200)]
        public async Task<ResponseResult> GrassEvltDetail([ApiDocParameter(Required = true)] string id, int allowIosNodisplay = 1)
        {
            var no = Guid.TryParse(id, out var gid) ? 0L : UrlShortIdUtil.Base322Long(id);
            var r = await _mediator.Send(new MiniEvltDetailQuery { Id = gid, No = no, AllowIosNodisplay = allowIosNodisplay });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// [1.8*] 发评测 or 修改评测
        /// </summary>
        /// <param name="cmd"></param>        
        /// <returns></returns>
        [HttpPost]
        [Authorize, CheckBindMobile]
        [ProducesResponseType(typeof(EvaluationAddedResult), 200)]
        public async Task<ResponseResult> AddEvaluation(MiniAddEvaluationCommand cmd)
        {
#if !DEBUG
            return null;
#else
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
#endif
        }



        /// <summary>
        /// 我的种草圈
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("my")]
        [Authorize]
        [ProducesResponseType(typeof(MiniMyEvaluationsDto), 200)]
        public async Task<ResponseResult> MyEvaluations([FromQuery] MiniMyEvaluationsQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }



        ///// <summary>
        ///// 删除种草
        ///// </summary>
        ///// <param name="command"></param>
        ///// <returns></returns>
        //[HttpPost("del")]
        //[Authorize]
        //[ProducesResponseType(typeof(bool), 200)]

        //public async Task<ResponseResult> DelEvalutation([FromBody] MiniDelEvaluationCommand command)
        //{
        //    var r = await _mediator.Send(command);
        //    return ResponseResult.Success(r);
        //}





        /// <summary>
        /// 根据id批量查询种草（用于搜索）
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("Search")]
        [ProducesResponseType(typeof(List<MiniEvaluationItemDto>), 200)]
        public ResponseResult Search([FromBody] MiniEvaluationSearchQuery query)
        {
            if (query == null || query.Ids == null || query.Ids.Count() == 0)
            {
                return ResponseResult.Success(new List<MiniEvaluationItemDto>());
            }

            var res = _mediator.Send(query).Result;
            return ResponseResult.Success(res);
        }

        /// <summary>
        /// 更新种草分享数
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(UpMiniSharedCounts))]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> UpMiniSharedCounts(MiniEvltUpSharedCountsCmd cmd)
        {
            await _mediator.Send(cmd);
            return ResponseResult.Success(true);
        }

        /// <summary>
        /// 增加种草下载素材数
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(AddDownloadMaterialCount))]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> AddDownloadMaterialCount(MiniAddDownloadMaterialCountCmd cmd)
        {
            await _mediator.Send(cmd);
            return ResponseResult.Success(true);
        }
        /// <summary>
        /// 种草关联购买商品
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetRelBuyGood))]
        [Authorize]
        [ProducesResponseType(typeof(RelOrderProdsQueryResult), 200)]
        public async Task<ResponseResult> GetRelBuyGood(RelOrderProdsQuery cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }
    }
}
