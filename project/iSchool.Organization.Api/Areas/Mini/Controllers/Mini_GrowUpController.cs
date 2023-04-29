using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace iSchool.Organization.Api.Controllers
{

    /// <summary>
    /// 小程序-成长
    /// </summary>
    [Area("mini")]
    [Route("/api/[area]/GrowUp/[action]")]
    [ApiController]
    public class Mini_GrowUpController : ControllerBase
    {

        private readonly IMediator _mediator;

        public Mini_GrowUpController(IMediator mediator)
        {
            _mediator = mediator;
        }


        /// <summary>
        ///获取成长页面内容
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(MiniGrowUpDataDto), 200)]
        public async Task<ResponseResult> Data([FromQuery] MiniGrowUpDataQuery query)
        {

            var list = await _mediator.Send(query);
            return ResponseResult.Success(list);
        }

        /// <summary>
        /// 获取孩子档案
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(List<MiniChildArchiveItemDto>), 200)]
        public async Task<ResponseResult> ChildArchives()
        {
            var list = await _mediator.Send(new MiniChildArchivesQuery());
            return ResponseResult.Success(list);
        }

        /// <summary>
        /// 添加孩子档案
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<ResponseResult> ChildArchives([FromBody] MiniAddChildArchiveCommand command)
        {
            var res = await _mediator.Send(command);
            return res;
        }


        /// <summary>
        /// 修改孩子档案
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> ChildArchives([FromBody] MiniUpdateChildArchiveCommand command)
        {
            var res = await _mediator.Send(command);
            return res;
        }




        /// <summary>
        /// 删除孩子档案
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> ChildArchives([FromBody] MiniDeleteChildArchiveCommand command)
        {
            var res = await _mediator.Send(command);
            return res;
        }



        /// <summary>
        /// 根据useid list 批量获取孩子档案（只返回第一个）
        /// </summary>
        /// <returns></returns>

        //[Authorize]

        [HttpPost]
        public async Task<ResponseResult> ChildArchiveList([FromBody] List<Guid> userIds)
        {
            if (userIds == null || userIds.Count() == 0)
                return ResponseResult.Success(new List<MiniChildArchiveItemDto>());
            var res = await _mediator.Send(new MiniChildArchiveListQuery { UserIds = userIds });
            return ResponseResult.Success(res);
        }

    }
}
