using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
    public class AuditLsPagerQueryHandler : IRequestHandler<AuditLsPagerQuery, AuditLsPagerQueryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        UserUnitOfWork _userUnitOfWork;

        public AuditLsPagerQueryHandler(IOrgUnitOfWork orgUnitOfWork, IUserUnitOfWork userUnitOfWork,
            IMediator mediator)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._userUnitOfWork = (UserUnitOfWork)userUnitOfWork;
            this._mediator = mediator;
        }

        public async Task<AuditLsPagerQueryResult> Handle(AuditLsPagerQuery query, CancellationToken cancellation)
        {
            var result = new AuditLsPagerQueryResult();
            await default(ValueTask);

            var sql = $@"
select aeb.id,aeb.activityid,aeb.evaluationid,e.title,e.CreateTime,e.Mtime,e.userid,a.title as ActiTitle,s.title as SpclTitle,
aeb.submitType,aeb.status as AebStatus,aeb.AuditTime,aeb.Auditor,
a.IsValid as a_isvalid,s.IsValid as s_isvalid,e.IsValid as e_isvalid,aeb.mobile
from ActivityEvaluationBind aeb
left join Activity a on aeb.activityid=a.id and a.IsValid=1
left join Evaluation e on aeb.evaluationid=e.id --and e.IsValid=1
left join Special s on aeb.specialid=s.id and s.IsValid=1
{{1}}
where aeb.IsValid=1 and aeb.IsLatest=1 and e.status={EvaluationStatusEnum.Ok.ToInt()} and e.IsValid=1
and a.type={ActivityType.Hd2.ToInt()} and aeb.status<>{ActiEvltAuditStatus.Not.ToInt()}
{"and e.title like @EvltTitle ".If(!query.EvltTitle.IsNullOrEmpty())}
{(query.SpecialIds?.Length > 0 ? "and s.id in @SpecialIds" : "")}
{"and a.id=@ActivityId".If(!query.ActivityId.In(null, "", "_"))}
{"and aeb.status=@Status".If((query.Status ?? 0) > 0)}
{{0}}
";
            var sql_join = new HashSet<string>();
            var sql_where = new StringBuilder();
            if (!query.UserName.IsNullOrEmpty())
            {
                sql_join.Add("left join [iSchoolUser].dbo.userinfo u on u.id=e.userid and u.channel is null");
                sql_where.AppendLine("and u.nickname like @UserName ");
            }
            sql = string.Format(sql, sql_where, (sql_join.Count < 1 ? "" : string.Join('\n', sql_join)));
            sql = $@"
select count(1) from ({sql}) T
;---
{sql}
order by aeb.Mtime desc,e.CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var dyp = new DynamicParameters(query)
                .Set(nameof(query.EvltTitle), $"%{query.EvltTitle}%")
                .Set(nameof(query.UserName), $"%{query.UserName}%");
            var gr = await _orgUnitOfWork.DbConnection.QueryMultipleAsync(sql, dyp);
            var cc = await gr.ReadFirstAsync<int>();
            var items = (await gr.ReadAsync<AuditLsPagerItemDto>()).AsArray();

            // 用户info
            var usinfos = await _mediator.Send(new UserMobileInfoQuery { UserIds = items.Select(_ => _.UserId).Distinct().ToArray() });
            foreach (var item in items)
            {
                if (!usinfos.TryGetOne(out var u, (_) => _.UserInfo.Id == item.UserId))
                    continue;

                item.UserName = u.UserInfo.NickName;
                item.Mobile = u.UserInfo.Mobile;
                item.UmacCount = (u.OtherUserInfo?.Length ?? 0) + 1;
            }

            // 活动 单篇奖金
            {
                var notok_actis = items.Where(_ => _.AebStatus != (int)ActiEvltAuditStatus.Ok).Select(_ => _.ActivityId).Distinct().ToArray();
                var actis = await GetPriceForOneEvaluation1(notok_actis);
                foreach (var item in items)
                {
                    if (item.AebStatus == (int)ActiEvltAuditStatus.Ok)
                        continue;
                    if (!actis.TryGetOne(out var a, (_) => _.ActivityId == item.ActivityId))
                        continue;
                    item.PriceForOneEvaluation = a.Price;
                }

                var ok_actis = items.Where(_ => _.AebStatus == (int)ActiEvltAuditStatus.Ok).Distinct().ToArray();
                var actis2 = await GetPriceForOneEvaluation2(ok_actis);
                foreach (var item in items)
                {
                    if (item.AebStatus != (int)ActiEvltAuditStatus.Ok)
                        continue;
                    if (!actis2.TryGetOne(out var a, (_) => _.AebId == item.Id))
                        continue;
                    item.PriceForOneEvaluation = a.Price;
                }
            }

            // 审核成功de活动评测de额外奖金
            {
                var ok_actis = items.Where(_ => _.AebStatus == (int)ActiEvltAuditStatus.Ok).Select(_ => _.Id).Distinct().ToArray();
                var actis = await GetActiEvltsExtraBonus2(ok_actis);
                foreach (var item in items)
                {
                    if (item.AebStatus != (int)ActiEvltAuditStatus.Ok)
                        continue;
                    if (!actis.TryGetOne(out var a, (_) => _.AebId == item.Id))
                        continue;
                    item.PriceForExtraBonus = (a.N, a.Prices);
                }
            }

            // 用户领取红包数量(关联活动)
            do
            {
                var u0s = (
                    from a in items
                    from u in usinfos
                    where a.UserId == u.UserInfo.Id
                    select (a.ActivityId, u.UserInfo.Id, u.OtherUserInfo.Select(_ => _.Id).ToArray())
                ).ToArray();

                var uc = await GetUGetRedpCount(u0s);
                foreach (var item in items)
                {
                    if (!uc.TryGetOne(out var c, _ => _.ActivityId == item.ActivityId && _.UserId == item.UserId)) continue;
                    item.UGetRedpCount = c.Count;
                }
            }
            while (false);

            result.PageInfo = items.ToPagedList(query.PageSize, query.PageIndex, cc);
            return result;
        }

        // 活动 单篇奖金(未审核通过)
        async Task<(Guid ActivityId, decimal Price)[]> GetPriceForOneEvaluation1(Guid[] activityIds)
        {
            if (activityIds?.Length < 1) return Array.Empty<(Guid, decimal)>();

            var arr = new (Guid ActivityId, decimal Price)[activityIds.Length];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = (activityIds[i], -1);
            }

            var sql = $@"
