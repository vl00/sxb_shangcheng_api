using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class PcEvaluationIndexQueryHandler : IRequestHandler<PcEvaluationIndexQuery, PcEvaluationIndexQueryResult>
    {
        IConfiguration config;
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;

        public PcEvaluationIndexQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IUserInfo me,            
            IMapper mapper, IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<PcEvaluationIndexQueryResult> Handle(PcEvaluationIndexQuery query, CancellationToken cancellation)
        {
            var result = new PcEvaluationIndexQueryResult();
            result.Me = me.IsAuthenticated ? me : null;
            result.Subjs = GetSubjs().ToList();
            var subjs = result.Subjs as List<(string Name, string Id)>;

            result.Specials = await mediator.Send(new SimpleSpecialQuery { });           
            if (query.R == 1)
            {                
                subjs.RemoveAll(_ => _.Name == "评测精选");
                subjs.Insert(0, ("全部", ""));
            }
            if (query.OrgNo != null)
            {
                var org = await mediator.Send(new OrgzBaseInfoQuery { No = query.OrgNo.Value });
                result.OrgInfo = mapper.Map<PcOrgItemDto>(org);

                // 不能通过'org.Subjects'判断机构的有评测的科目, 因为发评测时可以自定义课程.
                {
                    /*if (org.Subjects?.ToObject<string[]>() is IEnumerable<string> org_subjs)
                    {
                        org_subjs = org_subjs.Distinct().Union(new[] { "", "0", SubjectEnum.Other.ToInt().ToString() }).AsArray();
                        var subjs = result.Subjs as List<(string Name, string Id)>;
                        subjs.RemoveAll(_ => !org_subjs.Contains(_.Id));
                    }*/

                    var rdk = CacheKeys.PC_EvltsMain_subjs.FormatWith(result.OrgInfo.Id);
                    var org_subjs = await redis.GetAsync<List<string>>(rdk);
                    if (org_subjs == null)
                    {
                        var sql = $"select isnull(subject,{SubjectEnum.Other.ToInt()}) from EvaluationBind where IsValid=1 and orgid=@Id group by isnull(subject,{SubjectEnum.Other.ToInt()})";
                        org_subjs = (await unitOfWork.QueryAsync<string>(sql, new { result.OrgInfo.Id })).AsList();
                        await redis.SetAsync(rdk, org_subjs, 60 * 5);
                    }
                    org_subjs.AddRange(new[] { "", " ", "-1", "0" });
                    subjs.RemoveAll(_ => !org_subjs.Contains(_.Id));
                }
            }

            result.PageInfo = await GetPagedList(query, result.OrgInfo);

            return result;
        }

        async Task<PagedList<EvaluationItemDto>> GetPagedList(PcEvaluationIndexQuery query, PcOrgItemDto orgInfo)
        {
            IEnumerable<EvaluationItemDto> items = null;
            int cc = 0;
            await default(ValueTask);

            IEnumerable<int> subj_arr = null;
            IEnumerable<int> age_arr = null;
            var idx_other = false;
            var is_recommend = query.Stick == 1;
            if (!is_recommend)
            {
                subj_arr = query.Subj?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.TryParse(x, out var _x) ? _x : 0).Distinct()
                    .Where(x => x > -2 && !x.In(0))
                    .ToArray();

                age_arr = query.Age?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.TryParse(x, out var _x) ? _x : 0).Distinct()
                    .Where(x => x > -2 && !x.In(0))
                    .ToArray();

                idx_other = subj_arr?.Count() == 1 && subj_arr.ElementAt(0).In(-1, SubjectEnum.Other.ToInt());
            }

            var rdk = query.OrgNo == null ? CacheKeys.PC_EvltsMain.FormatWith(query.PageSize,
                    string.Join(',', subj_arr ?? Enumerable.Empty<int>()),
                    string.Join(',', age_arr ?? Enumerable.Empty<int>()),
                    query.Stick)
                : CacheKeys.PC_EvltsMain2.FormatWith(query.PageSize,
                    string.Join(',', subj_arr ?? Enumerable.Empty<int>()),
                    string.Join(',', age_arr ?? Enumerable.Empty<int>()),
                    query.Stick,
                    orgInfo!.Id);

            if (query.PageIndex == 1)
            {
                var jtk = await redis.GetAsync<JToken>(rdk);
                if (jtk != null)
                {
                    cc = (int)jtk["totalItemCount"];
                    items = jtk["page1_items"].ToObject<EvaluationItemDto[]>();
                }
            }
            if (items == null) // find in db sql
            {
                var sql = $@"
select evlt.*
from Evaluation evlt {{0}}
where evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} --and evlt.isPlaintext=@isPlaintext 
{{1}} ";
                var sql_join = new HashSet<string>();
                var where = "";
                if (is_recommend) //推荐|精华
                {
                    where += "and evlt.stick=1 ";
                }
                if (idx_other) //其他是排除主页显示的n个科目
                {
                    where += $@"
and (exists(select 1 from EvaluationBind b where b.IsValid=1 
	and b.evaluationid=evlt.id and (b.subject is null or b.subject not in({string.Join(',', GetMainSubj())})) and b.courseid is null)
or exists(select 1 from EvaluationBind b join Course c on c.id=b.courseid and c.IsValid=1 --and c.status={CourseStatusEnum.Ok.ToInt()}
	where b.IsValid=1 and b.evaluationid=evlt.id and b.courseid is not null and (c.subject is null or c.subject not in({string.Join(',', GetMainSubj())})))
) ";
                }
                else if (subj_arr?.Any() == true)
                {
                    where += $@"
and (exists(select 1 from EvaluationBind b where b.IsValid=1 
	and b.evaluationid=evlt.id and b.subject in ({string.Join(',', subj_arr)}) and b.courseid is null)
 or exists(select 1 from EvaluationBind b join Course c on c.id=b.courseid and c.IsValid=1 --and c.status={CourseStatusEnum.Ok.ToInt()}
	where b.IsValid=1 and b.evaluationid=evlt.id and b.courseid is not null and c.subject in ({string.Join(',', subj_arr)}))
)";
                }
                if (age_arr?.Any() == true)
                {
                    where += $@"
and (exists( select 1 from EvaluationBind b where b.IsValid=1 
    and b.evaluationid = evlt.id and b.age in ({string.Join(',', age_arr)}) and b.courseid is null )
 or exists( select 1 from EvaluationBind b join Course c on c.id=b.courseid and c.IsValid=1 --and c.status={CourseStatusEnum.Ok.ToInt()}
    where b.IsValid=1 and b.evaluationid=evlt.id and b.courseid is not null {Fmt_Age_SqlWhere(age_arr, "c.minage", "c.maxage")} )
)";
                }
                if (query.OrgNo != null)
                {
                    sql_join.Add("left join EvaluationBind eb on eb.IsValid=1 and eb.evaluationid=evlt.id");
                    sql_join.Add($"left join Organization org on org.IsValid=1 and eb.orgid=org.id and org.status={OrganizationStatusEnum.Ok.ToInt()}");
                    where += "and org.id=@OrgId";
                }

                sql = string.Format(sql, (sql_join.Count < 1 ? "" : string.Join('\n', sql_join)), where);
                sql = $@"
