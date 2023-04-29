using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Course;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Activity.Appliaction.Service.WeChat
{
    
    /// <summary>
    /// 关注回复--公众号回调
    /// </summary>
    public class ReplyCommandHandler : IRequestHandler<ReplyCommand, ResponseResult>
    {
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IHttpClientFactory httpClientFactory;
        AppSettings appSettings;
        Openid_WXOrgUnitOfWork _openid_WXOrgUnitOfWork;


        public ReplyCommandHandler(IOrgUnitOfWork unitOfWork, IWXUnitOfWork wXUnitOfWork
            , CSRedisClient redisClient
            , IHttpClientFactory httpClientFactory
            , IOptions<AppSettings> options
            , IOpenid_WXUnitOfWork openid_WXOrgUnitOfWork
            ,IMediator _mediator
            )
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            this.httpClientFactory = httpClientFactory;
            this.appSettings = options.Value;
            _openid_WXOrgUnitOfWork = (Openid_WXOrgUnitOfWork)openid_WXOrgUnitOfWork;
            this._mediator = _mediator;
        }



        public async Task<ResponseResult> Handle(ReplyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //校验是否是本次活动
                if (request.ActivityId.ToString() != Consts.Activity1_Guid)
                {
                    return await Task.FromResult(ResponseResult.Failed("活动Id错误!"));
                }
                //校验是否在活动期内
                var checkActivity = await _mediator.Send(new ActivitySimpleInfoQuery() { Id = request.ActivityId });
                if(checkActivity?.CheckIfNotValid() == 0)
                {
                    var callbackUrl = _redisClient.Get(request.CacheKey);

                    //缓存不存在，则代表已经回复，不再重复回复
                    if (string.IsNullOrEmpty(callbackUrl))
                    {
                        return await Task.FromResult(ResponseResult.Success("已自动回复小助手微信二维码!"));
                    }

                    #region 通过阿喜的api获取-accessToken
                    using var httpClient = httpClientFactory.CreateClient(string.Empty);
                    var getUrl = appSettings.AccessTokenApi.FormatWith(appSettings.WXServiceNumberToken.Split('_').LastOrDefault());
                    var res_accesstoken = await httpClient.GetAsync(getUrl);
                    res_accesstoken.EnsureSuccessStatusCode();
                    var r_accesstoken = (await res_accesstoken.Content.ReadAsStringAsync()).ToObject<AccessTokenApiResult>(true);
                    var accessToken = "";
                    if (r_accesstoken.success)
                        accessToken = r_accesstoken.data.Token;
                    else
                        throw new CustomResponseException("获取AccessToken失败：" + r_accesstoken);
                    #endregion

                    #region reply image message
                    var msgUrl = appSettings.CustomerServiceAPI.FormatWith(accessToken);
                    string postJson = "{\"touser\":\"" + request.OpenID + "\",\"msgtype\":\"image\",\"image\":{\"media_id\":\"" + appSettings.Media_Id + "\"}}";
                    var res_reply = await httpClient.PostAsync(msgUrl, new StringContent(postJson));
                    res_reply.EnsureSuccessStatusCode();
                    var r_reply = (await res_reply.Content.ReadAsStringAsync()).ToObject<CustomerServiceAPIResult>(true);
                    if (r_reply.ErrCode != 0)
                        throw new CustomResponseException("关注后自动回复失败：" + r_reply.ErrMsg);
                    #endregion

                    //delete callbackurl of redis ， delete here only
                    _redisClient.Del(_redisClient.Keys(request.CacheKey));
                    return await Task.FromResult(ResponseResult.Success("已自动回复小助手微信二维码!"));
                }
                else
                {
                    return await Task.FromResult(ResponseResult.Failed($"非活动期间！"));
                }                
                
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return await Task.FromResult(ResponseResult.Failed($"关注上学帮自动回复失败：{ex.Message}"));
            }
        }
    }

    /// <summary>
    /// 客服消息接口返回的实体
    /// </summary>
    public class CustomerServiceAPIResult
    {
        //{"errcode":0,"errmsg":"ok"}
        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }
    }

}