select activityid,price from ActivityRule where IsValid=1 and type={ActivityRuleType.SingleBonus.ToInt()} and activityid in @activityIds
";
            var itms = await _orgUnitOfWork.DbConnection.QueryAsync<(Guid, decimal)>(sql, new { activityIds });
            for (var i = 0; i < arr.Length; i++)
            {
                if (!itms.TryGetOne(out var it, _ => _.Item1 == arr[i].ActivityId)) continue;
                arr[i].Price = it.Item2;
            }

            for (var i = 0; i < arr.Length; i++)
            {
                if (arr[i].Price < 0)
                    arr[i].Price = 0;
            }
            return arr;
        }

        // 活动 单篇奖金(审核通过)
        async Task<(Guid AebId, decimal Price)[]> GetPriceForOneEvaluation2(AuditLsPagerItemDto[] aebs)
        {
            if (aebs?.Length < 1) return Array.Empty<(Guid, decimal)>();

            var arr = new (Guid AebId, decimal Price)[aebs.Length];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = (aebs[i].Id, -1);
            }

            var sql = $@"
select o.aebid,r.price,o.adataid,j.ruleid,r.number 
from ActivityEvalMoneyOrder o join ActivityDataHistory h on h.id=o.adataid
outer apply openjson(h.rules) with(ruleid uniqueidentifier '$')j
left join ActivityRule r on r.id=j.ruleid
where o.IsValid=1 and o.aebid in @aebids
and r.type={ActivityRuleType.SingleBonus.ToInt()}
";
            var qs = await _orgUnitOfWork.DbConnection.QueryAsync<(Guid, decimal)>(sql, new { aebids = arr.Select(_ => _.AebId) });
            for (var i = 0; i < arr.Length; i++)
            {
                if (!qs.TryGetOne(out var q, _ => _.Item1 == arr[i].AebId)) continue;
                arr[i].Price = q.Item2;
            }

            for (var i = 0; i < arr.Length; i++)
            {
                if (arr[i].Price < 0)
                    arr[i].Price = 0;
            }
            return arr;
        }

        // 审核成功de活动评测de额外奖金
        async Task<IEnumerable<(Guid AebId, int N, List<decimal> Prices)>> GetActiEvltsExtraBonus2(Guid[] aebIds)
        {
            if (aebIds?.Length < 1) return Array.Empty<(Guid, int, List<decimal>)>();

            var sql = $@"
select o.* from ActivityEvalMoneyOrder o where o.IsValid=1 and o.aebid in @aebIds
";
            var aemos = await _orgUnitOfWork.DbConnection.QueryAsync<ActivityEvalMoneyOrder>(sql, new { aebIds });
            var r = new List<(Guid AebId, int N, List<decimal> Prices)>();
            foreach (var x in aemos)
            {
                if (x.Remark.IsNullOrEmpty()) continue;
                var arr = x.Remark.Split('\n', StringSplitOptions.RemoveEmptyEntries);                
                foreach (var a in arr)
                {
                    var gs = Regex.Match(a, @"^根据活动规则'第(?<n>\d+)篇额外奖金', 用户收益(?<p>[\d\.]+)元.").Groups;
                    var n = int.TryParse(gs["n"].Value, out var _n) ? _n : -1;
                    var p = decimal.TryParse(gs["p"].Value, out var _p) ? _p : -1;
                    if (n < 1) continue;
                    if (!r.TryGetOne(out var y, _ => _.AebId == x.Aebid && _.N == n))
                    {
                        y = (x.Aebid, n, new List<decimal>());
                        r.Add(y);
                    }
                    y.Prices.Add(p);
                }
            }
            return r;
        }

        // 用户领取红包数量(关联活动)
        async Task<(Guid ActivityId, Guid UserId, int Count)[]> GetUGetRedpCount((Guid ActivityId, Guid UserId, Guid[] OtherUserIds)[] u0s)
        {
            if (u0s?.Length < 1) return new (Guid, Guid, int)[0];

            var rr = u0s.Select(x => (x.ActivityId, x.UserId, Count: 0)).ToArray();

            var sql = @"
select activityid,userid,count(1)as [count],sum(money)as [money] from ActivityEvalMoneyOrder 
where IsValid=1 and [orderstatus]%3=0 and ({0})
group by activityid,userid
";
            var sb = new List<string>();
            foreach (var x in u0s)
            {
                sb.Add($"(activityid='{x.ActivityId}' and userid in (" +
                    string.Join(',', Enumerable.Repeat($"'{x.UserId}'", 1).Union(x.OtherUserIds.Select(_ => $"'{_}'"))) +
                    $"))");
            }
            sql = string.Format(sql, string.Join(" or ", sb));
            var ls = await _orgUnitOfWork.DbConnection.QueryAsync<(Guid, Guid, int, decimal)>(sql);
            
            for (var i = 0; i < rr.Length; i++)
            {
                if (!u0s.TryGetOne(out var u, (_) => _.ActivityId == rr[i].ActivityId && _.UserId == rr[i].UserId)) continue;
                rr[i].Count = ls.Where(_ => _.Item1 == u.ActivityId && u.OtherUserIds.Append(u.UserId).Contains(_.Item2)).Sum(_ => _.Item3);
            }

            return rr;
        }
    }
}
