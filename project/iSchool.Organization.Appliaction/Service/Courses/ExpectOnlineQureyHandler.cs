using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Cache;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Courses;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using iSchool.Infrastructure.Extensions;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 期待上线
    /// </summary>
    public class ExpectOnlineQureyHandler : IRequestHandler<ExpectOnlineQurey, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IHttpClientFactory httpClientFactory;
        CSRedisClient _cSRedis;
        AppSettings appSettings;
        

        public ExpectOnlineQureyHandler(IOrgUnitOfWork unitOfWork
            , IHttpClientFactory httpClientFactory
            , CSRedisClient cSRedis
            , IOptions<AppSettings> options)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.httpClientFactory = httpClientFactory;
            this._cSRedis = cSRedis;
            this.appSettings = options.Value;
        }

        public async Task<ResponseResult> Handle(ExpectOnlineQurey request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            if (0 == request.Type)//期待上线逻辑
            {
                #region Key

                //用户订阅课程状态
                string key_SubscribeStatus = CacheKeys.SubscribeStatus.FormatWith(request.UserInfo.UserId, request.CourseId);

                //课程订阅总数
                string key_CourseSubscribeCount = CacheKeys.CourseSubscribeCount.FormatWith(request.CourseId);

                #endregion

            #region 课程订阅总数相关信息subData
            var subData = _cSRedis.Get<ExpectOnlineResponse>(key_CourseSubscribeCount);
            string resSql = $@" select c.id,c.no,c.name ,'' as QRCode,o.name as OrgName,'true' as Status, ceiling(Subscribe*1.2+89) as Subscribe from [dbo].[Course]  c left join [dbo].[Organization] o on c.orgid=o.id where  c.id=@courseid and  c.status=1 and o.status=1  ;";
            if (subData == null)
            {
                subData = _orgUnitOfWork.DbConnection.Query<ExpectOnlineResponse>(resSql, new DynamicParameters().Set("courseid", request.CourseId)).FirstOrDefault();
                subData.No= UrlShortIdUtil.Long2Base32(Convert.ToInt64(subData.No));
                if (subData == null) throw new CustomResponseException("课程不存在");
                _cSRedis.Set(key_CourseSubscribeCount, subData);
            } 
            #endregion

                var subscribeStatus = _cSRedis.Get<bool>(key_SubscribeStatus);//订阅状态

                if (subscribeStatus) subData.QRCode = ""; //已订阅
                else//未订阅
                {
                    subData.Status = false;
                    #region 二维码
                    string wxkey = string.Format(CacheKeys.gzhbackinfo, request.CourseId);
                    using var httpClient = httpClientFactory.CreateClient(string.Empty);

                    #region 通过阿喜的api获取AccessToken
                    var getUrl = appSettings.AccessTokenApi.FormatWith(appSettings.WXServiceNumberToken.Split('_').LastOrDefault());
                    var res_accesstoken = await httpClient.GetAsync(getUrl);
                    res_accesstoken.EnsureSuccessStatusCode();
                    var r_accesstoken = (await res_accesstoken.Content.ReadAsStringAsync()).ToObject<AccessTokenApiResult>(true);
                    var token = "";
                    if (r_accesstoken.success)
                        token = r_accesstoken.data.Token;
                    else
                        throw new CustomResponseException("获取AccessToken失败：" + r_accesstoken);
                    #endregion

                    //var token = wXOrgUnitOfWork.DbConnection.Query<string>($" SELECT [value] FROM [iSchool].dbo.keyValue WHERE [KEY]='{appSettings.WXServiceNumberToken}' ;").FirstOrDefault();
                    var postJson = "{\"expire_seconds\": " + 30 * 24 * 60 * 60 + ", \"action_name\": \"QR_STR_SCENE\", \"action_info\": {\"scene\": {\"scene_str\": \"" + wxkey + "\"}}}";

                    var res = await httpClient.PostAsync(string.Format(appSettings.CreateQRCodeUrl, token), new StringContent(postJson));
                    res.EnsureSuccessStatusCode();
                    var r = (await res.Content.ReadAsStringAsync()).ToObject<WXResult>(true);
                    if (!res.IsSuccessStatusCode)
                    {
                        return ResponseResult.Failed("无法获取二维码");
                    }

                    //并把回调api相关信息存入redis
                    var backUrl = $"{request.ApiUrl}/api/Courses/SubscribeCourse?courseId={request.CourseId}&username={request.UserInfo.UserName}";
                    _cSRedis.Set(wxkey, backUrl);
                    #endregion
                    subData.QRCode = string.Format(appSettings.GetQRCodeUrl, HttpUtility.UrlEncode(r.ticket));
                }


                _cSRedis.Set(key_SubscribeStatus, subData.Status, 60 * 60 * 24 * 30);//用户订阅课程状态入缓存
                return ResponseResult.Success(subData);
            }
            else if (1 == request.Type)//课程购买逻辑
            {
                ExpectOnlineResponse buyR = new ExpectOnlineResponse();
                #region 二维码
                string wxkey_coursebuy = string.Format(CacheKeys.gzhbackinfo_coursebuy, request.CourseId,DateTime.Now.Ticks);
                using var httpClient = httpClientFactory.CreateClient(string.Empty);

                #region 通过阿喜的api获取AccessToken
                var getUrl = appSettings.AccessTokenApi.FormatWith(appSettings.WXServiceNumberToken.Split('_').LastOrDefault());
                var res_accesstoken = await httpClient.GetAsync(getUrl);
                res_accesstoken.EnsureSuccessStatusCode();
                var r_accesstoken = (await res_accesstoken.Content.ReadAsStringAsync()).ToObject<AccessTokenApiResult>(true);
                var token = "";
                if (r_accesstoken.success)
                    token = r_accesstoken.data.Token;
                else
                    throw new CustomResponseException("获取AccessToken失败：" + r_accesstoken);
                #endregion

                //var token = wXOrgUnitOfWork.DbConnection.Query<string>($" SELECT [value] FROM [iSchool].dbo.keyValue WHERE [KEY]='{appSettings.WXServiceNumberToken}' ;").FirstOrDefault();
                var postJson = "{\"expire_seconds\": " + 30 * 24 * 60 * 60 + ", \"action_name\": \"QR_STR_SCENE\", \"action_info\": {\"scene\": {\"scene_str\": \"" + wxkey_coursebuy + "\"}}}";

                var res = await httpClient.PostAsync(string.Format(appSettings.CreateQRCodeUrl, token), new StringContent(postJson));
                res.EnsureSuccessStatusCode();
                var r = (await res.Content.ReadAsStringAsync()).ToObject<WXResult>(true);
                if (!res.IsSuccessStatusCode)
                {
                    return ResponseResult.Failed("无法获取二维码");
                }

                //并把回调api相关信息存入redis
                var backUrl = $"{request.ApiUrl}/api/Courses/SubscribeCourse?courseId={request.CourseId}&username={request.UserInfo.UserName}&type=1";
                _cSRedis.Set(wxkey_coursebuy, backUrl);
                #endregion
                buyR.QRCode = string.Format(appSettings.GetQRCodeUrl, HttpUtility.UrlEncode(r.ticket));
                return ResponseResult.Success(buyR);
            }
            else
            {
                return ResponseResult.Failed("参数错误_type");
            }
           
        }
    }

    public class WXResult
    {
        public string ticket { get; set; }
        public int expire_seconds { get; set; }
        public string url { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AccessTokenApiResult
    {
        public AccessTokenApiData data { get; set; }
        public bool success { get; set; }
        public int status { get; set; }
        public string msg { get; set; }
    }

    public class AccessTokenApiData
    {
        public string AppID { get; set; }
        public string AppName { get; set; }
        public string Token { get; set; }
    }
}



