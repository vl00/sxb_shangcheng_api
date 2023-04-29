using CSRedis;
using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace iSchool.Organization.Appliaction.Service.Specials
{
    public class AddEvaluationToSpecialsHandler : IRequestHandler<AddEvaluationToSpecialsCommand, AddEvaluationToSpecialsCommandResult>
    {
        IRepository<SpecialBind> _specialbindRepository;        
        IUserInfo me;
        IMediator _mediator;
        CSRedisClient redis;
        OrgUnitOfWork orgUnitOfWork;
        IConfiguration config;

        public AddEvaluationToSpecialsHandler(IRepository<SpecialBind> specialbindRepository,             
            IUserInfo me, CSRedisClient redis, IOrgUnitOfWork orgUnitOfWork, IConfiguration config,
            IMediator mediator)
        {
            _specialbindRepository = specialbindRepository;
            this.me = me;
            _mediator = mediator;
            this.redis = redis;
            this.orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
            this.config = config;
        }

        public async Task<AddEvaluationToSpecialsCommandResult> Handle(AddEvaluationToSpecialsCommand cmd, CancellationToken cancellationToken)
        {
            var result = new AddEvaluationToSpecialsCommandResult { Succeed = true };
            var now = DateTime.Now;
            (userinfo UserInfo, userinfo[] OtherUserInfo) my = default;
            var cachesToDel = new List<(string, string)>();
            
            var evlt = await _mediator.Send(new GetEvltBaseInfoQuery { EvltId = cmd.EvltId });            
            if (evlt == null) throw new CustomResponseException("当前状态不能添加专题！");
            if (evlt.AuthorId != me.UserId)
            {
                Debugger.Break(); // UserInfo.Mock()
                throw new AuthResponseException();
            }
            
            var new_spcl = await _mediator.Send(new GetSpecialInfoQuery { SpecialId = cmd.SpecialId });
            if (new_spcl == null) throw new CustomResponseException("专题不存在！");
            if (evlt.SpecialId == new_spcl.Id) // same spcl ..
            {
                Debugger.Break(); // UserInfo.Mock()
                return result;
            }

            // 旧专题的活动         
            var old_aeb = await _mediator.Send(new ActivityEvltLatestQuery { EvltId = cmd.EvltId });
            var old_is_hd = !(old_aeb?.Status).In(null, (byte)ActiEvltAuditStatus.Not);
            var old_hd = !old_is_hd ? null : await _mediator.Send(new HdDataInfoQuery { Id = old_aeb.Activityid, CacheMode = -1 });
            // 旧活动不是正常时,视为非活动
            old_hd = old_hd == null ? null : old_hd.GetFrStatus(now) == ActivityFrontStatus.Ok ? old_hd : null;
            // 新专题的活动
            var new_hd = (await _mediator.Send(new SpecialActivityQuery { SpecialId = cmd.SpecialId }))?.FirstOrDefault();
            if (new_hd?.GetFrStatus(now) is ActivityFrontStatus new_hd_status && new_hd_status != ActivityFrontStatus.Ok)
            {
                throw new CustomResponseException("活动" + EnumUtil.GetDesc(new_hd_status).TrimStart('活').TrimStart('动'));
            }

            // 检查能否编辑
            if (old_is_hd)
            {
                var editable = await _mediator.Send(new CheckEvltEditableQuery { EvltId = cmd.EvltId, Aeb = old_aeb });
                if (!editable.Enable)
                {
                    var day = Math.Ceiling(editable.DisableTtl?.TotalDays ?? 0);
                    throw new CustomResponseException(day > 0 ? $"活动评测审核成功{day}天内不能编辑." : $"活动评测审核成功后不能编辑.");
                }
            }

            var heldLck = (K: $"org:lck:{60 * 10}-{90}:evlt:{cmd.EvltId}:bind_spcl", Id: Guid.NewGuid(), Ttl: 60 * 10);
            try
            {
                if (!(await redis.SetAsync(heldLck.K, heldLck.Id, heldLck.Ttl, RedisExistence.Nx)))
                    heldLck.K = default;
            }
            catch
            {
                heldLck.K = default;
            }
            finally
            {
                if (heldLck.K == default)
                    throw new CustomResponseException("系统繁忙", 201);
            }
            try
            {
                // 绑定专题
                {
                    var bind = _specialbindRepository.Get(p => p.IsValid == true && p.Evaluationid == cmd.EvltId);
                    if (bind == null)
                    {
                        bind = new SpecialBind
                        {
                            Id = Guid.NewGuid(),
                            Evaluationid = cmd.EvltId,
                            Specialid = cmd.SpecialId,
                            IsValid = true
                        };

                        _ = _specialbindRepository.Insert(bind);
                    }
                    else if (bind.Specialid != cmd.SpecialId)
                    {
                        bind.Specialid = cmd.SpecialId;
                        _specialbindRepository.Update(bind);
                    }

                    await redis.BatchDelAsync((string.Format(CacheKeys.Evlt, cmd.EvltId), "base"));
                    cachesToDel.Add((string.Format(CacheKeys.Evlt, cmd.EvltId) + ":*", null));
                    cachesToDel.Add((CacheKeys.Rdk_spcl.FormatWith(cmd.SpecialId), null));
                    cachesToDel.Add((CacheKeys.Rdk_spcl.FormatWith(cmd.SpecialId) + ":*", null));
                    cachesToDel.Add((CacheKeys.Rdk_spcl.FormatWith("*"), null));
                    var cachesToDel1 = cachesToDel.ToList();                    
                    AsyncUtils.StartNew((sp, _1) => sp.GetService<CSRedisClient>().BatchDelAsync(cachesToDel1, 5));
                    cachesToDel.Clear();
                }

                // try参与活动 ActivityType.Hd2
                do
                {
                    //* 旧非活动专题 to 新非活动专题
                    if (old_hd?.Type != ActivityType.Hd2 && new_hd?.Type != ActivityType.Hd2)
                    {
                        break;
                    }

                    // get我的账号信息
                    if (old_hd?.Type == ActivityType.Hd2 || new_hd?.Type == ActivityType.Hd2)
                    {
                        my = (await _mediator.Send(new UserMobileInfoQuery { UserIds = new[] { me.UserId } })).FirstOrDefault();
                        if (Equals(my, null) || my.UserInfo == null) throw new CustomResponseException("查询用户信息异常");
                    }

                    //* 旧活动专题 to 新非活动专题
                    if (old_hd?.Type == ActivityType.Hd2 && new_hd?.Type != ActivityType.Hd2)
                    {
                        var sql = $@"
update ActivityEvaluationBind set IsLatest=0 where evaluationid=@evaluationid and IsLatest=1
;;
insert ActivityEvaluationBind(id,activityid,evaluationid,mobile,SubmitType,status,IsValid,IsLatest,Mtime,specialid,AuditTime,Auditor,AuditDesc,Reply)
    values(@id,@Activityid,@evaluationid,@mobile,@SubmitType,@status,1,1,@Mtime,@specialid,@AuditTime,@Auditor,@AuditDesc,@Reply)
";
                        await orgUnitOfWork.DbConnection.ExecuteAsync(sql, new ActivityEvaluationBind
                        {
                            Id = Guid.NewGuid(),
                            Activityid = Guid.Empty,
                            Evaluationid = evlt.Id,
                            Mobile = my.UserInfo.Mobile,
                            Mtime = now,
                            Specialid = new_spcl.Id,
                            Status = (byte)ActiEvltAuditStatus.Not,
                            SubmitType = (byte)(old_aeb.Status == (byte)ActiEvltAuditStatus.Failed ? ActiEvltSubmitType.Retrial : ActiEvltSubmitType.Multi),
                            AuditTime = old_aeb?.AuditTime,
                            Auditor = old_aeb?.Auditor,
                            AuditDesc = old_aeb?.AuditDesc,
                            Reply = old_aeb?.Reply,
                        });

                        break;
                    }

                    //
                    // check 手机号异常 or 到达活动上限
                    // 如果手机号异常, 那么绑定新活动专题后 aeb.Status直接到 ActiEvltAuditStatus.AuditButMoblieExcp
                    if (new_hd?.Type == ActivityType.Hd2)
                    {
                        result.Activity = new EvaluationAddedResult_NewActivity { Code = new_hd.Acode, Status = ActivityFrontStatus.Ok.ToInt() };
                        var uc = await _mediator.Send(new UserHd2ActiQuery
                        {
                            ActivityId = new_hd!.Id,
                            UserId = me.UserId,
                            OtherUserIds = my.OtherUserInfo.Select(_ => _.Id).ToArray(),
                            Now = now
                        });
                        if (new_hd.Data.Limit != null && (uc.Allcount_now == 0 ? 1 : uc.Allcount_now) >= new_hd.Data.Limit)
                            result.Activity.Status = ActivityFrontStatus.DayLimited.ToInt();
                        if (uc.Ocount > 0)
                            result.Activity.Ustatus = UserAccountInvalidType.MobileExcp.ToInt();
                    }

                    //* 旧非活动专题 to 新活动专题
                    if (old_hd?.Type != ActivityType.Hd2 && new_hd.Type == ActivityType.Hd2)
                    {
                        var sql = $@"
update ActivityEvaluationBind set IsLatest=0 where evaluationid=@evaluationid and IsLatest=1
;;
insert ActivityEvaluationBind(id,activityid,evaluationid,mobile,SubmitType,status,IsValid,IsLatest,Mtime,specialid,AuditTime,Auditor,AuditDesc,Reply)
    values(@id,@Activityid,@evaluationid,@mobile,@SubmitType,@status,1,1,@Mtime,@specialid,@AuditTime,@Auditor,@AuditDesc,@Reply)
";
                        await orgUnitOfWork.DbConnection.ExecuteAsync(sql, new ActivityEvaluationBind
                        {
                            Id = Guid.NewGuid(),
                            Activityid = new_hd.Id,
                            Evaluationid = evlt.Id,
                            Mobile = my.UserInfo.Mobile,
                            Mtime = now,
                            Specialid = new_spcl.Id,
                            SubmitType = old_aeb == null ? (byte)ActiEvltSubmitType.First : (old_aeb.SubmitType ?? (byte)ActiEvltSubmitType.Multi),
                            Status = (byte)(result.Activity.Ustatus == (int)UserAccountInvalidType.MobileExcp ? ActiEvltAuditStatus.AuditButMoblieExcp : ActiEvltAuditStatus.Audit),
                            AuditTime = old_aeb?.AuditTime,
                            Auditor = old_aeb?.Auditor,
                            AuditDesc = old_aeb?.AuditDesc,
                            Reply = old_aeb?.Reply,
                        });

                        break;
                    }

                    //* 旧活动专题 to 新活动专题
                    if (old_hd?.Type == ActivityType.Hd2 && new_hd.Type == ActivityType.Hd2)
                    {
                        // 只有修改活动评测才会update ActivityEvaluationBind
                        //
                        var sql = $@"
update ActivityEvaluationBind set IsLatest=0 where evaluationid=@evaluationid and IsLatest=1
;;
insert ActivityEvaluationBind(id,activityid,evaluationid,mobile,SubmitType,status,IsValid,IsLatest,Mtime,specialid,AuditTime,Auditor,AuditDesc,Reply)
    values(@id,@Activityid,@evaluationid,@mobile,@SubmitType,@status,1,1,@Mtime,@specialid,@AuditTime,@Auditor,@AuditDesc,@Reply)
";
                        var status = result.Activity.Ustatus == (int)UserAccountInvalidType.MobileExcp ? ActiEvltAuditStatus.AuditButMoblieExcp :
                            old_aeb.Status == (byte)ActiEvltAuditStatus.Failed ? ActiEvltAuditStatus.Failed : ActiEvltAuditStatus.Audit;
                        var submitType = old_aeb.Status == (byte)ActiEvltAuditStatus.Failed ? ActiEvltSubmitType.Retrial : ActiEvltSubmitType.Multi;
                        var new_aeb = new ActivityEvaluationBind
                        {
                            Id = Guid.NewGuid(),
                            Activityid = new_hd.Id,
                            Evaluationid = evlt.Id,
                            Mobile = my.UserInfo.Mobile,
                            Mtime = now,
                            Specialid = new_spcl.Id,
                            Status = (byte)(status),
                            SubmitType = (byte)(submitType),
                            AuditTime = old_aeb?.AuditTime,
                            Auditor = old_aeb?.Auditor,
                            AuditDesc = old_aeb?.AuditDesc,
                            Reply = old_aeb?.Reply,
                        };
                        await orgUnitOfWork.DbConnection.ExecuteAsync(sql, new_aeb);

                        break;
                    }
                }
                while (false);
            }
            finally
            {
                if (heldLck.K != default)
                {
                    try
                    {
                        if (heldLck.Id == (await redis.GetAsync<Guid>(heldLck.K)))
                            await redis.DelAsync(heldLck.K);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            if (cachesToDel.Count > 0)
            {
                var cachesToDel1 = cachesToDel.ToList();
                AsyncUtils.StartNew((sp, _1) => sp.GetService<CSRedisClient>().BatchDelAsync(cachesToDel1, 5));
                cachesToDel.Clear();
            }

            // for hd2
            for (var _01 = true; _01 && result.Activity != null; _01 = false)
            {
                if (result.Activity.Ustatus == (int)UserAccountInvalidType.MobileExcp) break;
                var path = result.Activity.Status == (int)ActivityFrontStatus.DayLimited 
                    ? Path.Combine(Directory.GetCurrentDirectory(), config[$"AppSettings:hd2:hd_daylimited_qrcode"])
                    : Path.Combine(Directory.GetCurrentDirectory(), config[$"AppSettings:hd2:hd_tile_qrcode"]);
                var bys = await File.ReadAllBytesAsync(path);
                result.Activity.Qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }
            
            return result;
        }
    }
}
