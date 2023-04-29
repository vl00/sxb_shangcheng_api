using System;
using System.Collections.Generic;
using iSchool.Organization.Api.Conventions;
using iSchool.Api.Swagger;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Course;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using iSchool.Organization.Appliaction.ResponseModels.Courses;
using System.IO;
using iSchool.Organization.Api.Filters;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.OrgService_bg.Course;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager;
using iSchool.Organization.Appliaction.OrgService_bg.RedeemCodes;
using iSchool.Organization.Appliaction.RequestModels.Orders;
using iSchool.Organization.Appliaction.ResponseModels.Orders;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 课程管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : Controller
    {
        IMediator _mediator;        
        public CoursesController(IMediator mediator)
        {
            _mediator = mediator;            
        }

        #region 课程列表 [ALL OK]
        /// <summary>
        /// [课程中心]
        /// </summary>        
        /// <param name="subjectId">科目Id</param>
        /// <param name="ageGroupId">年龄段Id</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小(默认加载15条，往后每次加载10条[需传入页大小])</param>
        /// <param name="isAuth">是否认证课程</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CoursesByOrgIdQueryResponse>), 200)]
        public ResponseResult GetCoursesByInfo(int? subjectId, int? ageGroupId,int? isAuth, int pageIndex=1, int pageSize=15 )
        {
            var request = new CoursesByInfoQuery
            {
                PageInfo = new PageInfo() { PageIndex = pageIndex, PageSize = pageSize },
                AgeGroupId = ageGroupId,
                SubjectId = subjectId,
                isAuth = isAuth
            };
            return _mediator.Send(request).Result;

        }

        /// <summary>
        /// 选择评测品牌--[相关课程]
        /// </summary>
        /// <param name="orgId">机构Id</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>课程列表</returns>
        [HttpGet("Organization/{orgId}")]
        [ProducesResponseType(typeof(List<CoursesByOrgIdQueryResponse>), 200)]
        public ResponseResult GetCoursesByOrganizationId(Guid orgId, int pageIndex=1, int pageSize=10)
        {
            return _mediator.Send(new CoursesByOrgIdQuery
            {
                PageInfo = new PageInfo() { PageIndex = pageIndex, PageSize = pageSize },
                OrgId = orgId
            }).Result;
        }

        #endregion

        #region 收藏课程[All OK]
        /// <summary>
        /// 收藏课程（含取消收藏）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CollectionCourse")]
        [Authorize, CheckBindMobile]//需登录验证
        public ResponseResult CollectionCourse(AddOrCancelCollectionRequest request)
        {
            //获取用户信息
            var user = HttpContext.RequestServices.GetService<IUserInfo>();
            var r = new AddOrCancelCollectionCommand()
            {
                UserId = user.UserId,                
                CourseId = request.CourseId
            };
            return _mediator.Send(r).Result;
        }
        #endregion

        #region 订阅课程[All OK]

        /// <summary>
        /// [公众号回调]用户关注公众号后回调，此时把把订阅信息入库到订阅表
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="username"></param>
        /// <param name="OpenID"></param>
        /// <param name="type">回调的类型 0 期待上线 1 购买课程</param>
        /// <returns></returns>
        [HttpPost("SubscribeCourse")]
        public ResponseResult SubscribeCourse([FromQuery]Guid courseId,string username, [FromForm]string OpenID,int type= 0) 
        {
            SubscribeCourseAdd request = new SubscribeCourseAdd()
            {
                CourseId = courseId,
                OpenID = OpenID,
                UserName = username,
                Type=type
            };
            
            return _mediator.Send(request).Result;
        }

        /// <summary>
        /// [期待上线]（备注：未订阅--展示公众号信息界面、已订阅--展示订阅后信息界面）
        /// </summary>
        /// <param name="courseId">课程Id</param>
        ///  <param name="type">类型 0未认证课程预约 1认证课程购买成功</param>
        /// <returns></returns>
        [HttpGet("ExpectOnline")]
        [Authorize, CheckBindMobile]//需登录验证
        [ProducesResponseType(typeof(ExpectOnlineResponse), 200)]
        public ResponseResult ExpectOnline(Guid courseId,int type)
        {
            //获取用户信息
            var user = HttpContext.RequestServices.GetService<IUserInfo>();
            var apiUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            return _mediator.Send(new ExpectOnlineQurey() { ApiUrl=apiUrl, CourseId=courseId, UserInfo= user,Type=type }).Result;
        }

        #endregion

        #region 课程详情
        /// <summary>
        /// 课程详情
        /// </summary>
        /// <param name="no">课程短Id</param>
        /// <returns></returns>
        [HttpGet("{no}")]
        [ProducesResponseType(typeof(CourseDetailsResponse), 200)]
        public async Task<ResponseResult> GetCourseDetailsById(
             [CommaConv, ApiDocParameter(typeof(string), In = ParameterLocation.Path)]
            long no)
        {           
            return await _mediator.Send(new CourseDetailsByIdQuery() { No = no });
        }

        #endregion

        #region 分享api--ShareLinkDto
        /// <summary>
        /// [课程详情分享]-[需要登录]
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("share")]
        [ProducesResponseType(typeof(ShareLinkDto), 200)]
        [Authorize, CheckBindMobile]
        public async Task<ResponseResult> ShareCourse(ShareCourseRequest request)
        {
            return await _mediator.Send(new ShareCourseQuery()
            {
                Id = request.Id,
                Cnl = request.Cnl,
                UserInfo = HttpContext.RequestServices.GetService<IUserInfo>(),
                FxHeaducode = request.FxHeaducode,
            });
        }
        #endregion

        #region 购买课程
        /// <summary>
        /// 购买课程
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CourseBook")]
        [Authorize]
        public ResponseResult CourseBook(CourseBookRequest request)
        {
            return _mediator.Send(new CourseBookCommand() { VerifyCode = request.VerifyCode, Mobile = request.Mobile, CourseId = request.CourseId,Remark=request.Remark }).Result;
        }
        #endregion

        #region 根据体验课Id,查询大课列表
        /// <summary>
        /// 【课程分销后台专用】大课信息
        /// </summary>
        /// <param name="courseId">体验课Id</param>
        /// <returns></returns>
        [HttpGet("bigcourse/{courseId}")]
        [ProducesResponseType(typeof(List<BigCourseResponse>), 200)]
        public async Task<ResponseResult> GetBigCourseById(Guid courseId)
        {
            var response = await _mediator.Send(new BigCourseByIdQuery() { CourseId = courseId });
            return ResponseResult.Success(response);
        }
        #endregion


        #region 临时使用
        /// <summary>
        /// 【临时使用】根据课程Id，获取兑换码兑换记录（CourseId=BA451764-34EC-4CC0-BE72-44E506B463D0）
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("getexchanges")]
        [ProducesResponseType(typeof(PagedList<ExchangeDto>), 200)]
        public IActionResult ExchangeList(Guid courseId,int pageIndex,int pageSize)
        {
            var request = new SearchExchangesQuery()
            {
                CourseId = courseId,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            var result = _mediator.Send(request).Result;
            return Json(result);
        }


        /// <summary>
        /// 展示模板内容及兑换码信息 （CourseId=BA451764-34EC-4CC0-BE72-44E506B463D0）
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <returns></returns>
        [HttpGet("showMsgTemplate")]
        [ProducesResponseType(typeof(MsgAndDHCodeDto), 200)]
        public IActionResult ShowMsgTemplateAndDHCodeInfo(Guid courseId)
        {

            var request = new ShowMsgDHCodeQuery()
            {
                CourseId = courseId
            };
            var result = _mediator.Send(request).Result;
            return Json(result);

        }

        /// <summary>
        /// (新增/编辑)保存模板 （CourseId=BA451764-34EC-4CC0-BE72-44E506B463D0）
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("savemsg")]
        [ProducesResponseType(typeof(ResponseResult), 200)]
        public IActionResult SaveMsgTemplate(SaveMsgTemplateCommand cmd)
        {

            var result = _mediator.Send(cmd).Result;
            return Json(result);

        }

        /// <summary>
        /// 导出兑换记录列表到excel
        /// </summary>
        [HttpGet("export")]

        public async Task<IActionResult> ExportExchange(Guid cid)
        {
            var id = await _mediator.Send(new ExportExchangesCommand() { CourseId = cid });
            return Json(string.IsNullOrEmpty(id) ? ResponseResult.Failed("系统繁忙") : ResponseResult.Success(id, null));
        }

        /// <summary>
        /// excel导入兑换码
        /// </summary>
        /// <cid>课程Id</cid>
        /// <returns></returns>
        [HttpPost("insertdb")]
        public async Task<IActionResult> AddRedeemCodeFromExcel(Guid cid)
        {
            var excelfile = HttpContext.Request.Form.Files[0];

            if (excelfile.Length > 1024.0 * 1024 * 100) // > 100M
            {
                return Json(new
                {
                    isOk = false,
                    msg = "excel超过100M了, 请分割",
                });
            }
            try
            {
                var uid = HttpContext.RequestServices.GetService<IUserInfo>().UserId;

                using (var excel = new MemoryStream((int)excelfile.Length))
                {
                    await excelfile.CopyToAsync(excel);

                    await _mediator.Send(new ExcelImportRedeemCodeCommand { Excel = excel, UserId = uid, CourseId = cid });

                    return Json(new { isOk = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isOk = false,
                    msg = ex.Message,
                });
            }
        }

        ///// <summary>
        ///// 释放订单【占用】的兑换码
        ///// 【占用:订单只是绑定了兑换码，尚未发送】
        ///// </summary>
        ///// <param name="cid"></param>
        ///// <returns></returns>
        //[HttpPost("release")]
        //public async Task<IActionResult> ReleaseOccupiedCode(Guid cid)
        //{
        //    var result =_mediator.Send(new ReleaseOccupiedCodeCommand() { CourseId = cid, }).Result;
        //    return Json(result);
        //}

        /// <summary>
        /// 根据订单Id，获取订单详情
        /// </summary>
        /// <param name="ordId">订单Id</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(OrderDetailsDto), 200)]
        [HttpGet("orderdetails")]
        public IActionResult GetOrderDetails(Guid ordId)
        {
            var result = _mediator.Send(new OrderDetailsByOrdIdQuery() { OrderId = ordId }).Result;
            return Json(result);
        }


        #endregion


    }
}
