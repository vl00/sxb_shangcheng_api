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
    public class AddEvaluationCommandHandler : IRequestHandler<AddEvaluationCommand, EvaluationAddedResult>
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

        public AddEvaluationCommandHandler(IOrgUnitOfWork unitOfWork, IUserInfo me, IMediator mediator,
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

        void Valid_cmd(AddEvaluationCommand cmd, bool IsEdit)
        {
            if (!IsEdit)
            {
                cmd.Course ??= new AddEvaluationCommand_CourseEntity();
                if (cmd.Course.CourseId == null)
                {
                    if (cmd.Course.Mode?.Length == 0) cmd.Course.Mode = null;
                }
                if (cmd.OrgId == null && cmd.Course.CourseId != null)
                {
                    throw new CustomResponseException("请选择机构");
                }
                if (cmd.OrgId == null && cmd.OrgName.IsNullOrWhiteSpace())
                {
                    throw new CustomResponseException("请填写自定义机构名字");
                }
                //if (cmd.Course.CourseId == null && cmd.Course.CourseName.IsNullOrWhiteSpace())
                //{
                //    if (cmd.Course.Subject != null || cmd.Course.Age != null || cmd.Course.Mode != null || cmd.Course.Duration != null)
                //        throw new CustomResponseException("请填写自定义课程名字");
                //}
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
                        if (cmd.Ctt1.Pictures.Length > 10)
                            throw new CustomResponseException("图片数量不能超过10张");
                        if (cmd.Ctt1.Content.IsNullOrWhiteSpace())
                            throw new CustomResponseException("正文不能为空");
                    }
                    break;
                case (int)EvltContentModeEnum.Pro:
                    {
                        _ = cmd.Ctt2 ?? throw new CustomResponseException("专业模式内容不应为null");
                        cmd.Ctt2.Steps ??= new EvltContent2Step[0];
                        if (cmd.Ctt2.Title.IsNullOrWhiteSpace())
                            throw new CustomResponseException("标题不能为空");
                        if (cmd.Ctt2.Title.Length > 30)
                            throw new CustomResponseException("标题不能超过30字");
                        var pc = 0;
                        for (var i = 0; i < cmd.Ctt2.Steps.Length; i++)
                        {
                            var c = cmd.Ctt2.Steps[i];
                            c.Pictures ??= new string[0];
                            c.Thumbnails ??= new string[0];
                            if (c.Pictures.Length != c.Thumbnails.Length)
                                throw new CustomResponseException($"第{i + 1}步真实图片数量与缩略图数量不一致");
                            if ((pc += c.Pictures.Length) > 10)
                                throw new CustomResponseException("图片数量不能超过10张");
                            if (c.Content.IsNullOrWhiteSpace())
                                throw new CustomResponseException($"第{i + 1}步正文不能为空");
                        }
                    }
                    break;
            }

            // 目前编辑时保存修改和绑定专题是分开操作的
            if (IsEdit && cmd.SpecialId != null)
            {
                throw new CustomResponseException("参数错误");
            }
            if (IsEdit && !string.IsNullOrEmpty(cmd.Promocode))
            {
                cmd.Promocode = null;
            }
        }

        public async Task<EvaluationAddedResult> Handle(AddEvaluationCommand cmd, CancellationToken cancellation)
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

            if (!IsEdit)
            {
                result.Id = Guid.NewGuid();
            }
            else
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
                    (int)EvltContentModeEnum.Pro => cmd.Ctt2.Steps.Select(_ => _.Content).ToArray(),
                    _ => null
                };
                var trst = await mediator.Send(new SensitiveKeywordCmd { Txts = txts });
                if (!trst.Pass)
                {
                    if ((trst.FilteredTxts?.Length ?? 0) < 1) 
                        throw new CustomResponseException("您发表的测评有敏感词，请修改后再发", ResponseCode.GarbageContent.ToInt());

                    for (var i = 0; i < txts.Length; i++)
                    {
                        switch (cmd.Mode)
                        {
                            case (int)EvltContentModeEnum.Normal:
                                cmd.Ctt1.Content = trst.FilteredTxts[i];
                                break;
                            case (int)EvltContentModeEnum.Pro:
                                cmd.Ctt2.Steps[i].Content = trst.FilteredTxts[i];
                                break;
                        }
                    }
                }
            }

            // 检测活动
            if (!string.IsNullOrEmpty(cmd.Promocode))
            {
                hdInfo = await mediator.Send(new HdDataInfoQuery { Code = cmd.Promocode, CacheMode = 1 });
                var hstatus = hdInfo.GetFrStatus();
                if (hstatus != ActivityFrontStatus.Ok) throw new CustomResponseException("活动" + EnumUtil.GetDesc(hstatus).TrimStart('活').TrimStart('动'));
                //
                // 绑定活动专题时才check ActivityFrontStatus.DayLimited
            }
            //
            // 获取传入的专题            
            if (cmd.SpecialId != null)
            {
                var spcl = await mediator.Send(new GetSpecialInfoQuery { SpecialId = cmd.SpecialId.Value });
                result.SpecialId = spcl.Id;
                result.SpecialId_s = UrlShortIdUtil.Long2Base32(spcl.No);
                result.SpecialName = spcl.Title;
            }
            //
            // 添加时 检查活动和专题是否匹配            
            if (!IsEdit && hdInfo?.Data != null)
            {
                switch (hdInfo.Type)
                {
                    case ActivityType.Hd1:
                    case ActivityType.Hd2:
                        if (cmd.SpecialId != null)
                        {
                            var hds = await mediator.Send(new SpecialActivityQuery { SpecialId = cmd.SpecialId.Value });
                            if (hds?.Any(x => x.Id == hdInfo.Id) != true)
                                throw new CustomResponseException("选择的专题不属于该活动.");
                        }
                        break;
                }
            }
            //
            // 编辑时 检查是否活动评测并检查能否编辑            
            for (var _01 = true; _01 && IsEdit; _01 = false) //&& result.SpecialId != null
            {
                var aeb = await mediator.Send(new ActivityEvltLatestQuery { EvltId = result.Id });
                if (aeb == null || aeb.Status == (byte)ActiEvltAuditStatus.Not)
                    break;

                // 检查能否编辑
                if (aeb.Status == (byte)ActiEvltAuditStatus.Ok)
                {
                    var editable = await mediator.Send(new CheckEvltEditableQuery { EvltId = result.Id, Aeb = aeb });
                    if (!editable.Enable)
                    {
                        var day = Math.Ceiling(editable.DisableTtl?.TotalDays ?? 0);
                        throw new CustomResponseException(day > 0 ? $"活动评测审核成功{day}天内不能编辑." : $"活动评测审核成功后不能编辑.");
                    }
                    break;
                }
                // 修改后原先不通过的评测需要变更提交类型为驳回重交
                after_up_evlt.Add(async () =>
                {
                    if (aeb.Status == (byte)ActiEvltAuditStatus.Failed)
                    {
                        var sql = $"update ActivityEvaluationBind set SubmitType={ActiEvltSubmitType.Retrial.ToInt()},status={ActiEvltAuditStatus.Audit.ToInt()},Mtime=getdate(),IsValid=1,IsLatest=1 where id=@Id";
                        await unitOfWork.ExecuteAsync(sql, new { aeb.Id });
                    }
                });                                
            }

            // 封面图取第一张 
            var cover = cmd.Mode switch
            {
                (int)EvltContentModeEnum.Normal => cmd.Ctt1.Thumbnails.FirstOrDefault(),
                (int)EvltContentModeEnum.Pro => cmd.Ctt2.Steps.FirstOrDefault()?.Thumbnails?.FirstOrDefault(),
                _ => null
            };
            var isPlaintext = string.IsNullOrEmpty(cover);

            // 无图需要生成封面图
            if (isPlaintext)
            {
                createCoverTask = CreateEvltCover(cmd, result.Id);
            }

            // check orgid and courseid
            //!! 在之前已过滤了 `cmd.OrgId == null && cmd.Course.CourseId != null` 不正确的情况
            if (cmd.OrgId != null)
            {
                var sql = $@"
select top 1 org.status as Item1,c.status as Item2 from Organization org with(nolock)
left join Course c with(nolock) on org.id=c.orgid and c.IsValid=1 {"and c.id=@CourseId".If(cmd.Course.CourseId != null)}
where org.IsValid=1 and org.id=@OrgId
";
                var (org_status, c_status) = await unitOfWork.QueryFirstOrDefaultAsync<(byte?, byte?)>(sql, new { cmd.OrgId, cmd.Course.CourseId });
                if (org_status == null) throw new CustomResponseException("选择的机构不存在");
                if (org_status != 1) throw new CustomResponseException("选择的机构已下线");
                if (cmd.Course.CourseId != null)
                {
                    if (c_status == null) throw new CustomResponseException("选择的课程不存在");
                    if (c_status != 1) throw new CustomResponseException("选择的课程已下线");
                }
            }

            // EvaluationBind
            // 自定义机构+课程 暂不需要编辑            
            if (!IsEdit) 
            {
                var dbm_EvaluationBind = new EvaluationBind();
                dbm_EvaluationBind.IsValid = false;
                dbm_EvaluationBind.Id = Guid.NewGuid();
                dbm_EvaluationBind.Evaluationid = result.Id;
                dbm_EvaluationBind.Orgid = cmd.OrgId;
                dbm_EvaluationBind.Orgname = cmd.OrgName;
                mapper.Map(cmd.Course, dbm_EvaluationBind);
                await unitOfWork.DbConnection.InsertAsync(dbm_EvaluationBind);
                tbfValid.Add(("[EvaluationBind]", "[id]", dbm_EvaluationBind.Id));
            }

            // EvaluationItem
            {
                var ls_EvaluationItem = new List<EvaluationItem>();
                switch (cmd.Mode)
                {
                    case (int)EvltContentModeEnum.Normal:
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
                                dbm_EvaluationItem.Id = cmd.Ctt1.Id.Value;
                                dbm_EvaluationItem.IsValid = true;
                            }
                            dbm_EvaluationItem.Evaluationid = result.Id;
                            dbm_EvaluationItem.Type = 0;
                            dbm_EvaluationItem.Content = cmd.Ctt1.Content;
                            dbm_EvaluationItem.Pictures = cmd.Ctt1.Pictures.ToJsonString();
                            dbm_EvaluationItem.Thumbnails = cmd.Ctt1.Thumbnails.ToJsonString();
                        }
                        break;
                    case (int)EvltContentModeEnum.Pro:
                        {
                            byte i = 0;
                            foreach (var ctt in cmd.Ctt2.Steps)
                            {
                                i++;
                                var dbm_EvaluationItem = new EvaluationItem();
                                ls_EvaluationItem.Add(dbm_EvaluationItem);
                                if (!IsEdit)
                                {
                                    dbm_EvaluationItem.Id = Guid.NewGuid();
                                    dbm_EvaluationItem.IsValid = false;
                                    tbfValid.Add(("EvaluationItem", "id", dbm_EvaluationItem.Id));
                                }
                                else
                                {
                                    dbm_EvaluationItem.Id = ctt.Id.Value;
                                    dbm_EvaluationItem.IsValid = true;
                                }
                                dbm_EvaluationItem.Evaluationid = result.Id;
                                dbm_EvaluationItem.Type = i;
                                dbm_EvaluationItem.Content = ctt.Content;
                                dbm_EvaluationItem.Pictures = ctt.Pictures.ToJsonString();
                                dbm_EvaluationItem.Thumbnails = ctt.Thumbnails.ToJsonString();
                            }
                        }
                        break;
                }
                if (!IsEdit)//新增
                    await unitOfWork.DbConnection.InsertAsync(ls_EvaluationItem);
                else
                    await unitOfWork.DbConnection.UpdateAsync(ls_EvaluationItem);
            }

            // await some tasks 
            if (createCoverTask != null)
            {
                cover = await createCoverTask;
            }

            // Evaluation 目前 先set Evaluation Status to Ok
            var dbm_Evaluation = new Evaluation();
            if (!IsEdit)
            {
                dbm_Evaluation.IsValid = false;
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
            dbm_Evaluation.Cover = cover;
            dbm_Evaluation.Stick = false;
            dbm_Evaluation.Status = (byte)EvaluationStatusEnum.Ok.ToInt();
            dbm_Evaluation.Title = cmd.Mode == (int)EvltContentModeEnum.Normal ? cmd.Ctt1.Title :
                cmd.Mode == (int)EvltContentModeEnum.Pro ? cmd.Ctt2.Title : null;
            dbm_Evaluation.Userid = me.UserId;
            if (!IsEdit)//新增
            {
                await unitOfWork.DbConnection.InsertAsync(dbm_Evaluation);
            }
            else
            {
                var sql = @"
UPDATE [dbo].[Evaluation] SET [title]=@Title,[cover]=@Cover,[isPlaintext]=@IsPlaintext,[mode]=@Mode,
[Mtime]=@Mtime,[ModifyDateTime]=@ModifyDateTime,[Modifier]=@Modifier,[ModifyCount]=isnull([ModifyCount],0)+1
WHERE [id]=@Id;
";
                await unitOfWork.DbConnection.ExecuteAsync(sql, dbm_Evaluation, unitOfWork.DbTransaction);
            }

            #region old-codes 旧活动1 insert to ActivityEvaluationBind
            /* if (!IsEdit)
            {                
                if (hdInfo != null && hdInfo.Acode == "h1" && hdInfo.PromoNo != null)
                {
                    var aebind = new ActivityEvaluationBind();
                    aebind.Id = Guid.NewGuid();
                    aebind.IsValid = true;
                    aebind.Activityid = hdInfo.Id;
                    aebind.Evaluationid = result.Id;
                    aebind.Promocode = cmd.Promocode;
                    aebind.Promono = hdInfo.PromoNo;
                    try
                    {
                        await unitOfWork.DbConnection.ExecuteAsync(@"
delete from ActivityEvaluationBind where Activityid=@ActivityId and Evaluationid=@Evaluationid ;
insert ActivityEvaluationBind(id,Activityid,Evaluationid,Promocode,Promono,IsValid) select newid(),@Activityid,@Evaluationid,@Promocode,@Promono,@IsValid ; 
                    ", aebind,
                        unitOfWork.DbTransaction);

                        unitOfWork.CommitChanges();
                    }
                    catch (Exception ex)
                    {
                        try { unitOfWork.Rollback(); } catch { }
                        throw new CustomResponseException(ex.Message);
                    }
                }
            } */
            #endregion old-codes

            // if '!IsEdit' then up some tb IsValid to true
            try
            {
                if (!IsEdit)
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
            }
            catch (Exception ex)
            {
                try { unitOfWork.Rollback(); } catch { }
                throw new CustomResponseException(ex.Message);
            }
           
            // clear cache
            if (dbm_Evaluation.Status == (byte)EvaluationStatusEnum.Ok.ToInt())
            {
                await mediator.Send(new ClearFrontEvltCacheCommand { EvltId = result.Id, SpclId = cmd.SpecialId });
            }

            result.Id_s = UrlShortIdUtil.Long2Base32(no);

            //
            // 添加后 绑定专题并try参与活动
            if (!IsEdit && cmd.SpecialId != null)
            {
                var rr = await mediator.Send(new AddEvaluationToSpecialsCommand { EvltId = result.Id, SpecialId = cmd.SpecialId.Value });
                result.Activity = rr.Activity;
            }

            if (after_up_evlt?.Count > 0)
            {
                foreach (var t in after_up_evlt)
                    await (t?.Invoke() ?? Task.CompletedTask);
            }

            return result;
        }

        static string GetEvltContent(AddEvaluationCommand cmd)
        {
            return cmd.Mode switch
            {
                (int)EvltContentModeEnum.Normal => cmd.Ctt1.Content,
                (int)EvltContentModeEnum.Pro => string.Join('\n', cmd.Ctt2.Steps.Select(_ => _.Content)),
                _ => ""
            };
        }

        async Task<string> CreateEvltCover(AddEvaluationCommand cmd, Guid evltId)
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
