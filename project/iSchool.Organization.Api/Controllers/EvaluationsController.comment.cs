using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service;
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
    public partial class EvaluationsController : ControllerBase
    {
        /// <summary>
        /// 评论点赞（含取消）
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("comment/like")]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> LikeEvaluationComment(LikeEvaluationCommentCommand cmd)
        {
            await _mediator.Send(cmd);
            return ResponseResult.Success(true);
        }

        /// <summary>
        /// 添加评论 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("comment")]
        [Authorize, CheckBindMobile]
        [ProducesResponseType(typeof(AddEvltCommentDto), 200)]
        public async Task<ResponseResult> AddEvaluationComment([FromBody]AddEvltCommentCommand cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 分页查询某个评测里的评论列表
        /// </summary>
        /// <param name="id">评测id</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小，默认10</param>
        /// <returns></returns>
        [HttpGet("{id}/comments/{pageIndex}")]
        [ProducesResponseType(typeof(PagedList<EvaluationCommentDto>), 200)]
        public async Task<ResponseResult> GetEvaluationComments(Guid id, int pageIndex, int pageSize = 10)
        {
            var r = await _mediator.Send(new EvltCommentsQuery
            {
                EvltId = id,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Naf = DateTime.Now,
            });
            return ResponseResult.Success(r);
        } 
        /// <summary>
        /// 分页查询某个评测里的子回复列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("comments/child")]
        [ProducesResponseType(typeof(PagedList<EvaluationCommentDto>), 200)]
        public async Task<ResponseResult> GetEvaluationChildrenComments([FromQuery] ChildrenCommentsQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }
        /// <summary>
        /// 评论详情页
        /// </summary>
        /// <param name="id">评测评论id</param>
        /// <returns></returns>
        [HttpGet("comments/detail")]
        [ProducesResponseType(typeof(EvaluationCommentDto), 200)]
        public async Task<ResponseResult> GetEvaluationCommentsDetail(Guid id)
        {
            var r = await _mediator.Send(new EvltCommentDetailQuery
            {
                EvltCmtId = id,

            });
            return ResponseResult.Success(r);

        }
        /// <summary>
        /// 我的评论
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小，默认10</param>
        /// <returns></returns>
        [HttpGet("comments/my")]
        [Authorize]
        [ProducesResponseType(typeof(EvaluationLoadMoreQueryResult), 200)]
        public async Task<ResponseResult> GetMyComment(int pageIndex = 0, int pageSize = 0)
        {

            var r = await _mediator.Send(new MyEvaluationCommentQuery
            {

                PageIndex = pageIndex,
                PageSize = pageSize
            });
            return ResponseResult.Success(r);

        }

        #region 删除评论
        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
     
        [HttpPost("comments/remove")]
        [Authorize]//需登录验证
        public ResponseResult RemoveEvaluationComment(RemoveEvaluationCommentCommand r)
        {

            return _mediator.Send(r).Result;
        }
        #endregion
    }
}
