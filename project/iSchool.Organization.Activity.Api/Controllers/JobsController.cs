using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Organization.Activity.Appliaction.RequestModels;
using iSchool.Organization.Activity.Appliaction.Service.Jobs;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace iSchool.Organization.Activity.Api.Controllers
{
    /// <summary>
    /// 后台作业
    /// </summary>
    [ApiController, Route("[controller]")]
#if !DEBUG
    [Authorize("jobs")]
#endif
    public class JobsController : Controller
    {
        IMediator _mediator;

        public JobsController(IMediator _mediator)
        {
            this._mediator = _mediator;
        }

        /// <summary>
        /// 微信公众号定期提醒--用户测评点赞数
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(SyncRegularNotice))]
        public async Task SyncRegularNotice(SyncRegularNoticeCommand cmd)
        {
            await _mediator.Send(cmd);
        }

        /// <summary>
        /// 计算活动期间用户评测点赞排行
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(ComputeActivityUserEvltLikeRank))]
        public async Task<ResponseResult> ComputeActivityUserEvltLikeRank()
        {
            return await _mediator.Send(new ComputeActivityUserEvltLikeRankCommand());
        }

    }
}
