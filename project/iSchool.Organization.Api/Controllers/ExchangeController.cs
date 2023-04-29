using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 兑换管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeController : Controller
    {
        IMediator _mediator;

        public ExchangeController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// 兑换课程【TODO】
        /// </summary>
        /// <param name="request">兑换课程请求实体类</param>
        /// <returns></returns>
        [HttpPost]
        public ResponseResult ExchangeOfCourse([FromBody] ExchangeCourseRequest request)
        {
            var aa = request;
            //todo
            return ResponseResult.Success();
        }
    }
}
