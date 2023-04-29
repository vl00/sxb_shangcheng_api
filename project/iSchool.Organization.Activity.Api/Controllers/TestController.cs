using Autofac.Features.Indexed;
using iSchool.Api.Swagger;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Course;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using System.Web;

namespace iSchool.Organization.Activity.Api.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        IMediator _mediator;        
        readonly IRepository<Domain.Organization> _organizationRepository;
        readonly OrgUnitOfWork _orgUnitOfWork;

        public TestController(IMediator mediator,
            IRepository<Domain.Organization> organizationRepository,
            IIndex<string, IBaseRepository<Domain.Organization>> baseRepositorys, IOrgUnitOfWork orgUnitOfWork)
        {
            _mediator = mediator;
            _organizationRepository = organizationRepository;
            var openid_wxRepository = baseRepositorys["Openid_WXBaseRepository"];
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        /// <summary>
        /// 这是测试代码
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(Index))]
        public string Index()
        {

            //_logger.LogDebug(1, "这是调试");
            //_logger.LogWarning("这是警告");
            //_logger.LogError("这是错误");
            //_logger.LogInformation("这是消息");

            return "this is index";
        }

        [Authorize]
        [HttpGet]
        public async Task<string> Get([FromServices] IUserInfo user)
        {
            await Task.CompletedTask;
            //this.HttpContext
            return $"test get {HttpContext.User.Identity.Name}, id={user.UserId}";
        }
        /// <summary>
        /// 测试代码
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetUow")]
        public string GetUow()
        {
            try
            {
                var orgs = _orgUnitOfWork.DbConnection.Query<Domain.Organization>("SELECT TOP 10 * FROM dbo.Organization  WHERE  ages IS NOT NULL");

                _orgUnitOfWork.BeginTransaction();


                throw new Exception("这是一个错误！");
                _orgUnitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {

                _orgUnitOfWork.Rollback();
            }
            return "";
        }


        [HttpGet(nameof(Cbm))]
        [Authorize, CheckBindMobile]
        public int Cbm()
        {
            throw new Exception("cbm");
            //return 123;
        }

        [HttpGet("NO/{id_s}")]
        public ResponseResult<long> NO(string id_s)
        {
            var no = UrlShortIdUtil.Base322Long(id_s);
            return ResponseResult<long>.Success(no);
        }

        [HttpGet("fn1")]
        public ResponseResult<string> A1(
            [FromQuery, ApiDocParameter(typeof(string))] 
            string id)
        {
            return ResponseResult<string>.Success(id);
        }

        [HttpPost("fn_likes")]
        public async Task<ResponseResult> A2(EvltLikesQuery query)
        {
            var r = await _mediator.Send(query);
            return ResponseResult.Success(r);
        }

        [HttpPost("Evaluations")]
        [Authorize, CheckBindMobile]
        [ProducesResponseType(typeof(EvaluationAddedResult), 200)]
        public async Task<ResponseResult> AddEvaluation(AddEvaluationCommand cmd)
        {
            var r = await _mediator.Send(cmd);
            return ResponseResult.Success(r);
        }

        [HttpPost("WeChatOfficialAccount/Reply")]
        public async Task<ResponseResult> WeChatOfficialAccount_Reply([FromQuery] Guid activityId, [FromQuery] string cachekey)
        {
            cachekey = HttpUtility.UrlDecode(cachekey);
            await default(ValueTask);
            return ResponseResult.Success(new 
            {
                activityId,
                cachekey
            });
        }
    }
}
