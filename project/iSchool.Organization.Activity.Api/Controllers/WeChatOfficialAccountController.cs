using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Organization.Activity.Appliaction.Service.WeChat;
using iSchool.Organization.Appliaction.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MediatR;
using System.Web;

namespace iSchool.Organization.Activity.Api.Controllers
{
    /// <summary>
    /// 微信公众号管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WeChatOfficialAccountController : Controller
    {
        IMediator _mediator;
        public WeChatOfficialAccountController(IMediator _mediator) 
        {
            this._mediator = _mediator;
        }

        #region 回调-关注回复
        /// <summary>
        /// [关注回复]用户关注公众号回复小助手二维码
        /// </summary>
        /// <param name="activityId">活动Id</param>
        /// <param name="cachekey">缓存Key</param>
        /// <param name="OpenID">用户公众号中的OpenID</param>
        /// <returns></returns>
        [HttpPost("Reply")]
        public ResponseResult Reply([FromQuery] Guid activityId, [FromQuery]string cachekey, [FromForm] string OpenID)
        {
            
            ReplyCommand request = new ReplyCommand()
            {
                ActivityId = activityId,
                OpenID = OpenID,
                CacheKey= HttpUtility.UrlDecode(cachekey)
            };
            return _mediator.Send(request).Result;            
        }
        #endregion



    }
}
