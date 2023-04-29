using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
    public class ActiAuditCommandHandler : IRequestHandler<ActiAuditCommand, ActiAuditCommandResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        ISxbGenerateNo _gendNo;
        CSRedisClient redis;

        public ActiAuditCommandHandler(IOrgUnitOfWork orgUnitOfWork, CSRedisClient redis,
            ISxbGenerateNo gendNo,
            IMediator mediator)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this._gendNo = gendNo;
            this.redis = redis;
        }

        public async Task<ActiAuditCommandResult> Handle(ActiAuditCommand cmd, CancellationToken cancellation)
        {
            var result = new ActiAuditCommandResult();
            ActivityEvaluationBind aeb = default!;
            var cacheToDel = new List<(string, string)>();
            await default(ValueTask);

            // valid cmd
            do
            {
                if (cmd.IsPass && (!cmd.Adesc.IsNullOrEmpty() || cmd.Areply != null))
                {
                    result.Errcode = 4;
                    result.Errmsg = "参数错误";
                    return result;
                }
                if (!cmd.IsPass && cmd.Adesc.IsNullOrEmpty())
                {
                    result.Errcode = 4;
                    result.Errmsg = "请填写审核意见";
                    return result;
                }                
            }
            while (false);

            aeb = await _mediator.Send(new ActivityEvltLatestQuery { EvltId = cmd.EvltId });
            if (aeb?.Id != cmd.AebId)
            {
                result.Errcode = 4;
                result.Errmsg = "参数错误";
                return result;
            }
            if (aeb.Status.In((byte)ActiEvltAuditStatus.Not, null))
            {
                result.Errcode = 5;
                result.Errmsg = "已脱离活动";
                return result;
            }
            if (!(aeb.Status).In((byte)ActiEvltAuditStatus.Audit, (byte)ActiEvltAuditStatus.AuditButMoblieExcp))
            {
                result.Errcode = 6;
                result.Errmsg = "已审核";
                return result;
            }

            // 检查活动?
            /* var hd = await _mediator.Send(new HdDataInfoQuery { Id = aeb.Activityid, CacheMode = -1 });
            var hd_status = hd.GetFrStatus();
            if (hd_status != ActivityFrontStatus.Ok)
            {
                result.Errcode = 1;
                result.Errmsg = $"活动{EnumUtil.GetDesc(hd_status)}";
                return result;
            } //*/

            var heldLck = (K: $"org:lck:{60 * 10}-{90}:hd2audit:hd2{aeb.Activityid}_evlt{cmd.EvltId}", Id: Guid.NewGuid(), Ttl: 60 * 10);
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
                    throw new CustomResponseException("系统繁忙", 21111);
            }
            try
            {
                // 驳回
                if (!cmd.IsPass)
                {
                    aeb.Status = (byte)ActiEvltAuditStatus.Failed;
                    aeb.AuditDesc = cmd.Adesc;
                    aeb.Reply = cmd.Areply;
                    aeb.AuditTime = DateTime.Now;
                    aeb.Auditor = cmd.AuditorId;
                    await Set_aeb(result, aeb, true);
                    // after
                }
                // 审核成功
                else
                {
                    Debugger.Break();
                    var evlt = await _mediator.Send(new GetEvltBaseInfoQuery { EvltId = aeb!.Evaluationid });
                    if (evlt == null)
                    {
                        result.Errcode = 1;
                        result.Errmsg = "评测已下架";
                        goto LB_end;
                    }

                    // 检查手机号有无冲突
                    var uc = await _mediator.Send(new UserHd2ActiQuery { ActivityId = aeb.Activityid, UserId = evlt.AuthorId });
                    if (uc.Ocount > 0)
                    {
                        aeb.Status = (byte)ActiEvltAuditStatus.AuditButMoblieExcp;
                        await Set_aeb(result, aeb, false);
                        result.Errcode = 2;
                        result.Errmsg = "手机号冲突";
                        goto LB_end;
                    }

                    //
                    // do审核成功 计算用户得到的收益
                    //
                    var (adata, rules) = await Get_curr_rules(aeb!.Activityid);
                    var remark = new StringBuilder();
                    var money = 0m;
                    // 单篇奖金
                    do
                    {
                        if (rules.TryGetOne(out var rule, _ => _.Type == (byte)ActivityRuleType.SingleBonus))
                        {
                            if (rule.Price > 0)
                            {
                                money += rule.Price.Value;
                                remark.AppendLine($"根据活动规则'单篇奖金', 用户收益{rule.Price}元.");
                            }
                        }
                    }
                    while (false);
                    // 第N篇额外奖金 第N篇=成功审核的第几篇
                    do
                    {
                        var r3s = rules.Where(_ => _.Type == (byte)ActivityRuleType.ExtraBonus).OrderBy(_ => _.Number);
                        if (!r3s.Any()) break;
                        var theN = -1;
                        {
                            var sql = $@"
select count(1)+1 from ActivityEvalMoneyOrder o where o.IsValid=1 --and o.orderstatus%3=0
and activityid=@Activityid and o.userid in @userids
";
                            theN = (await _orgUnitOfWork.DbConnection.ExecuteScalarAsync<int?>(sql, new 
                            {
                                aeb.Activityid,
                                userids = uc.OtherUserIds.Append(uc.UserId),
                            })) ?? -1;
                        }
                        foreach (var rule in r3s)
                        {                            
                            if (theN == rule.Number && rule.Price > 0)
                            {
                                money += rule.Price.Value;
                                remark.AppendLine($"根据活动规则'第{rule.Number}篇额外奖金', 用户收益{rule.Price}元.");
                            }
                        }
                    } while (false);
                    //
                    // more rules ...
                    //
                    // 活动预算与支出
                    do
                    {
                        if (!rules.TryGetOne(out var rule, _ => _.Type == (byte)ActivityRuleType.StopOrKeepActivity))
                            break;
                        if (rule.Number != 2)
                            break;
                        if (adata.Budget == null)
                            break;

                        // 是否超过预算,超过提前结束活动
                        var sql = $@"
select sum([Money]) from ActivityEvalMoneyOrder where IsValid=1 and Activityid=@Activityid and [orderstatus]%3=0
";
                        var moneyOuted = (await _orgUnitOfWork.DbConnection.ExecuteScalarAsync<decimal?>(sql, new { adata.Activityid }) ?? 0m);
                        if ((moneyOuted + money) < (adata.Budget.Value)) break;

                        remark.AppendLine($"根据活动规则'参加内容预计金额达到上限停止活动','{adata.Title}'提前下架.");

                        sql = $@"
update Activity set [status]={ActivityStatus.Fail.ToInt()},Modifier=@AuditorId,ModifyDateTime=getdate() where id=@Activityid and [status]={ActivityStatus.Ok.ToInt()}
";
                        var i = await _orgUnitOfWork.DbConnection.ExecuteAsync(sql, new { adata.Activityid, cmd.AuditorId });
                        if (i < 1)
                        {
                            result.Errcode = 1;
                            result.Errmsg = "活动已下线";
                            goto LB_end;
                        }

                        cacheToDel.Add((CacheKeys.ActivitySimpleInfo.FormatWith(adata.Activityid), null));
                        cacheToDel.Add((CacheKeys.simplespecial_acd.FormatWith(adata.Activityid), null));
                        cacheToDel.Add((CacheKeys.simplespecial, null));
                    }
                    while (false);
                    // 审核通过后N天内不允许用户修改删除评测
                    do
                    {
                        if (!rules.TryGetOne(out var rule, _ => _.Type == (byte)ActivityRuleType.OperationNotAllowed))
                            break;
                        if ((rule.Number ?? 0) < 1)
                            break;

                        _ = redis.SetAsync(CacheKeys.Editdisable_evlt.FormatWith(cmd.EvltId), 1, 
                            (int)TimeSpan.FromDays(rule.Number.Value).TotalSeconds, RedisExistence.Nx);
                    }
                    while (false);

                    // update to db
                    do
                    {
                        aeb.Status = (byte)ActiEvltAuditStatus.Ok;
                        aeb.Auditor = cmd.AuditorId;
                        aeb.AuditTime = DateTime.Now;
                        await Set_aeb(result, aeb, true);
                        if (result.Errcode != 0) break;

                        var dbm_ActivityEvalMoneyOrder = new ActivityEvalMoneyOrder
                        {
                            Id = Guid.NewGuid(),
                            Aebid = aeb.Id,
                            Activityid = aeb.Activityid,
                            Userid = uc.UserId,
                            Adataid = adata.Id,
                            IsValid = true,
                            Creator = cmd.AuditorId,
                            CreateTime = aeb.AuditTime ?? DateTime.Now,
                            Money = money,
                            Remark = remark.ToString(),
                            Orderstatus = (byte)ActiEvltMoneyOrderStatus.PayedManually,
                            Orderno = _gendNo.GetNumber()
                        };
                        await _orgUnitOfWork.DbConnection.InsertAsync(dbm_ActivityEvalMoneyOrder);
                    }
                    while (false);
                }
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
            
            LB_end:

            if (cacheToDel.Count > 0)
            {
                await Task.WhenAny(Task.Delay(1000 * 3), redis.BatchDelAsync(cacheToDel, 5));
            }

            return result;
        }

        async Task Set_aeb(ActiAuditCommandResult result, ActivityEvaluationBind aeb, bool strict)
        {
            var sql = $@"
update ActivityEvaluationBind set [status]=@Status,[auditDesc]=@AuditDesc,[reply]=@Reply,[AuditTime]=@AuditTime,[Auditor]=@Auditor
where id=@Id {"and IsValid=1 and IsLatest=1".If(strict)} 
";            
            var i = await _orgUnitOfWork.DbConnection.ExecuteAsync(sql, aeb);
            if (i < 1) result.Errcode = 1;
        }

        async Task<(ActivityDataHistory, ActivityRule[])> Get_curr_rules(Guid activityId)
        {
            var sql = "select top 1 * from ActivityDataHistory where activityid=@activityId order by ModifyDateTime desc";
            var adh = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<ActivityDataHistory>(sql, new { activityId });
            if (adh == null) throw new CustomResponseException("没有活动记录");
            var ruleIds = adh.Rules.ToObject<Guid[]>();
            if (ruleIds?.Length < 1) return (adh, Array.Empty<ActivityRule>());

            sql = $"select * from [{nameof(ActivityRule)}] where id in @ruleIds and activityid=@activityId";
            var rules = (await _orgUnitOfWork.DbConnection.QueryAsync<ActivityRule>(sql, new { activityId, ruleIds })).AsArray();
            if (ruleIds.Length != rules.Length) throw new CustomResponseException("活动规则错误");
            return (adh, rules);
        }
    }
}
