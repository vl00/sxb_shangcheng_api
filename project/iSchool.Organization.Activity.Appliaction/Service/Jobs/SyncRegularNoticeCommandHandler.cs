using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Activity.Appliaction.ResponseModels.Jobs;
using iSchool.Organization.Activity.Appliaction.ResponseModels.WeChat;
using iSchool.Organization.Appliaction.Service.Course;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeChat;
using WeChat.Model;

namespace iSchool.Organization.Activity.Appliaction.Service.Jobs
{
    /// <summary>
    /// 定期提醒--用户测评点赞数
    /// </summary>
    public class SyncRegularNoticeCommandHandler : IRequestHandler<SyncRegularNoticeCommand>
    {
        AppSettings appSettings;
        OrgUnitOfWork unitOfWork;
        Openid_WXOrgUnitOfWork openid_WXOrgUnitOfWork;
        CSRedisClient redis;
        IHttpClientFactory httpClientFactory;

        public SyncRegularNoticeCommandHandler(
             IOrgUnitOfWork unitOfWork
            ,IOptions<AppSettings> options
            ,CSRedisClient redis
            ,IOpenid_WXUnitOfWork openid_WXOrgUnitOfWork
            ,IHttpClientFactory httpClientFactory
            )
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.openid_WXOrgUnitOfWork =(Openid_WXOrgUnitOfWork)openid_WXOrgUnitOfWork;
            this.appSettings = options.Value;
            this.redis = redis;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<Unit> Handle(SyncRegularNoticeCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string appName = appSettings.WXServiceNumberToken.Split('_').LastOrDefault();

            //get evlt users
            string evlt_Sql = $@" select aty.title as activitytitle,e.title evlttitle,aty.endtime,e.id as EvaluationId,e.no as EvaluationNo,
                                 ank.top_no as ranking, ank.likecount as likes,ank.userid from [dbo].[Evaluation] e
                                 left join ActivityUserEvltLikeRank ank on e.id=ank.evaluationid and e.IsValid=1
                                 left join Activity aty on ank.activityid=aty.id and ank.IsValid=1
                                 where  aty.IsValid=1 and aty.id=@activityId
                                 order by ranking  ; ";            

            //当天待发送用户
            var toSendActEvltNotices = unitOfWork.DbConnection.Query<ActivityEvltNoticeDto>(evlt_Sql, new DynamicParameters().Set("activityId", new Guid(Consts.Activity1_Guid))).ToList();
            var userIds = string.Join("','", toSendActEvltNotices?.Select(_ => _.UserId).ToList());

            //get  users by app when user release evlt--获取公众号中，发过评测的用户
            string openIds_Sql = $@" select openID,userID from [dbo].[openid_weixin] where valid=1 and appName=@appName and  userID in ('{userIds}') ";
            var wxUsers = openid_WXOrgUnitOfWork.DbConnection.Query<OpenidModel>(openIds_Sql, new DynamicParameters().Set("appName", appName));

            var wxuserIds = wxUsers.Select(_ => _.UserId);
            toSendActEvltNotices = toSendActEvltNotices?.Where(_ => wxuserIds.Contains(_.UserId)).OrderBy(_=>_.Ranking).ToList();

            //定期通知表记录
            var oldSendActivityEvltNotices = unitOfWork.DbConnection.Query<ActivityEvltNotice>("Select * from  [dbo].[ActivityEvltNotice] ");

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

            int Ranking = 1;//排名

            //当天已发送过通知的用户
            var currentSentUsers = new List<Guid>();



            for (int i = 0; i < toSendActEvltNotices?.Count; i++)
            {
                var toSendUser = toSendActEvltNotices[i];
                Ranking = toSendUser.Ranking;

                /*发送规则：
                 * 初始点赞数为0，并且不发送通知
                 *   a、点赞数变化则发送通知，并更新发送表的点赞数为本次点赞数、NoChangeInDays=1
                 *   b、点赞数没变化，则NoChangeInDays+=1；如果NoChangeInDays<=6 && NoChangeInDays%3==0,则发送通知并更新 ，否则不发
                 *   c、用户多篇评测，则选符合a或者b的评测，发送其中排名最前的
                 *   d、点赞数为0，或者NoChangeInDays>6,则不发通知，不更新记录表
                 */

                var oldSend = oldSendActivityEvltNotices.FirstOrDefault(_ => _.UserId == toSendUser.UserId && _.EvaluationId == toSendUser.EvaluationId);
                if (oldSend == null)
                {
                    unitOfWork.DbConnection.Execute(@$"Insert Into [dbo].[ActivityEvltNotice] ([NoChangeInDays], [UserId], [Likes], [Id], [evaluationid]) Values(@NoChangeInDays, @UserId, @Likes, NEWID(), @evaluationid)"
                    ,new DynamicParameters().Set("NoChangeInDays", 1).Set("UserId", toSendUser.UserId).Set("Likes", toSendUser.Likes).Set("evaluationid", toSendUser.EvaluationId));                   
                }
                #region 不发送情况
                if((toSendUser.Likes == oldSend?.Likes && toSendUser.Likes>0)|| toSendUser.Likes==0)//大前提：点赞数不变化
                {
                    //规则d
                    if (oldSend?.NoChangeInDays > 6 || toSendUser.Likes == 0) continue;

                    else if (oldSend?.NoChangeInDays%3!=0)//6天以内无变化，则更新 NoChangeInDays+=1，
                    {
                        unitOfWork.DbConnection.Execute($"update [dbo].[ActivityEvltNotice]  Set NoChangeInDays+=1 Where UserId=@UserId and evaluationid=@evaluationid"
                        ,new DynamicParameters().Set("UserId", toSendUser.UserId).Set("evaluationid", toSendUser.EvaluationId));
                        continue;
                    }                    
                }
                #endregion

                #region 发送
                //每个用户只发排名最好的一条测评
                if (!currentSentUsers.Contains(toSendUser.UserId))
                {
                    #region 向公众号推送模板消息
                    TemplateManager templateManager = new TemplateManager(httpClient);
                    var message = new SendTemplateRequest(wxUsers.FirstOrDefault(_ => _.UserId == toSendUser.UserId).OpenID, appSettings.TemplateId);
                    message.Url = appSettings.ActEvltDetails.FormatWith(UrlShortIdUtil.Long2Base32(Convert.ToInt64(toSendUser.EvaluationNo)));
                    message.SetData(new TemplateDataFiled[] {
                        new  TemplateDataFiled(){
                           Filed = "first",
                           Value = $"您在【{toSendUser.ActivityTitle}】中发表了《{toSendUser.EvltTitle}》",
                        },
                        new TemplateDataFiled(){
                           Filed = "keyword1",
                           Value = $"点赞数{toSendUser.Likes}，点赞排名{Ranking}名"
                        },
                        new TemplateDataFiled(){
                           Filed="keyword2",
                           Value=$"进行中，{toSendUser.EndTime.Month}月{toSendUser.EndTime.Day}日结束"
                        }
                    });
                    var response = await templateManager.SendAsync(accessToken, message);
                    #endregion
                    currentSentUsers.Add(toSendUser.UserId);
                    if(oldSend!=null)//更新点赞数为当前点赞数、变化天数=1
                        unitOfWork.DbConnection.Execute($"update [dbo].[ActivityEvltNotice]  Set NoChangeInDays=@NoChangeInDays,Likes=@Likes Where UserId=@UserId and evaluationid=@evaluationid"
                        , new DynamicParameters().Set("UserId", toSendUser.UserId).Set("evaluationid", toSendUser.EvaluationId).Set("Likes",toSendUser.Likes).Set("NoChangeInDays", toSendUser.Likes==oldSend.Likes? oldSend.NoChangeInDays+1:1));

                } 
                #endregion

            }

            return default;
        }
    }


}
