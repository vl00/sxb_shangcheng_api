using AutoMapper;
using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Drawing.Imaging;
using iSchool.Domain.Repository.Interfaces;

namespace iSchool.Organization.Appliaction.Service
{
    public class MiniAddEvaluationCommandHandler : IRequestHandler<MiniAddEvaluationCommand, EvaluationAddedResult>
    {
        IServiceProvider services;
        OrgUnitOfWork unitOfWork;
        IUserInfo me;
        IMediator mediator;
        IConfiguration config;
        EvltCoverCreateOption evltCoverCreateOption;
        IHttpClientFactory httpClientFactory;
        IMapper mapper;
        CSRedisClient redis;
        IRepository<Evaluation> _evalRepo;        

        public MiniAddEvaluationCommandHandler(IOrgUnitOfWork unitOfWork, IUserInfo me, IMediator mediator,
            IConfiguration config, IHttpClientFactory httpClientFactory, IMapper mapper, CSRedisClient redis,
            IOptionsSnapshot<EvltCoverCreateOption> evltCoverCreateOption,
            IServiceProvider services, IRepository<Evaluation> evalRepo)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            this.mediator = mediator;
            this.config = config;
            this.httpClientFactory = httpClientFactory;
            this.mapper = mapper;
            this.redis = redis;
            this.evltCoverCreateOption = evltCoverCreateOption.Value;
            this.services = services;
            this._evalRepo = evalRepo;
        }

        void Valid_cmd(MiniAddEvaluationCommand cmd, bool IsEdit)
        {
            //cmd.Mode = 1;            

            if (!IsEdit)
            {
                // old codes
            }
            switch (cmd.Mode)
            {
                case (int)EvltContentModeEnum.Normal:
                    {
                        _ = cmd.Ctt1 ?? throw new CustomResponseException("自由模式内容不应为null");
                        cmd.Ctt1.Pictures ??= new string[0];
                        cmd.Ctt1.Thumbnails ??= new string[0];
                        if (cmd.Ctt1.Title.IsNullOrWhiteSpace())
                            throw new CustomResponseException("标题不能为空");
                        if (cmd.Ctt1.Title.Length > 30)
                            throw new CustomResponseException("标题不能超过30字");
                        if (cmd.Ctt1.Pictures.Length != cmd.Ctt1.Thumbnails.Length)
                            throw new CustomResponseException("真实图片数量与缩略图数量不一致");
                        if (cmd.Ctt1.Pictures.Length > 9)
                            throw new CustomResponseException("图片数量不能超过9张");
                        if (cmd.Ctt1.Pictures.Length < 1 && cmd.Ctt1.VideoCoverUrl.IsNullOrEmpty())
                            throw new CustomResponseException("必须至少上传1张图片或1个视频");
                        if (cmd.Ctt1.Content.IsNullOrWhiteSpace())
                            throw new CustomResponseException("正文不能为空");
                        if (cmd.Ctt1.Content.Length < 50)
                            throw new CustomResponseException("正文不能少于50字");
                        if (cmd.Ctt1.VideoUrl.IsNullOrEmpty() && !cmd.Ctt1.VideoCoverUrl.IsNullOrEmpty())
                            throw new CustomResponseException("视频有封面图,但播放地址为空");
                    }
                    break;
                default:
                    throw new NotSupportedException();
                    //break;
            }

            // 关联主体
            switch ((EvltRelatedModeEnum)cmd.RelatedMode)
            {
                case EvltRelatedModeEnum.Course:
                    cmd.RelatedOrgIds = null;
                    cmd.RelatedCourseIds = cmd.RelatedCourseIds?.Distinct().ToArray();
                    if ((cmd.RelatedCourseIds?.Length ?? 0) < 1) throw new CustomResponseException("必须关联1个课程");
                    break;
                case EvltRelatedModeEnum.Org:
                    cmd.RelatedCourseIds = null;
                    cmd.RelatedOrgIds = cmd.RelatedOrgIds?.Distinct().ToArray();
                    if ((cmd.RelatedOrgIds?.Length ?? 0) < 1) throw new CustomResponseException("必须关联1个品牌");
                    break;
                case EvltRelatedModeEnum.Other:
                    cmd.RelatedCourseIds = null;
                    cmd.RelatedOrgIds = null;
                    break;
                default:
                    throw new CustomResponseException("无效的关联主体类型");
            }
        }