select count(1) from ({sql}) T
;
{sql}
order by {"evlt.stick desc,".If(is_recommend)}evlt.viewcount desc,evlt.CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
                var dyp = new DynamicParameters(query)
                    .Set("@OrgId", orgInfo?.Id);
                var gr = await unitOfWork.QueryMultipleAsync(sql, dyp);
                cc = await gr.ReadFirstAsync<int>();
                items = (await gr.ReadAsync<Evaluation>()).Select(evlt => mapper.Map<EvaluationItemDto>(evlt)).AsArray();

                if (query.PageIndex == 1)
                {
                    await redis.SetAsync(rdk, new { totalItemCount = cc, page1_items = items }, 60 * 15);
                }
            }
            var pg = items.ToPagedList(query.PageSize, query.PageIndex, cc);

            // user info
            var rr = await mediator.Send(new UserSimpleInfoQuery { UserIds = items.Select(_ => _.AuthorId).Distinct() });
            foreach (var item in items)
            {
                var r = rr.FirstOrDefault(_ => _.Id == item.AuthorId);
                if (r == null) continue;
                item.AuthorName = r.Nickname;
                item.AuthorHeadImg = r.HeadImgUrl ?? config["AppSettings:UserDefaultHeadImg"];
            }

            // likes
            var rr1 = await mediator.Send(new EvltLikesQuery { EvltIds = items.Select(_ => _.Id).Distinct().ToArray() });
            foreach (var item in items)
            {
                if (!rr1.Items.TryGetValue(item.Id, out var r)) continue;
                item.LikeCount = r.Likecount;
                item.IsLikeByMe = r.IsLikeByMe;
            }

            return pg;
        }

        IEnumerable<int> GetMainSubj()
        {
            foreach (var c1 in config.GetSection("AppSettings:pc_elvtMainPage_subjSide").GetChildren())
            {
                var temp = c1["item2"];
                if (!temp.IsNullOrWhiteSpace())
                {
                    var i = int.TryParse(temp, out var _i) ? _i : 0;
                    if (i.In(0, (int)SubjectEnum.Other)) continue;
                    yield return i;
                }
            }
        }

        IEnumerable<(string, string)> GetSubjs()
        {
            foreach (var c1 in config.GetSection("AppSettings:pc_elvtMainPage_subjSide").GetChildren())
            {
                yield return (c1["item1"], c1["item2"]);
            }
        }

        string Fmt_Age_SqlWhere(IEnumerable age_arr, string c_minage, string c_maxage)
        {
            var sb = new StringBuilder();
            foreach (var age in age_arr)
            {
                if (!Enum.IsDefined(typeof(AgeGroup), age)) continue;
                var sa = EnumUtil.GetDesc(age.ToString().ToEnum<AgeGroup>()).AsSpan();
                var amin = new string(sa[..sa.IndexOf('-')].Trim());
                var amax = new string(sa[(sa.IndexOf('-') + 1)..].Trim());
                sb.AppendLine($"and not({c_maxage}<{amin} or {c_minage}>{amax})");
            }
            return sb.ToString();
        }
    }
}
