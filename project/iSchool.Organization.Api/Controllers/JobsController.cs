using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 后台作业
    /// </summary>
    [ApiController, Route("[controller]")]
#if !DEBUG
    [Authorize("jobs")]
#endif
    public class JobsController : ControllerBase
    {
        IMediator _mediator;

        public JobsController(IMediator _mediator)
        {
            this._mediator = _mediator;
        }

        [HttpGet(nameof(Index))]
        [AllowAnonymous]
        public async Task<int> Index()
        {
            var t = new LongUrlToShortUrlRequest() { OriginUrl= "https://apiorgtest.sxkid.com:9981/api/Organizations/OrgByContion?type=203&authentication=&pageIndex=1&pageSize=16&courseOrOrgName=" };
            var ss = await _mediator.Send(t);
            return 0;
        }

        /// <summary>
        /// 同步评测点赞
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(SyncEvltLike))]
        public async Task SyncEvltLike(SyncEvltLikeCommand cmd)
        {
            await _mediator.Send(cmd);
        }

        /// <summary>
        /// 同步评测里的评论点赞
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(SyncEvltCommentLike))]
        public async Task SyncEvltCommentLike(SyncEvltCommentLikeCommand cmd)
        {
            await _mediator.Send(cmd);
        }

        /// <summary>
        /// 同步评测UV
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost(nameof(SyncEvltUV))]
        public async Task<ResponseResult> SyncEvltUV(SyncEvltUVCommand cmd)
        {
            await _mediator.Send(cmd);
            return ResponseResult.Success(cmd);
        }

        /// <summary>
        /// 同步PV
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost(nameof(SyncPV))]
        public async Task<ResponseResult> SyncPV(SyncPVCommand cmd)
        {
            await _mediator.Send(cmd);
            return ResponseResult.Success(cmd);
        }

        /// <summary>
        /// 同步org PV
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost(nameof(SyncOrgPV))]
        public async Task<ResponseResult> SyncOrgPV(SyncOrgPvCommand cmd)
        {
            await _mediator.Send(cmd);
            return ResponseResult.Success(cmd);
        }

        /// <summary>
        /// 1.1评测自动刷赞
        /// </summary>
        /// <param name="cmd">9,13,17,20,22</param>
        /// <returns></returns>
        [HttpPost(nameof(AutoLikeEvaluation))]
        public async Task<ResponseResult> AutoLikeEvaluation(AutoLikeEvaluationCommand cmd)
        {
            await _mediator.Send(cmd);
            return ResponseResult.Success(true);
        }

        /// <summary>
        /// 1.1自动上下线
        /// </summary>
        /// <param name="cmd">ContentType默认为课程</param>
        /// <returns></returns>
        [HttpPost(nameof(AutoOnlineOrOff))]
        public async Task<ResponseResult> AutoOnlineOrOff(AutoOnlineOrOffCommand cmd)
        {
            await _mediator.Send(cmd);
            return ResponseResult.Success(true);
        }

        /// <summary>
        /// 活动自动下架
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(ActivityAutoOff))]
        public async Task<ResponseResult> ActivityAutoOff()
        {
            await _mediator.Send(new ActivityAutoOffCommand());
            return ResponseResult.Success(true);
        }


        [HttpPost(nameof(ScanLck))]
        public async Task<ResponseResult> ScanLck(ScanLckCommand cmd)
        {
            var r = await _mediator.Send(cmd);
            return r.IsNullOrEmpty() ? ResponseResult.Success(true) : ResponseResult.Failed(r);
        }

        /// <summary>
        /// 检查未支付订单是否过期
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(CheckOrderIsExpired))]
        public async Task<ResponseResult> CheckOrderIsExpired()
        {
            var r = await _mediator.Send(new CheckOrderIsExpiredCommand());
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 同步课程库存
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(UpbackCourseStock))]
        public async Task<ResponseResult> UpbackCourseStock(UpbackCourseStockCommand cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }

        [HttpPost(nameof(FixCourseOrderUnpayToPayed))]
        public async Task<ResponseResult> FixCourseOrderUnpayToPayed(FixCourseOrderUnpayToPayedCommand cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(true);
        }

        /// <summary>
        /// 自动确定收货
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost(nameof(OrderShippedAuto))]
        [ProducesResponseType(typeof(Guid[]), 200)]
        public async Task<ResponseResult> OrderShippedAuto(OrderShippedAutoCmd cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 定时获取未完成的快递单详情(百度)
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost(nameof(ToGetKuaidiDetails))]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> ToGetKuaidiDetails(JobToGetKuaidiDetailsCmd cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(true);
        }

        /// <summary>
        /// 自动取消退货申请
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost(nameof(RefundStatusInSendbackDoAutoExpiredCancel))]
        [ProducesResponseType(typeof(Guid[]), 200)]
        public async Task<ResponseResult> RefundStatusInSendbackDoAutoExpiredCancel(RefundStatusInSendbackDoAutoExpiredCancelCmd cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }
    }
}
