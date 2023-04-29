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
   /// 编辑评测
   /// </summary>
    public class UpdateEvaluationCommandHandler : IRequestHandler<UpdateEvaluationCommand, ResponseResult>
    {
        OrgUnitOfWork orgUnitOfWork;
        WXOrgUnitOfWork _wXOrgUnitOfWork;
        CSRedisClient _redisClient;
        EvltCoverCreateOption evltCoverCreateOption;
        IHttpClientFactory httpClientFactory;
        IConfiguration config;
        Random rd = new Random();
        IMediator _mediator;

        const int time = 60 * 60;

        public UpdateEvaluationCommandHandler(IOrgUnitOfWork unitOfWork
            , CSRedisClient redisClient
           , IWXUnitOfWork wXUnitOfWork
            , IConfiguration config
            , IOptionsSnapshot<EvltCoverCreateOption> evltCoverCreateOption
            , IHttpClientFactory httpClientFactory
            , IMediator mediator
            )
        {
            this.orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._redisClient = redisClient;
            this._wXOrgUnitOfWork = (WXOrgUnitOfWork)wXUnitOfWork;
            this.evltCoverCreateOption = evltCoverCreateOption.Value;
            this.config = config;
            this.httpClientFactory = httpClientFactory;
            this._mediator = mediator;
        }


        public async Task<ResponseResult> Handle(UpdateEvaluationCommand request, CancellationToken cancellationToken)
        {           
            try
            {      
                var p = request;

                #region 同步sql及逻辑 
                var dp = new DynamicParameters();
                string evalSql = @"update dbo.Evaluation set title=@title,cover=@cover,isPlaintext=@isPlaintext,HasVideo=@HasVideo,stick=@stick,ModifyDateTime=@time,Modifier=@Modifier where id=@evaluationid ; ";
                string itemSql = @" update [dbo].[EvaluationItem] set content=@content_{0}, pictures=@pictures_{0}, thumbnails=@thumbnails_{0} ,video=@video_{0},videoCover=@videoCover_{0} where id=@id_{0}; ";

                #region 评测绑定机构--课程
                StringBuilder bindSql = new StringBuilder();
                bindSql.Append($@" update [dbo].[EvaluationBind] set  IsValid=0 where evaluationid=@evaluationid ;");
                if (request.ListCourseId?.Any() == true)//绑定机构下的课程
                {
                    bindSql.Append($@"INSERT INTO  [dbo].[EvaluationBind]([id], [evaluationid], [orgid], [courseid],  [IsValid])VALUES ");
                    List<string> values = new List<string>();
                    var orgid = request.ListOrgId[0];
                    foreach (var item in request.ListCourseId)
                    {
                        values.Add($"(NEWID(), @evaluationid,'{orgid}','{item}',  @IsValid)");
                    }
                    bindSql.Append(string.Join(',', values));
                }
                else if (request.ListOrgId?.Any() == true)//仅仅绑定机构
                {
                    bindSql.Append($@"INSERT INTO  [dbo].[EvaluationBind]([id], [evaluationid], [orgid],  [IsValid])VALUES");
                    List<string> values = new List<string>();
                    foreach (var item in request.ListOrgId)
                    {
                        values.Add($" (NEWID(), @evaluationid,'{item}', @IsValid) ");
                    }
                    bindSql.Append(string.Join(',', values));
                } 
                #endregion

                string speBingSql = "";
                if (p.Specialid != null && p.Specialid != default)
                {
                    speBingSql = $@" Update [dbo].[SpecialBind] set IsValid=0 where evaluationid=@evaluationid;
                                    Insert Into [dbo].[SpecialBind]([id], [specialid], [evaluationid], [IsValid])values(NEWID(),@specialid,@evaluationid,1);";
                }

                StringBuilder strBuilderItemSqls = new StringBuilder();
                var isPlaintext = true;//默认纯文字图片
                string content = "";
                var cover = "";//封面图
                bool HasVideo = false;//是否有视频
                var listPic = new List<string>();
                var listVideoCover = new List<string>();
                if (p.ListEvltItems?.Any() == true)
                {
                    for (int i = 0; i < p.ListEvltItems?.Count; i++)
                    {
                        var item = p.ListEvltItems[i];
                        if (string.IsNullOrEmpty(item.Thumbnails) || item.Thumbnails == "[]")//无图
                        {
                            content += item.Content;
                        }
                        else
                        {
                            isPlaintext = false;
                            listPic.AddRange(JsonSerializationHelper.JSONToObject<List<string>>(item.Thumbnails));
                        }
                        strBuilderItemSqls.AppendFormat(itemSql, i, item.Thumbnails, item.Id);
                        dp.Set($"content_{i}", item.Content)
                          .Set($"pictures_{i}", item.Pictures)
                          .Set($"thumbnails_{i}", item.Thumbnails)
                          .Set($"id_{i}", item.Id)
                          .Set($"video_{i}", item.Video)
                          .Set($"videoCover_{i}", item.VideoCover);
                        if (!string.IsNullOrEmpty(item.VideoCover))
                            listVideoCover.Add(item.VideoCover);
                    }
                }

                HasVideo = listVideoCover?.Any() == true;
                if (HasVideo) //优先去视频第一个视频第一针，没则取第一张图片作为封面
                {
                    cover = listVideoCover[0];
                }
                else if (isPlaintext)
                {
                    var imgContent = content.Length > 30 ? content.Substring(0, 30) : content;
                    cover = await CreatePlainTextPicture(imgContent, p.Id);
                }
                else cover = listPic?.FirstOrDefault();

                //总sql
                string sql = evalSql + strBuilderItemSqls.ToString()+ bindSql.ToString() + speBingSql;
                //所有参数
                  dp.Set("evaluationid", p.Id)
                    .Set("title", p.Title)
                    .Set("cover", cover)
                    .Set("isPlaintext", isPlaintext)
                    .Set("HasVideo", HasVideo)
                    .Set("stick", p.IsStick)
                    .Set("time", DateTime.Now)
                    .Set("IsValid", true)
                    //.Set("orgid", p.OrgId == default ? null : p.OrgId)
                    //.Set("courseid", p.CourseId == default ? null : p.CourseId)
                    //.Set("coursename", p.CourseName)
                    //.Set("subject", p.Subject)
                    //.Set("age", p.Age)
                    //.Set("mode", p.Mode)
                    //.Set("duration", p.Duration)
                    .Set("specialid", p.Specialid)    
                    .Set("Modifier",p.Modifier)
                    ;

                #endregion

                #region 同步
                orgUnitOfWork.BeginTransaction();

                orgUnitOfWork.DbConnection.Execute(sql, dp, orgUnitOfWork.DbTransaction);

                orgUnitOfWork.CommitChanges();
                #endregion

                #region 清除相关缓存-移到在action中清除


                //CSRedisClientHelper.BatchDel(_redisClient, new List<string>()
                //{
                //    CacheKeys.EvaluationLikesCount.FormatWith(p.Id)
                //    ,"org:evltsMain:*"
                //    ,"org:spcl:*"
                //});
                #endregion

                return await Task.FromResult(ResponseResult.Success("更新成功"));
            }
            catch (Exception ex)
            {
                orgUnitOfWork.Rollback();
                return await Task.FromResult(ResponseResult.Failed(ex.Message));
            }
        }
        #region 生成一张纯文字图片
        public async Task<string> CreatePlainTextPicture(string text, Guid evalId)
        {
            using var ms = ImgHelper.CreateEvltCover(text, evltCoverCreateOption);
            return await upload_cover(ms, evalId);

        }

        public async Task<string> upload_cover(Stream img, Guid evalId)
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
