using iSchool.Api.Swagger;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Api.Conventions;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Appliaction;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service;
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
    [Route("api/[controller]")]
    [ApiController]
    public partial class EvaluationsController : ControllerBase
    {
        IMediator _mediator;

        public EvaluationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 查询评测主页.推荐可直接用urlpath 'ls'
        /// </summary>
        /// <param name="subj">科目,多选用","隔开</param>
        /// <param name="age">年龄段,多选用","隔开</param>
        /// <param name="stick">是否推荐，1代表推荐</param>
        /// <returns></returns>        
        [HttpGet("ls/{subj?}")]
        [ProducesResponseType(typeof(EvaluationIndexQueryResult), 200)]
        public async Task<ResponseResult> Index(int stick, [FromRoute] string subj, string age)
        {
            if (subj.IsNullOrEmpty()) subj = "0";
            if (age.IsNullOrEmpty()) age = "0";
            var r = await _mediator.Send(new EvaluationIndexQuery { Subj = subj, Age = age, Stick = stick });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 查询评测主页.推荐可urlpath 'v2/ls/1?'
        /// </summary>
        /// <param name="subj">科目,多选用“，”隔开,不需要科目传下划线'_'或0</param>
        /// <param name="age">年龄段,多选用“，”隔开</param>
        /// <param name="stick">1=只要精华；0或不传=全部</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>        
        [HttpGet("v2/ls/{pageIndex?}")]
        [ProducesResponseType(typeof(EvaluationIndexQueryResult2), 200)]
        public async Task<ResponseResult> Index2(string subj = "_", string age = "0", int stick = 0, int pageIndex = 1)
        {
            if (subj == "_") subj = "0";
            var r = await _mediator.Send(new EvaluationIndexQuery2 { Subj = subj, Age = age, PageIndex = pageIndex, Stick = stick });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 分页查询评测主页下的评测列表
        /// </summary>
        /// <param name="stick">1=只要精华；0或不传=全部</param>
        /// <param name="subj">科目,多选用“，”隔开</param>
        /// <param name="age">年龄段,多选用“，”隔开</param>
        /// <param name="pageIndex">页码</param>        
        /// <returns></returns>
        [HttpGet("ls/{subj}/{pageIndex}")]
        [ProducesResponseType(typeof(EvaluationLoadMoreQueryResult), 200)]
        public async Task<ResponseResult> Ls(int stick = 0, [FromRoute] string subj = "0", string age = "0", int pageIndex = 0)
        {
            var r = await _mediator.Send(new EvaluationLoadMoreQuery
            {
                Stick = stick,
                Subj = subj,
                PageIndex = pageIndex,
                Age = age
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 评测详情
        /// </summary>
        /// <param name="id" type="string">评测短id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EvltDetailDto), 200)]
        public async Task<ResponseResult> GetEvaluation(
            [CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path, Required = true)]
            long id)
        {
            var r = await _mediator.Send(new EvltDetailQuery { No = id });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 点赞评测（含取消）
        /// </summary>
        /// <returns></returns>
        [HttpPost("like")]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> LikeEvaluation(LikeEvaluationCommand cmd)
        {
            var res = await _mediator.Send(cmd);
            if (res)
                return ResponseResult.Success(res);
            else
                return ResponseResult.Failed();
        }

        /// <summary>
        /// 发评测
        /// </summary>
        /// <param name="cmd"></param>        
        /// <returns></returns>
        [HttpPost]
        [Authorize, CheckBindMobile]
        [ProducesResponseType(typeof(EvaluationAddedResult), 200)]
        public async Task<ResponseResult> AddEvaluation(AddEvaluationCommand cmd)
        {
            return null;

            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }
        /// <summary>
        /// 评测编辑获取选项
        /// </summary>
        /// <param name="evaluid"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<ResponseResult> EditEvaluation([FromQuery]Guid evaluid)
        {
            return null;
            var r = await _mediator.Send(new GetEditEvaluationCommand() { Id= evaluid });
            return r;
        }
        /// <summary>
        /// 添加评测时获取枚举
        /// </summary>
        /// <returns></returns>
        [HttpGet("enum")]
        [Authorize]
        [ProducesResponseType(typeof(EvaluationEnumsResult), 200)]
        public ResponseResult AddEvaluation_Enum([FromServices] IConfiguration config)
        {
            config = config.GetSection("AppSettings:addEvltTick");
            var ls = new List<(string, string)>();
            foreach (var cf in config.GetChildren())
            {
                var q = cf["q"];
                var aa = cf.GetSection("a").Get<string[]>();
                var a = aa[new Random().Next(0, aa.Length)];
                ls.Add((q, a));
            }
            return ResponseResult.Success(new EvaluationEnumsResult
            {
                Subject = EnumUtil.GetDescs<SubjectEnum>().Select(_ => (_.Value.ToInt(), _.Desc)),
                AgeGroup = EnumUtil.GetDescs<AgeGroup>().Select(_ => (_.Value.ToInt(), _.Desc)),
                CourceDuration = EnumUtil.GetDescs<CourceDurationEnum>().Select(_ => (_.Value.ToInt(), _.Desc)),
                TeachMode = EnumUtil.GetDescs<TeachModeEnum>().Select(_ => (_.Value.ToInt(), _.Desc)),
                m2s = ls,
            });
        }

        /// <summary>
        /// 为评测添加投票
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("vote")]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ResponseResult> AddVoteTo_a_Evaluation(AddVoteAfterEvaluationAddedCommand cmd)
        {
            if (cmd.Items.Count() < 2)
                return ResponseResult.Failed("投票项必须大于等于二！");
            await _mediator.Send(cmd);
            return ResponseResult.Success(true);
        }

        /// <summary>
        /// 用户投票
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("vote/user")]
        [Authorize]
        [ProducesResponseType(typeof(UserSelectEvltVoteResult[]), 200)]
        public async Task<ResponseResult> UserSelectVote(UserSelectEvltVoteCommand cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 分享评测
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("share")]
        [ProducesResponseType(typeof(ShareLinkDto), 200)]
        public async Task<ResponseResult> Share(ShareEvltCommand cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 我的里面评测列表分页
        /// </summary>        
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">页大小</param>
        ///  <param name="SeeUserId">查看别人的评测,传对应用户的userid，不传表示查我的</param>
        /// <returns></returns>
        [HttpGet("my/{pageIndex}")]
        [Authorize]
        [ProducesResponseType(typeof(LoadMoreResult<MyEvaluationItemDto>), 200)]
        public async Task<ResponseResult> GetMy(Guid? SeeUserId,int pageIndex, int pageSize = 10)
        {
            var r = await _mediator.Send(new MyEvaluationPageQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SeeUserId= SeeUserId
            });
            return ResponseResult.Success(r);
        }


       /// <summary>
       /// 我收藏的评测
       /// </summary>
       /// <param name="pageIndex"></param>
       /// <param name="pageSize"></param>
       /// <returns></returns>
        [HttpGet("ls/collect/{pageIndex}")]
        [Authorize]
        [ProducesResponseType(typeof(EvaluationLoadMoreQueryResult), 200)]
        public async Task<ResponseResult> MyCollectEvaluation(int pageIndex = 0,int pageSize=0)
        {
            var r = await _mediator.Send(new MyCollectEvaluationQuery
            {
                
                PageIndex = pageIndex,
                PageSize = pageSize
            });
            return ResponseResult.Success(r);
        }

        /// <summary>
        /// 我点赞的评测
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("ls/like/{pageIndex}")]
        [Authorize]
        [ProducesResponseType(typeof(EvaluationLoadMoreQueryResult), 200)]
        public async Task<ResponseResult> MyLikeEvaluation(int pageIndex = 0, int pageSize = 0)
        {
            var r = await _mediator.Send(new MyLikeEvaluationQuery
            {

                PageIndex = pageIndex,
                PageSize = pageSize
            });
            return ResponseResult.Success(r);
        }

        #region 收藏评测[All OK]
        /// <summary>
        /// 收藏评测（含取消收藏）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CollectionEvaluation")]
        [Authorize]//需登录验证
        public async Task<ResponseResult> CollectionEvaluation(EvaluationCollectionRequest request)
        {
            //获取用户信息
            var user = HttpContext.RequestServices.GetService<IUserInfo>();
            var r = new EvaluationCollectionCommand()
            {
                UserId = user.UserId,
                EvaluationId = request.EvaluationId
            };
            return await _mediator.Send(r);
        }
        #endregion

        #region 删除评测
        /// <summary>
        /// 删除评测
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        ///   [HttpPost("CollectionEvaluation")]
        [HttpPost("Remove")]
        [Authorize]//需登录验证
        public async Task<ResponseResult> RemoveEvaluation(RemoveEvaluationCommand r)
        {
            return null;
            return await _mediator.Send(r);
        }
        #endregion
    }
}
