using CSRedis;
using Dapper;
using EasyWeChat.Model;
using Enyim.Caching;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Courses;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyWeChat.Interface;
namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 订阅课程--公众号回调
    /// </summary>
    public class SubscribeCourseAddHandler : IRequestHandler<SubscribeCourseAdd, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IHttpClientFactory httpClientFactory;
        AppSettings appSettings;
        Openid_WXOrgUnitOfWork _openid_WXOrgUnitOfWork;
        IRepository<iSchool.Organization.Domain.Course> _courseRepo;
        ITemplateMessageService _templateMessageService;

        public SubscribeCourseAddHandler(IOrgUnitOfWork unitOfWork, IWXUnitOfWork wXUnitOfWork
            , CSRedisClient redisClient
            , IHttpClientFactory httpClientFactory
            , IOptions<AppSettings> options
            , IOpenid_WXUnitOfWork openid_WXOrgUnitOfWork
            , IRepository<iSchool.Organization.Domain.Course> courseRepo
            , ITemplateMessageService templateMessageService)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            this.httpClientFactory = httpClientFactory;
            this.appSettings = options.Value;
            _openid_WXOrgUnitOfWork = (Openid_WXOrgUnitOfWork)openid_WXOrgUnitOfWork;
            _courseRepo = courseRepo;
            _templateMessageService = templateMessageService;
        }


        public async Task<ResponseResult> Handle(SubscribeCourseAdd request, CancellationToken cancellationToken)
        {     
            try
            {
                if (0 == request.Type)
                {
                    //获取订阅课程的用户信息及公众号信息
                    string wxsql = $"select * from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and openID='{request.OpenID}'  ";
                    #region 微信用户信息，每个环境都不一样库
                    WXInfo wxInfo = _openid_WXOrgUnitOfWork.DbConnection.Query<WXInfo>(wxsql).FirstOrDefault();
                    #endregion

                    #region 通过阿喜的api获取AccessToken
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
                    if (wxInfo == null) throw new CustomResponseException("公众号信息不存在！");
                    else
                    {


                        //wxInfo.UserID = request.UserId;//用户Id
                        var dy = new DynamicParameters();
                        dy.Add("@id", Guid.NewGuid());
                        dy.Add("@courseid", request.CourseId);
                        dy.Add("@userid", wxInfo.UserID);
                        dy.Add("@CreateTime", DateTime.Now);
                        dy.Add("@IsValid", true);

                        #region 订阅
                        _orgUnitOfWork.BeginTransaction();

                        //1、课程表的订阅数+1；
                        string updateSql = $@" update [dbo].[Course] set subscribe+=1  where id=@courseid ";

                        var count = _orgUnitOfWork.DbConnection.Execute(updateSql, dy, _orgUnitOfWork.DbTransaction);


                        if (count == 1)//课程存在
                        {
                            //2、Subscribe订阅表增加一条记录
                            string sql = $@"insert into  [dbo].[Subscribe] ([id], [courseid], [userid], [CreateTime], [IsValid])
                                values(@id, @courseid, @userid, @CreateTime, @IsValid)";

                            count += _orgUnitOfWork.DbConnection.Execute(sql, dy, _orgUnitOfWork.DbTransaction);

                        }
                        else
                        {
                            throw new CustomResponseException("课程不存在");
                        }

                        _redisClient.Del(_redisClient.Keys(CacheKeys.CourseDetails.FormatWith(request.CourseId)));//清除课程详情缓存
                        _redisClient.Del(_redisClient.Keys(CacheKeys.Del_Courses.FormatWith("*")));//清除课程相关缓存
                        _redisClient.Del(_redisClient.Keys(CacheKeys.SubscribeList.FormatWith("*", "*")));//清除课程相关缓存               

                    //获取课程订阅信息，入缓存
                    string subSql = $@" select c.id,c.no,c.name ,'' as QRCode,o.name as OrgName,'true' as Status, ceiling(Subscribe*1.2+89) as Subscribe from [dbo].[Course]  c left join [dbo].[Organization] o on c.orgid=o.id where  c.id=@courseid  and  c.status=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} ;";
                    var subData = _orgUnitOfWork.DbConnection.Query<ExpectOnlineResponse>(subSql, dy, _orgUnitOfWork.DbTransaction).FirstOrDefault();
                    subData.No = UrlShortIdUtil.Long2Base32(Convert.ToInt64(subData.No));
                    _redisClient.Set(CacheKeys.CourseSubscribeCount.FormatWith(request.CourseId), subData);
                    var statusKey = CacheKeys.SubscribeStatus.FormatWith(wxInfo.UserID, request.CourseId);
                    _redisClient.Set(statusKey, true);

                        _orgUnitOfWork.CommitChanges();

                        #endregion

                        #region 向公众号推送模板消息
                        //using var httpClient = httpClientFactory.CreateClient(string.Empty);

                        var message = new SendTemplateRequest(wxInfo.OpenID, appSettings.TemplateId);
                        message.Url = appSettings.WXCourseDetialUrl.FormatWith(subData.No);
                        message.SetData(new TemplateDataFiled[] {
                 new  TemplateDataFiled(){
                    Filed = "first",
                    Value = "您已成本订阅课程提醒服务，课程上线后，我们将通过消息推送，向您及时分享课程信息。",

                 },
                 new TemplateDataFiled(){
                    Filed = "keyword1",
                    Value = @$"{subData.Name}课程订阅服务"
                 },
                 new TemplateDataFiled(){
                    Filed="keyword2",
                    Value="课程暂未上线",
                 },
                 new TemplateDataFiled(){
                    Filed="remark.DATA",
                    Value="点击下方【查看详情】查看更多课程👇",
                 }

                });

                        var response = await _templateMessageService.SendAsync(accessToken, message);
                        if (response.errcode != ResponseCodeEnum.success)
                            throw new CustomResponseException($"发送模板消息失败：{response.errmsg}");
                        else //清除回调缓存
                        {
                            string wxrkey = string.Format(CacheKeys.gzhbackinfo, request.CourseId);
                            _redisClient.Del(wxrkey);
                        }
                        #endregion

                        return await Task.FromResult(ResponseResult.Success("订阅信息已入库！"));
                    }
                }
                else if (1 == request.Type)
                {
                    #region 通过阿喜的api获取AccessToken
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
                    #region 向公众号推送模板消息
                    //using var httpClient = httpClientFactory.CreateClient(string.Empty);
                    var message = new SendTemplateRequest(request.OpenID, appSettings.CourseBookkWechatTemplateId);
                    var courseM = _courseRepo.Get(request.CourseId);
                    if (null == courseM) throw new CustomResponseException("购买课程参数回调错误：courseid=" + request.CourseId);
                    var course_short_no= UrlShortIdUtil.Long2Base32(Convert.ToInt64(courseM.No));
                    message.Url = appSettings.WXCourseDetialUrl.FormatWith(course_short_no);
                    message.SetData(new TemplateDataFiled[] {
                 new  TemplateDataFiled(){
                    Filed = "first",
                    Value = "恭喜您报名成功！",

                 },
                 new TemplateDataFiled(){
                    Filed = "keyword1",
                    Value ="报名成功"
                 },
                 new TemplateDataFiled(){
                    Filed="keyword2",
                    Value=@$"《{courseM.Title }》",
                 },
                 new TemplateDataFiled(){
                    Filed="keyword3",
                    Value="48小时内工作人员会联系你，点击下方【查看详情】查看课程内容。",
                 },
                 new TemplateDataFiled(){
                    Filed="remark.DATA",
                    Value="48小时内工作人员会联系你，点击下方【查看详情】查看课程内容。",
                 }

                });

                    var response = await _templateMessageService.SendAsync(accessToken,message);
                    if (response.errcode != ResponseCodeEnum.success)
                        throw new CustomResponseException($"发送模板消息失败：{response.errmsg}");
                    else //清除回调缓存
                    {
                        string wxrkey = string.Format(CacheKeys.gzhbackinfo, request.CourseId);
                        _redisClient.Del(wxrkey);
                    }
                    #endregion
                    return await Task.FromResult(ResponseResult.Success("已向购买课程者推送通知！"));
                }
                else {

                    return await Task.FromResult(ResponseResult.Failed("参数有误_type！"));
                }

            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return await Task.FromResult(ResponseResult.Failed($"订阅信息入库失败！{ex.Message}"));
            }            
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class WXInfo 
    {
        /// <summary>
        /// 用户微信Id
        /// </summary>
        public string OpenID { get; set; }

        /// <summary>
        /// 公众号名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserID { get; set; }
    }

}
