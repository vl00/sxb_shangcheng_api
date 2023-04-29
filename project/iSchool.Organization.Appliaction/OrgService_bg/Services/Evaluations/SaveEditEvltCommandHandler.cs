using CSRedis;
using Dapper;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;
using iSchool.Organization.Appliaction.CommonHelper;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    /// <summary>
    /// 发布抓取评测
    /// </summary>
    public class SaveEditEvltCommandHandler : IRequestHandler<SaveEditEvltCommand, ResponseResult>
    {
        OrgUnitOfWork orgUnitOfWork;
        WXOrgUnitOfWork _wXOrgUnitOfWork;
        CSRedisClient _redisClient;
        EvltCoverCreateOption evltCoverCreateOption;
        IHttpClientFactory httpClientFactory;
        IConfiguration config;       
        Random rd = new Random();

        const int time = 60 * 60;

        public SaveEditEvltCommandHandler(IOrgUnitOfWork unitOfWork
            ,CSRedisClient redisClient
           ,IWXUnitOfWork wXUnitOfWork
            , IConfiguration config
            , IOptionsSnapshot<EvltCoverCreateOption> evltCoverCreateOption
            , IHttpClientFactory httpClientFactory
            )
        {
            this.orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._redisClient = redisClient;
            this._wXOrgUnitOfWork = (WXOrgUnitOfWork)wXUnitOfWork;
            this.evltCoverCreateOption = evltCoverCreateOption.Value;
            this.config = config;
            this.httpClientFactory = httpClientFactory;
        }


        public async Task<ResponseResult> Handle(SaveEditEvltCommand request, CancellationToken cancellationToken)
        {
            #region 待改进
            //            try
            //            {
            //                bool IsUrgent = request.IsUrgent;
            //                DateTime dateTime = IsUrgent ? DateTime.Now.AddMinutes(-30) : DateTime.Now.AddDays(-1);

            //                var p = request;

            //                //图片urls
            //                var listUrls = string.IsNullOrEmpty(request.Url) ? null : JsonSerializationHelper.JSONToObject<List<string>>(request.Url);

            //                //缩略图片urls
            //                var listThumUrls = string.IsNullOrEmpty(request.ThumUrl) ? null : JsonSerializationHelper.JSONToObject<List<string>>(request.ThumUrl);

            //                //评论内容集合
            //                var listComments = string.IsNullOrEmpty(request.Comments) ? null : JsonSerializationHelper.JSONToObject<List<string>>(request.Comments);

            //                var evalId = request.Id;//抓取评测Id作为评测Id

            //                var content = HtmlHelper.NoHTML(request.Content);
            //                var imgContent = content.Length > 30 ? content.Substring(0, 30) : content;
            //                var cover = (listThumUrls == null || listThumUrls.Count == 0) ? await CreatePlainTextPicture(imgContent, evalId) : listThumUrls[0];

            //                //随机用户总数
            //                var totalUserCount = _wXOrgUnitOfWork.DbConnection.Query<int>($@" select count(1) FROM [dbo].[userInfo] WHERE channel='1' ;").FirstOrDefault();
            //                var userInfos = GetEvltUserInfos(totalUserCount, 10);



            //                #region 发布同步


            //                //发布同步数据到以下相关表
            //                orgUnitOfWork.BeginTransaction();

            //                #region EvaluationCrawler--抓取评论表
            //                string crawSql = $@" UPDATE [dbo].[EvaluationCrawler] SET 
            //                                title=@title,status=@status,content=@content,orgid=@orgid,courseid=@courseid
            //                                ,specialid=@specialid,url=@url,comments=@comments, CreateTime=@CreateTime, IsValid=@IsValid
            //                                WHERE ID=@ID ;";
            //                orgUnitOfWork.DbConnection.Execute(crawSql, new DynamicParameters()
            //                    .Set("title", request.Title)
            //                    .Set("status", CaptureEvalStatusEnum.Published)
            //                    .Set("content", request.Content)
            //                    .Set("orgid", request.OrgId)
            //                    .Set("courseid", request.CourseId)
            //                    .Set("specialid", request.Specialid)
            //                    .Set("url", request.Url)
            //                    .Set("comments", request.Comments)
            //                    .Set("CreateTime", DateTime.Now)
            //                    .Set("IsValid", true)
            //                    .Set("ID", request.Id)
            //                    , orgUnitOfWork.DbTransaction);

            //                #endregion

            //                #region Evaluation--评测表
            //                string evalSql = $@"INSERT INTO [dbo].[Evaluation] ([id], [title], [cover], [isPlaintext], [mode], [userid], [status], [crawlerId], [CreateTime], [Creator], [IsValid],[IsOfficial])
            //                                VALUES(@id, @title, @cover, @isPlaintext, @mode, @userid, @status, @crawlerId, @CreateTime, @Creator, @IsValid,@IsOfficial);";
            //                orgUnitOfWork.DbConnection.Execute(evalSql, new DynamicParameters()
            //                   .Set("id", evalId)
            //                   .Set("title", request.Title)
            //                   .Set("cover", cover)
            //                   .Set("isPlaintext", listUrls == null ? true : false)
            //                   .Set("mode", EvltContentModeEnum.Normal)
            //                   .Set("userid", userInfos[0].UserId)
            //                   .Set("status", EvaluationStatusEnum.Ok)
            //                   .Set("crawlerId", request.Id)
            //                   .Set("CreateTime", dateTime)
            //                   .Set("Creator", userInfos[0].UserId)
            //                   .Set("IsValid", true)
            //                   .Set("IsOfficial", true)
            //                   , orgUnitOfWork.DbTransaction);



            //                #endregion

            //                #region EvaluationItem--评测内容表
            //                string itemSql = $@" Insert into [dbo].[EvaluationItem]
            //                                    ([id], [evaluationid], [type], [content], [pictures], [thumbnails], [IsValid])
            //                                    values(NEWID(),@evaluationid,@type,@content,@pictures,@thumbnails,1) ;";
            //                orgUnitOfWork.DbConnection.Execute(itemSql, new DynamicParameters()
            //                   .Set("evaluationid", evalId)
            //                   .Set("type", EvltContentItemTypeEnum.Normal)
            //                   .Set("content", request.Content)
            //                   .Set("pictures", request.Url)
            //                   .Set("thumbnails", request.ThumUrl)
            //                   , orgUnitOfWork.DbTransaction);

            //                #endregion

            //                #region EvaluationComment--评论内容表
            //                if (listComments != null && listComments.Count > 0)
            //                {
            //                    List<string> values = new List<string>();
            //                    var seed = 0; //累计随机数                   
            //                    for (int i = 0; i < listComments.Count; i++)
            //                    {
            //                        if (!string.IsNullOrEmpty(listComments[i].Trim()))
            //                        {

            //                            int mm = IsUrgent ? rd.Next(2, 6) : rd.Next(30, 200);//随机数mm
            //                            var time = dateTime.AddMinutes(mm + seed);
            //                            values.Add($"( NEWID(), '{evalId}', '{userInfos[i + 1].UserId}', '{userInfos[i + 1].NickName}', '{listComments[i]}', '{time}', '{new Guid()}',@IsValid,@IsOfficial)");
            //                            seed += mm;
            //                        }
            //                    }
            //                    if (values.Count > 0)
            //                    {

            //                        string comSql = $@"insert into [dbo].[EvaluationComment] ([id], [evaluationid], [userid], [username], [comment], [CreateTime], [Creator],[IsValid],[IsOfficial])
            //                                values {string.Join(',', values)} ;";
            //                        orgUnitOfWork.DbConnection.Execute(comSql, new DynamicParameters()
            //                           .Set("IsValid", true)
            //                           .Set("IsOfficial", true)
            //                           , orgUnitOfWork.DbTransaction);
            //                    }
            //                }
            //                #endregion

            //                #region 更新评论数
            //                string update = $@"update e set e.commentcount=c.cc
            //                                   from Evaluation e, (select evaluationid, count(1) cc from EvaluationComment where IsValid = 1 and evaluationid=@evaluationid group by evaluationid)c
            //                                   where e.id = c.evaluationid and e.IsValid = 1 and e.id=@evaluationid ;";


            //                orgUnitOfWork.DbConnection.Execute(update, new DynamicParameters()
            //                   .Set("evaluationid", evalId)
            //                   , orgUnitOfWork.DbTransaction);
            //                #endregion

            //                #region EvaluationBind--评测绑定表

            //                string bindSql = $@"INSERT INTO  [dbo].[EvaluationBind]
            //                                    ([id], [evaluationid], [orgid], [courseid],  [IsValid])
            //                                    VALUES(@id, @evaluationid, @orgid, @courseid,  @IsValid);";
            //                orgUnitOfWork.DbConnection.Execute(bindSql, new DynamicParameters()
            //                   .Set("id", Guid.NewGuid())
            //                   .Set("evaluationid", evalId)
            //                   .Set("orgid", request.OrgId == default ? null : request.OrgId)
            //                   .Set("courseid", request.CourseId == default ? null : request.CourseId)
            //                   .Set("IsValid", true)
            //                   , orgUnitOfWork.DbTransaction);

            //                #endregion

            //                #region SpecialBind--评测专题绑定表
            //                if (request.Specialid != null && request.Specialid != default)
            //                {
            //                    string speBingSql = $@" 
            //Update [dbo].[SpecialBind] set IsValid=0 where specialid=@specialid and evaluationid=@evaluationid;
            //Insert Into [dbo].[SpecialBind]([id], [specialid], [evaluationid], [IsValid])values(NEWID(),@specialid,@evaluationid,1);";
            //                    orgUnitOfWork.DbConnection.Execute(speBingSql, new DynamicParameters()
            //                        .Set("specialid", request.Specialid)
            //                        .Set("evaluationid", evalId)
            //                        , orgUnitOfWork.DbTransaction);
            //                }

            //                #endregion

            //                orgUnitOfWork.CommitChanges();
            //                #endregion

            //                #region 发布同步后，清除相关缓存--后续需要继续完善


            //                CSRedisClientHelper.BatchDel(_redisClient, new List<string>()
            //                {
            //                     CacheKeys.Evlt.FormatWith(evalId)
            //                    ,CacheKeys.EvaluationLikesCount.FormatWith(evalId)
            //                    ,"org:evltsMain:*"
            //                    ,"org:spcl:*"
            //                });


            //                #endregion

            //                return await Task.FromResult(ResponseResult.Success("发布成功"));
            //            }
            //            catch (Exception ex)
            //            {
            //                orgUnitOfWork.Rollback();
            //                return await Task.FromResult(ResponseResult.Failed(ex.Message));
            //            }        
            #endregion
            return null;
        }

        #region 随机产生评测用户
        /// <summary>
        /// 随机产生评测用户
        /// </summary>
        /// <param name="userCount">随机用户总数</param>
        /// <param name="taskcount">一次取N条</param>
        /// <returns></returns>
        private List<UserInfo> GetEvltUserInfos(int userCount,int taskcount)
        {
            return _wXOrgUnitOfWork.DbConnection.Query<UserInfo>($@" SELECT  id as UserId,nickname FROM [dbo].[userInfo] WHERE channel='1' order by UserId OFFSET {rd.Next(1, userCount)} ROWS FETCH NEXT {taskcount} ROWS ONLY ;").ToList();
                        
        }

        public class UserInfo
        {
            /// <summary>
            /// 用户Id
            /// </summary>
            public Guid UserId { get; set; }

            /// <summary>
            /// 用户名称
            /// </summary>
            public string NickName { get; set; }
        }
        #endregion

        #region 生成一张纯文字图片
        async Task<string> CreatePlainTextPicture(string text,Guid evalId)
        {
            using var ms = ImgHelper.CreateEvltCover(text, evltCoverCreateOption);
            return await upload_cover(ms,evalId);
            
        }

        async Task<string> upload_cover(Stream img, Guid evalId)
        {
            var url = config[Consts.OrgBaseUrl_UploadUrl].FormatWith($"eval/{evalId}", $"{Guid.NewGuid()}.png");
            using var http = httpClientFactory.CreateClient(string.Empty);
            http.DefaultRequestHeaders.Set("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
            var req = new HttpRequestMessage(HttpMethod.Post, url)
                .SetContent(new StreamContent(img));
            var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var rez = JToken.Parse(await res.Content.ReadAsStringAsync());
            if ((int?)rez["status"] != 0) throw new CustomResponseException("封面图生成后上传失败");
            return rez["compress"]?["cdnUrl"]?.ToString() ?? rez["cdnUrl"].ToString();
        }

        

        #endregion


    }
}
