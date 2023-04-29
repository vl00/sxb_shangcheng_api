using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Api.Swagger;
using iSchool.Domain.Modles;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.DrpFx;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 分销
    /// </summary>
    [Route("api/drpfx")]
    [ApiController]
    public class DrpFxController : Controller
    {
        private readonly IMediator _mediator;

        public DrpFxController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// [1.6*]课程分销-推广奖励信息
        /// </summary>
        /// <param name="id">课程id</param>
        /// <returns></returns>
        [HttpGet("course/tginfo")]
        [ProducesResponseType(typeof(GetCourseDrpFxInfoDto), 200)]
        public async Task<ResponseResult> GetCourseDrpFxTuigInfo([ApiDocParameter(Required = true)] Guid id)
        {
            var r = await _mediator.Send(new GetCourseDrpFxInfoQuery { CourseId = id });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// get分销组建团队条件情况(前端调用)
        /// </summary>
        /// <param name="me">我</param>
        /// <returns></returns>
        [Authorize, CheckBindMobile]
        [HttpGet("headerfx/team/setup")]
        [ProducesResponseType(typeof(HeaderFxTeamSetupInfoDto), 200)]
        public async Task<ResResult> GetHeaderFxTeamSetup([FromServices] IUserInfo me)
        {
            var r = await _mediator.Send(new GetHeaderFxTeamSetupQuery { UserId = me.UserId });
            return ResResult.Success(r);
        }

        /// <summary>
        /// get分销组建团队条件情况(后端调用)
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        [HttpGet("headerfx/team/setup2")]
        [ProducesResponseType(typeof(HeaderFxTeamSetupInfoDto), 200)]
        public async Task<ResResult> GetHeaderFxTeamSetup2(Guid id)
        {
            var r = await _mediator.Send(new GetHeaderFxTeamSetupQuery { UserId = id });
            return ResResult.Success(r);
        }

        /// <summary>
        /// [分销后台] 批量获取用户评测和购买课程info
        /// </summary>
        /// <param name="userids"></param>
        /// <returns></returns>
        [HttpPost("users/evltandbuycourse")]
        [ProducesResponseType(typeof(UserEvltAndBuyCourseInfoDto[]), 200)]
        public async Task<ResResult> GetUserEvltAndBuyCourseInfo(Guid[] userids)
        {
            var r = await _mediator.Send(new GetUserEvltAndBuyCourseInfoQuery { UserIds = userids });
            return ResResult.Success(r);
        }

        /// <summary>
        /// [分销后台] 批量获取用户最新浏览课程信息
        /// </summary>
        /// <param name="userids"></param>
        /// <returns></returns>
        [HttpPost("users/visitlog")]
        [ProducesResponseType(typeof(List<UserCourseVisitLog>), 200)]
        public async Task<ResResult> GetUserCourseVisitLog(List<Guid> userids)
        {
            var r = await _mediator.Send(new GetUserCourseVisitLogQuery { UserIds = userids });
            return ResResult.Success(r);
        }

    }
}
