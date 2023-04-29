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
using static iSchool.Organization.Appliaction.CommonHelper.UserInfoHelper;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
   /// <summary>
   /// 新增评测
   /// </summary>
    public class AddEvltCommandHandler : IRequestHandler<AddEvltCommand, ResponseResult>
    {
        OrgUnitOfWork orgUnitOfWork;
        WXOrgUnitOfWork _wXOrgUnitOfWork;
        CSRedisClient _redisClient;
        EvltCoverCreateOption evltCoverCreateOption;
        IHttpClientFactory httpClientFactory;
        IConfiguration config;
        Random rd = new Random();


        public AddEvltCommandHandler(IOrgUnitOfWork unitOfWork
            , CSRedisClient redisClient
           , IWXUnitOfWork wXUnitOfWork
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


        public async Task<ResponseResult> Handle(AddEvltCommand request, CancellationToken cancellationToken)
        {           
            try
            {
                bool IsUrgent = request.IsUrgent;
                DateTime dateTime = IsUrgent ? DateTime.Now.AddMinutes(-30) : DateTime.Now.AddDays(-1);

                var p = request;

                //图片urls
                var listUrls = string.IsNullOrEmpty(request.Url) ? null : JsonSerializationHelper.JSONToObject<List<string>>(request.Url);

                //缩略图片urls
                var listThumUrls = string.IsNullOrEmpty(request.ThumUrl) ? null : JsonSerializationHelper.JSONToObject<List<string>>(request.ThumUrl);

                //评论内容集合
                var listComments = string.IsNullOrEmpty(request.Comments) ? null : JsonSerializationHelper.JSONToObject<List<string>>(request.Comments);

                var evalId = request.Id;//新建

                var content = HtmlHelper.NoHTML(request.Content);
                var imgContent = content.Length > 30 ? content.Substring(0, 30) : content;
                var cover = listThumUrls?.Any()==false ? await CreatePlainTextPicture(imgContent, evalId) : listThumUrls[0];

                //随机用户         
                int userCount = 1+(listComments?.Any()==false ? 0 : listComments.Count) ;
                var userInfos = new UserInfoHelper(_wXOrgUnitOfWork).GetUserInfos(userCount, -10);

                #region 同步sql及逻辑 
                var dp = new DynamicParameters();
                string evalSql = $@"INSERT INTO [dbo].[Evaluation] ([id], [title], [cover], [isPlaintext], [mode], [userid], [status],  [CreateTime], [Creator], [IsValid],[IsOfficial],[stick])
                                VALUES(@evaluationid, @title, @cover, @isPlaintext, @EvltContentMode, @userid, @status, @CreateTime, @Creator, @IsValid,@IsOfficial,@stick);";
                string itemSql = $@" Insert into [dbo].[EvaluationItem]
                                    ([id], [evaluationid], [type], [content], [pictures], [thumbnails], [IsValid])
                                    values(NEWID(),@evaluationid,@type,@content,@pictures,@thumbnails,1) ;";
                #region  comSql EvaluationComment--评论内容表
                string comSql = "";
                if (listComments?.Any()==true)
                {
                    List<string> values = new List<string>();
                    var seed = 0; //累计随机数                   
                    for (int i = 0; i < listComments.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(listComments[i].Trim()))
                        {

                            int mm = IsUrgent ? rd.Next(2, 6) : rd.Next(30, 200);//随机数mm
                            var time = dateTime.AddMinutes(mm + seed);
                            int index = i + 1;
                            values.Add($"( NEWID(), @evaluationid, @UserId_{index} , @NickName_{index}, @Comment_{index}, @time_{index}, @Creator,@IsValid,@IsOfficial)");
                            seed += mm;
                            dp.Set("UserId_" + index, userInfos[index].UserId)
                              .Set("NickName_" + index, userInfos[index].NickName)
                              .Set("Comment_"+index, listComments[i])
                              .Set("time_"+index, time);
                              
                        }
                    }
                    if (values.Any()==true)
                    {

                        comSql = $@"insert into [dbo].[EvaluationComment] ([id], [evaluationid], [userid], [username], [comment], [CreateTime], [Creator],[IsValid],[IsOfficial])
                                values {string.Join(',', values)} ;";                       
                    }
                }
                #endregion
                string update = $@"update e set e.commentcount=c.cc
                                   from Evaluation e, (select evaluationid, count(1) cc from EvaluationComment where IsValid = 1 and evaluationid=@evaluationid group by evaluationid)c
                                   where e.id = c.evaluationid and e.IsValid = 1 and e.id=@evaluationid ;";

                #region bindSql 评测--机构--课程绑定表
                
                StringBuilder bindSql = new StringBuilder();
                if (request.ListCourseId?.Any()==true)//绑定机构下的课程
                {
                    bindSql.Append($@"
INSERT INTO  [dbo].[EvaluationBind]
([id], [evaluationid], [orgid], [courseid],  [IsValid])
VALUES
 ");
                    List<string> values = new List<string>();
                    var orgid = request.ListOrgId[0];
                    foreach (var item in request.ListCourseId)
                    {
                        values.Add($"(NEWID(), @evaluationid,'{orgid}','{item}',  @IsValid)");
                    }
                    bindSql.Append(string.Join(',', values));
                }
                else if (request.ListOrgId?.Any()==true)//仅仅绑定机构
                {
                    bindSql.Append($@"
INSERT INTO  [dbo].[EvaluationBind]
([id], [evaluationid], [orgid],  [IsValid])
VALUES
");
                    List<string> values = new List<string>();
                    foreach (var item in request.ListOrgId)
                    {
                        values.Add($" (NEWID(), @evaluationid,'{item}', @IsValid) ");
                    }
                    bindSql.Append(string.Join(',', values));
                }
                #endregion

                #region speBingSql  SpecialBind--评测专题绑定表
                string speBingSql = "";
                if (request.Specialid != null && request.Specialid != default)
                {
                    speBingSql = $@" 
                                Update [dbo].[SpecialBind] set IsValid=0 where specialid=@specialid and evaluationid=@evaluationid;
                                Insert Into [dbo].[SpecialBind]([id], [specialid], [evaluationid], [IsValid])values(NEWID(),@specialid,@evaluationid,1);";                   
                }

                #endregion

                var aa = bindSql.ToString();
                //总sql
                string sql = evalSql+ itemSql+ comSql+ update+ bindSql.ToString()+ speBingSql;
                //所有参数
                 dp .Set("evaluationid", evalId)
                    .Set("title", request.Title)
                    .Set("cover", cover)
                    .Set("type", EvltContentItemTypeEnum.Normal)
                    .Set("isPlaintext", listUrls == null ? true : false)
                    .Set("EvltContentMode", EvltContentModeEnum.Normal)
                    .Set("userid", userInfos[0].UserId)
                    .Set("Creator",request.Creator)
                    .Set("content", request.Content)
                    .Set("status", EvaluationStatusEnum.Ok)
                    .Set("CreateTime", dateTime)
                    .Set("IsValid", true)
                    .Set("IsOfficial", true)
                    .Set("pictures", request.Url)
                    .Set("thumbnails", request.ThumUrl)
                    //.Set("id", Guid.NewGuid())
                    //.Set("orgid", request.OrgId == default ? null : request.OrgId)
                    //.Set("courseid", request.CourseId == default ? null : request.CourseId)
                    //.Set("specialid", request.Specialid)
                    //.Set("coursename",request.CourseName)
                    //.Set("subject", request.Subject)
                    //.Set("age", request.Age)
                    //.Set("mode", request.Mode)
                    //.Set("duration", request.Duration)
                    .Set("stick",request.IsStick)
                    ;

                #endregion



                #region 发布同步


                //发布同步数据到以下相关表
                orgUnitOfWork.BeginTransaction();

                orgUnitOfWork.DbConnection.Execute(sql, dp, orgUnitOfWork.DbTransaction);

               orgUnitOfWork.CommitChanges();
                #endregion

                #region 发布同步后


                //_redisClient.BatchDel(new List<string>()
                //{
                //    CacheKeys.EvaluationLikesCount.FormatWith(evalId)
                //    ,"org:evltsMain:*"
                //    ,"org:spcl:*"
                //});


                #endregion

                return await Task.FromResult(ResponseResult.Success("发布成功"));
            }
            catch (Exception ex)
            {
                orgUnitOfWork.Rollback();
                return await Task.FromResult(ResponseResult.Failed(ex.Message));
            }
        }

        #region 生成一张纯文字图片
        async Task<string> CreatePlainTextPicture(string text, Guid evalId)
        {
            using var ms = ImgHelper.CreateEvltCover(text, evltCoverCreateOption);
            return await upload_cover(ms, evalId);

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