        public async Task<EvaluationAddedResult> Handle(MiniAddEvaluationCommand cmd, CancellationToken cancellation)
        {          
            var IsEdit = cmd.EvaluationId != null && Guid.Empty != cmd.EvaluationId;
            Valid_cmd(cmd, IsEdit);
            var no = 0L;
            var result = new EvaluationAddedResult { }; //评测id
            Task<string> createCoverTask = null;            
            var tbfValid = new List<(string tb, string idField, object idValue)>();
            HdDataInfoDto hdInfo = null;
            var after_up_evlt = new List<Func<Task>>();
            await default(ValueTask);            

            if (!IsEdit) // add
            {
                result.Id = Guid.NewGuid();

                if (!(await redis.SetAsync($"org:lck2:addevlt:uid_{me.UserId}", 1, 4, RedisExistence.Nx)))
                {
                    throw new CustomResponseException("发评测太快了");
                }
            }
            else // update
            {
                var evlt = await mediator.Send(new GetEvltBaseInfoQuery { EvltId = cmd.EvaluationId.Value });
                if (null == evlt) throw new CustomResponseException("参数错误");
                if (evlt.AuthorId != me.UserId) throw new CustomResponseException("非法操作");
                no = evlt.No;
                result.Id = cmd.EvaluationId.Value;
                result.SpecialId = evlt.SpecialId;
                result.SpecialId_s = evlt.SpecialNo == null ? null : UrlShortIdUtil.Long2Base32(evlt.SpecialNo.Value);
                result.SpecialName = evlt.SpecialName;
            }

            // check if 测评有敏感词
            {
                var txts = cmd.Mode switch
                {
                    (int)EvltContentModeEnum.Normal => new[] { cmd.Ctt1.Content },
                    _ => null
                };
                var trst = await mediator.Send(new SensitiveKeywordCmd { Txts = txts });
                if (!trst.Pass)
                {
                    if ((trst.FilteredTxts?.Length ?? 0) < 1)
                        throw new CustomResponseException("您发表的测评有敏感词，请修改后再发", ResponseCode.GarbageContent.ToInt());

                    cmd.Ctt1.Content = trst.FilteredTxts[0];
                }
            }

            //
            // 添加时 检查活动和专题是否匹配            
            if (!IsEdit && hdInfo?.Data != null)
            {
                // ...
            }
            //
            // 编辑时 检查是否活动评测并检查能否编辑            
            for (; IsEdit && 1 == 1;) //&& result.SpecialId != null
            {
                // ...    
                break;
            }

            // 封面图取第一张             
            var hasVideo = cmd.Mode switch
            {
                (int)EvltContentModeEnum.Normal => !cmd.Ctt1.VideoUrl.IsNullOrEmpty(),
                _ => false,
            };
            var cover = cmd.Mode switch
            {
                (int)EvltContentModeEnum.Normal => hasVideo ? cmd.Ctt1.VideoCoverUrl : cmd.Ctt1.Thumbnails.FirstOrDefault(),
                _ => null
            };
            var isPlaintext = cmd.Mode switch
            {
                (int)EvltContentModeEnum.Normal => string.IsNullOrEmpty(cmd.Ctt1.Thumbnails.FirstOrDefault()),
                _ => true,
            };

            // 无图需要生成封面图
            // 有视频就拿视频封面
            if (string.IsNullOrEmpty(cover))
            {
                createCoverTask = CreateEvltCover(cmd, result.Id);
            }

            // 关联的主体 可以编辑 (不用判断是否下架)
            var courseOrgs = new List<(Guid OrgId, Guid? CourseId)>();
            if (cmd.RelatedOrgIds?.Length > 0)
            {
                var sql = $@"
select org.id as Item1,convert(uniqueidentifier,null) as Item2 
from Organization org 
where org.id in @RelatedOrgIds 
"; 
                var orgs = (await unitOfWork.QueryAsync<(Guid OrgId, Guid?)>(sql, new { cmd.RelatedOrgIds }));
                if (cmd.RelatedOrgIds.Any(c => !orgs.Select(_ => _.OrgId).Contains(c)))
                    throw new CustomResponseException("含有无效的品牌");
                else courseOrgs.AddRange(orgs);
            }
            if (cmd.RelatedCourseIds?.Length > 0)
            {
                var sql = $@"
select org.id as Item1,c.id as Item2 
from Organization org 
join Course c on org.id=c.orgid 
where 1=1 and c.id in @RelatedCourseIds 
--and org.IsValid=1 and c.IsValid=1 
--and org.status={OrganizationStatusEnum.Ok.ToInt()} and c.status={CourseStatusEnum.Ok.ToInt()}
";
                var courses = (await unitOfWork.QueryAsync<(Guid OrgId, Guid? CourseId)>(sql, new { cmd.RelatedCourseIds }));
                if (cmd.RelatedCourseIds.Any(c => !courses.Select(_ => _.CourseId).Contains(c)))
                    throw new CustomResponseException("含有无效的商品");
                else courseOrgs.AddRange(courses);
            }

            // EvaluationBind        
            if (true)
            {
                var ls_EvaluationBind = new List<EvaluationBind>(courseOrgs.Count);
                foreach (var (orgId, courseId) in courseOrgs)
                {
                    var dbm_EvaluationBind = new EvaluationBind();
                    dbm_EvaluationBind.IsValid = IsEdit;
                    dbm_EvaluationBind.Id = Guid.NewGuid();
                    dbm_EvaluationBind.Evaluationid = result.Id;
                    dbm_EvaluationBind.Orgid = orgId;
                    dbm_EvaluationBind.Courseid = courseId;
                    ls_EvaluationBind.Add(dbm_EvaluationBind);
                    if (!IsEdit) tbfValid.Add(("[EvaluationBind]", "[id]", dbm_EvaluationBind.Id));
                }
                if (IsEdit)
                {
                    await unitOfWork.DbConnection.ExecuteAsync(@"
                        update [EvaluationBind] set IsValid=0 where Evaluationid=@Evaluationid
                    ", new { Evaluationid = result.Id });
                }
                await unitOfWork.DbConnection.InsertAsync(ls_EvaluationBind);
            }

            // EvaluationItem
            {
                var ls_EvaluationItem = new List<EvaluationItem>();
                {
                    var dbm_EvaluationItem = new EvaluationItem();
                    ls_EvaluationItem.Add(dbm_EvaluationItem);
                    if (!IsEdit)
                    {
                        dbm_EvaluationItem.IsValid = false;
                        dbm_EvaluationItem.Id = Guid.NewGuid();
                        tbfValid.Add(("EvaluationItem", "id", dbm_EvaluationItem.Id));
                    }
                    else
                    {
                        dbm_EvaluationItem.Id = Guid.NewGuid();
                        dbm_EvaluationItem.IsValid = true;
                    }
                    dbm_EvaluationItem.Evaluationid = result.Id;
                    dbm_EvaluationItem.Type = 0;
                    dbm_EvaluationItem.Content = cmd.Ctt1.Content;
                    dbm_EvaluationItem.Pictures = cmd.Ctt1.Pictures.ToJsonString();
                    dbm_EvaluationItem.Thumbnails = cmd.Ctt1.Thumbnails.ToJsonString();
                    dbm_EvaluationItem.Video = cmd.Ctt1.VideoUrl;
                    dbm_EvaluationItem.VideoCover = cmd.Ctt1.VideoCoverUrl;
                }
                if (!IsEdit)
                {
                    //新增
                    await unitOfWork.DbConnection.InsertAsync(ls_EvaluationItem);
                }
                else
                {
                    await unitOfWork.DbConnection.ExecuteAsync($@"
                        update EvaluationItem set IsValid=0 where Evaluationid=@Evaluationid
                    ", new { Evaluationid = result.Id });

                    await unitOfWork.DbConnection.InsertAsync(ls_EvaluationItem);
                }
            }

            // await some tasks 
            if (createCoverTask != null)
            {
                cover = await createCoverTask;
            }

            // Evaluation 目前 先set Evaluation Status to Ok
            var dbm_Evaluation = new Evaluation();
            {
                if (!IsEdit)
                {
                    dbm_Evaluation.IsValid = false;
                    dbm_Evaluation.Stick = false;
                    dbm_Evaluation.ModifyDateTime = dbm_Evaluation.CreateTime = DateTime.Now;
                    dbm_Evaluation.Modifier = dbm_Evaluation.Creator = me.UserId;
                    tbfValid.Add((nameof(Evaluation), nameof(Evaluation.Id), result.Id));
                }
                else
                {
                    dbm_Evaluation.IsValid = true;
                    dbm_Evaluation.ModifyDateTime = dbm_Evaluation.Mtime = DateTime.Now;
                    dbm_Evaluation.Modifier = me.UserId;
                }
                dbm_Evaluation.Id = result.Id;
                dbm_Evaluation.Mode = (byte)cmd.Mode;
                dbm_Evaluation.IsPlaintext = isPlaintext;
                dbm_Evaluation.HasVideo = hasVideo;
                dbm_Evaluation.Cover = cover;                
                dbm_Evaluation.Status = (byte)EvaluationStatusEnum.Ok.ToInt();
                dbm_Evaluation.Title = cmd.Mode == (int)EvltContentModeEnum.Normal ? cmd.Ctt1.Title : null;
                dbm_Evaluation.Userid = me.UserId;
                if (!IsEdit)//新增
                {
                    await unitOfWork.DbConnection.InsertAsync(dbm_Evaluation);
                }
                else
                {
                    var sql = @"
update [dbo].[Evaluation] SET [title]=@Title,[cover]=@Cover,[isPlaintext]=@IsPlaintext,[HasVideo]=@HasVideo,
[mode]=@Mode,[Mtime]=@Mtime,[ModifyDateTime]=@ModifyDateTime,[Modifier]=@Modifier,[ModifyCount]=isnull([ModifyCount],0)+1
where [id]=@Id;
";
                    await unitOfWork.DbConnection.ExecuteAsync(sql, dbm_Evaluation, unitOfWork.DbTransaction);
                }
            }

            // if '!IsEdit' then up some tb IsValid to true
            if (!IsEdit)
            {
                try
                {
                    unitOfWork.BeginTransaction();

                    foreach (var tbf in tbfValid)
                    {
                        await unitOfWork.ExecuteAsync($@"
                            update {tbf.tb} set IsValid=1 where {tbf.idField}=@idValue ;
                        ", new { tbf.idValue }, unitOfWork.DbTransaction);
                    }

                    dbm_Evaluation.No = no = await unitOfWork.ExecuteScalarAsync<long>(
                        @"select No from Evaluation where Id=@Id",
                        new { result.Id },
                        unitOfWork.DbTransaction);

                    unitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    unitOfWork.SafeRollback();
                    throw new CustomResponseException(ex.Message);
                }
            }

            // clear cache
            if (dbm_Evaluation.Status == (byte)EvaluationStatusEnum.Ok.ToInt())
            {
                await mediator.Send(new ClearFrontEvltCacheCommand { EvltId = result.Id });
            }

            result.Id_s = UrlShortIdUtil.Long2Base32(no);

            //
            // 添加后 绑定专题并try参与活动
            //if (!IsEdit && cmd.SpecialId != null)
            { }

            if (after_up_evlt?.Count > 0)
            {
                foreach (var t in after_up_evlt)
                    await (t?.Invoke() ?? Task.CompletedTask);
            }

            // 添加后 发积分?等
            if (!IsEdit)
            {
                AsyncUtils.StartNew((sp, _) => 
                {
                    return sp.GetService<IntegrationEvents.IOrganizationIntegrationEventService>().PublishEventAsync(new IntegrationEvents.Events.AddEvaluationIntegrationEvent
                    {
                        Id = result.Id,
                        UserId = me.UserId,
                        Title = dbm_Evaluation.Title,
                        CreateTime = dbm_Evaluation.CreateTime,
                    });
                }); 
            }

            return result;
        }

        static string GetEvltContent(MiniAddEvaluationCommand cmd)
        {
            return cmd.Mode switch
            {
                (int)EvltContentModeEnum.Normal => cmd.Ctt1.Content,
                _ => ""
            };
        }

        async Task<string> CreateEvltCover(MiniAddEvaluationCommand cmd, Guid evltId)
        {
            var text = GetEvltContent(cmd);
            using var ms = ImgHelper.CreateEvltCover(text, evltCoverCreateOption);
            return await upload_cover(ms, evltId);
        }

        async Task<string> upload_cover(Stream img, Guid evltId)
        {
            var url = config[Consts.BaseUrl_UploadUrl].FormatWith($"eval/{evltId}", $"{Guid.NewGuid()}.png");
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
    }
}
