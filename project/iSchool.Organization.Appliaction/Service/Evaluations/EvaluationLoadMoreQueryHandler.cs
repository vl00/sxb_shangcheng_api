using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class EvaluationLoadMoreQueryHandler : IRequestHandler<EvaluationLoadMoreQuery, EvaluationLoadMoreQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        ElvtMainPageSizeOption pageSizeOption;
        IMapper mapper;
        IConfiguration config;
        IKeyValueReposiory _kevValueRepo;
        const int cache_min = 60 * 15;

        public EvaluationLoadMoreQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IUserInfo me,
            IConfiguration config, IOptionsSnapshot<ElvtMainPageSizeOption> pageSizeOption,
            IMapper mapper, IKeyValueReposiory kevValueRepo)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.pageSizeOption = pageSizeOption.Value;
            this.mapper = mapper;
            this.config = config;
            this._kevValueRepo = kevValueRepo;

        }

        public async Task<EvaluationLoadMoreQueryResult> Handle(EvaluationLoadMoreQuery req, CancellationToken cancellation)
        {
            var is_recommend = req.Stick == 1 ? true : false; //推荐

            var rdk = CacheKeys.RdK_evlts.FormatWith(req.Subj, req.Age,req.Stick);
            var pg = new EvaluationLoadMoreQueryResult();
            pg.CurrPageIndex = req.PageIndex;
            EvaluationItemDto[] items = null;
            var (cc, c1, c0) = await GetTotal(req);
            pg.TotalPageCount = Math.Max(GetTotalPageCount(c1, pageSizeOption.Size1), GetTotalPageCount(c0, pageSizeOption.Size0));

            // 快速比较超出总页数
            if (pg.CurrPageIndex > pg.TotalPageCount)
            {
                items = new EvaluationItemDto[0];
                goto LB_END;
            }

            // first page try find from redis cache first
            if (pg.CurrPageIndex == 1)
            {
                items = await redis.GetAsync<EvaluationItemDto[]>(rdk);
                if (items != null) goto LB_END;
            }

            // pagesize 有图:无图 == 20:5            
            try
            {
                unitOfWork.ReadDbConnection.TryOpen();
                var t1 = GetItems(req, false, pageSizeOption.Size1);
                var t0 = GetItems(req, true, pageSizeOption.Size0);
                await Task.WhenAll(t1, t0);
                // 合并也要排序
                items = t1.Result.Union(t0.Result).OrderByDescending(x => x, new FuncComparer<EvaluationItemDto>((x1, x2) =>
                {
                    if (is_recommend)
                    {
                        if (x1.Stick && !x2.Stick) return 1;
                        if (!x1.Stick && x2.Stick) return -1;
                    }
                    if (x1.ViewCount != x2.ViewCount) return x1.ViewCount > x2.ViewCount ? 1 : -1;
                    if (x1.CreateTime != x2.CreateTime) return x1.CreateTime > x2.CreateTime ? 1 : -1;
                    return 0;
                }))
                .ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                unitOfWork.ReadDbConnection.Close();
            }

            // 查用户信息
            {
                var uInfos = await mediator.Send(new UserSimpleInfoQuery
                {
                    UserIds = items.Select(_ => _.AuthorId)
                });
                foreach (var u in uInfos)
                {
                    foreach (var u0 in items.Where(_ => _.AuthorId == u.Id))
                    {
                        u0.AuthorName = u.Nickname;
                        u0.AuthorHeadImg = u.HeadImgUrl;
                    }
                }
            }

            // fisrt page set to redis cache 
            if (pg.CurrPageIndex == 1)
            {
                await redis.SetAsync(rdk, items, cache_min);
            }

            LB_END:
            pg.CurrItems = items;

            // find likecount + IsLikeByMe
            if (items?.Length > 0)
            {
                var likes = await mediator.Send(new EvltLikesQuery { EvltIds = items.Select(_ => _.Id).ToArray() });
                foreach (var item in items)
                {
                    if (!likes.Items.TryGetValue(item.Id, out var lk)) continue;
                    item.LikeCount = lk.Likecount;
                    item.IsLikeByMe = lk.IsLikeByMe;
                }
            }

            if (pg.CurrPageIndex == 1)
            {
                pg.Subjs = GetSubjs().ToArray();
            }
            return pg;
        }

        void Resolve_Search_Args(EvaluationLoadMoreQuery req, out int[] subj_arr, out int[] age_arr, out bool no_limit_subj, out bool no_limit_age,
            out bool is_recommend, out bool idx_other)
        {
            #region old codes
            //var no_limit_subj = false;
            //var no_limit_age = false;
            //var is_recommend = req.Stick == 1 ? true : false;//推荐
            //if (!is_recommend)
            //{
            //    var subj_arr = req.Subj?.Split(',') ?? new string[0];
            //    var age_arr = req.Age?.Split(',') ?? new string[0];
            //    no_limit_subj = subj_arr.Length == 1 && 0 == Convert.ToInt32(subj_arr[0]) ? true : false;
            //    no_limit_age = age_arr.Length == 1 && 0 == Convert.ToInt32(age_arr[0]) ? true : false;
            //}
            #endregion

            subj_arr = null;
            age_arr = null;
            idx_other = false;
            no_limit_subj = false;
            no_limit_age = false;
            is_recommend = req.Stick == 1 ? true : false; //推荐
            if (!is_recommend)
            {
                subj_arr = req.Subj?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.TryParse(x, out var _x) ? _x : 0).Distinct()
                    .Where(x => x > -2 && !x.In(0))
                    .ToArray();

                age_arr = req.Age?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.TryParse(x, out var _x) ? _x : 0).Distinct()
                    .Where(x => x > -2 && !x.In(0))
                    .ToArray();

                no_limit_subj = (subj_arr?.Length ?? 0) < 1;
                no_limit_age = (age_arr?.Length ?? 0) < 1;

                //首页其他传值-1 or 199
                idx_other = subj_arr?.Length == 1 && subj_arr[0].In(-1, SubjectEnum.Other.ToInt()) ? true : false;
            }
        }

        // 获取总item数 (总数, 有图总数, 无图总数)
        async Task<(long cc, long c1, long c0)> GetTotal(EvaluationLoadMoreQuery req)
        {
            Resolve_Search_Args(req, out int[] subj_arr, out int[] age_arr, out bool no_limit_subj, out bool no_limit_age,
                out bool is_recommend, out bool idx_other);

            var k = CacheKeys.RdK_evltsTotal.FormatWith(req.Subj, req.Age, req.Stick);
            var total = await redis.HGetAllAsync(k);
            if (total?.Any() == true) return (Convert.ToInt64(total["cc"]), Convert.ToInt64(total["c1"]), Convert.ToInt64(total["c0"]));
           
            var sql = $@"
select count(1) as cc,
isnull(sum(case when evlt.isPlaintext=1 then 0 else 1 end),0) as c1,  
isnull(sum(case when evlt.isPlaintext=1 then 1 else 0 end),0) as c0
from Evaluation evlt
where evlt.IsValid=1 and evlt.status={Consts.EvltOkStatus} "
            ;

            if (is_recommend) //推荐 查询精华评测@2020.08.25向pm确认过
            {
                sql += "and evlt.stick=1 ";
            }
            else
            {
                if (idx_other)//首页其他
                {
                    sql += $@"
or (exists(select 1 from EvaluationBind b where b.IsValid=1 
	and b.evaluationid=evlt.id and (b.subject is null or b.subject not in({string.Join(',', GetMainSubj())}))  and b.courseid is null)
or exists(select 1 from EvaluationBind b join Course c on c.id=b.courseid and c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()}
	where b.IsValid=1 and b.evaluationid=evlt.id and b.courseid is not null and (c.subject is null or c.subject not in({string.Join(',', GetMainSubj())})))
) ";
                }
                else if (!no_limit_subj)
                {
                    sql += $@"
and (exists(select 1 from EvaluationBind b where b.IsValid=1 
	and b.evaluationid=evlt.id and b.subject in ({string.Join(',', subj_arr)}) and b.courseid is null)
or exists(select 1 from EvaluationBind b join Course c on c.id=b.courseid and c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()}
	where b.IsValid=1 and b.evaluationid=evlt.id and b.courseid is not null and c.subject in ({string.Join(',', subj_arr)}))) ";

                }
                if (!no_limit_age)
                {

                    var ageFilter = "";
                    var idx = 0;
                    //年龄段
                    foreach (var item in age_arr)
                    {

                        var age = Convert.ToInt32(item);
                        if (Enum.IsDefined(typeof(AgeGroup), age))
                        {

                            var ages_str = EnumUtil.GetDesc((AgeGroup)age).Split('-');
                            var request_min = Convert.ToInt32(ages_str[0]);
                            var request_max = Convert.ToInt32(ages_str[1]);
                            ageFilter += @$"  {" or ".If(0 != idx)}  (c.minage>={request_min} and c.maxage<={request_max})or (c.minage<={request_min} and c.maxage>={request_min})or (c.minage<={request_max} and c.maxage>={request_max})";

                        }
                        idx++;
                    }

                    sql += $@"and (exists(select 1 from EvaluationBind b where b.IsValid=1 
    and b.evaluationid = evlt.id and b.age in ({ string.Join(',', age_arr)}) and b.courseid is null)
or exists(select 1 from EvaluationBind b join Course c on c.id = b.courseid and c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()}
    where b.IsValid = 1 and b.evaluationid = evlt.id and b.courseid is not null and ( {ageFilter}) and c.maxage>0)) ";

                }

            }

            var dy = await unitOfWork.QueryFirstAsync(sql);
            var r = ((long)Convert.ToInt64(dy.cc), (long)Convert.ToInt64(dy.c1), (long)Convert.ToInt64(dy.c0));

            await redis.StartPipe()
                .HSet(k, "cc", r.Item1)
                .HSet(k, "c1", r.Item2)
                .HSet(k, "c0", r.Item3)
                .Expire(k, cache_min)
                .EndPipeAsync();

            return r;
        }

        // 获取当前有/无图item
        async Task<IEnumerable<EvaluationItemDto>> GetItems(EvaluationLoadMoreQuery req, bool isPlaintext, int pagesize)
        {
            Resolve_Search_Args(req, out int[] subj_arr, out int[] age_arr, out bool no_limit_subj, out bool no_limit_age,
                out bool is_recommend, out bool idx_other);

            var sql = $@"
select evlt.*
from Evaluation evlt
where evlt.IsValid=1 and evlt.status=@status and evlt.isPlaintext=@isPlaintext {{0}} 
order by {"evlt.stick desc,".If(is_recommend)}evlt.viewcount desc,evlt.CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";


            var where = "";
            if (is_recommend) //推荐 查询精华评测
            {
                where += "and evlt.stick=1";

            }
            else //筛选的科目
            {

                if (idx_other) //其他是排除主页显示的3个科目
                {
                    where += $@"
and (exists(select 1 from EvaluationBind b where b.IsValid=1 
	and b.evaluationid=evlt.id and (b.subject is null or b.subject not in({string.Join(',', GetMainSubj())})) and b.courseid is null)
or exists(select 1 from EvaluationBind b join Course c on c.id=b.courseid and c.IsValid=1
	where b.IsValid=1 and b.evaluationid=evlt.id and b.courseid is not null and (c.subject is null or c.subject not in({string.Join(',', GetMainSubj())})))
) ";
                }
                else if (!no_limit_subj)
                {
                    where += $@"
and (exists(select 1 from EvaluationBind b where b.IsValid=1 
	and b.evaluationid=evlt.id and b.subject in ({string.Join(',', subj_arr)}) and b.courseid is null)
or exists(select 1 from EvaluationBind b join Course c on c.id=b.courseid and c.IsValid=1
	where b.IsValid=1 and b.evaluationid=evlt.id and b.courseid is not null and c.subject in ({string.Join(',', subj_arr)})))";
                }

                if (!no_limit_age)//筛选的年龄段
                {
                    var ageFilter = "";
                    var idx = 0;
                    //年龄段
                    foreach (var item in age_arr)
                    {

                        var age = Convert.ToInt32(item);
                        if (Enum.IsDefined(typeof(AgeGroup), age))
                        {

                            var ages_str = EnumUtil.GetDesc((AgeGroup)age).Split('-');
                            var request_min = Convert.ToInt32(ages_str[0]);
                            var request_max = Convert.ToInt32(ages_str[1]);
                            ageFilter += @$" {" or ".If(0!=idx)} (c.minage>={request_min} and c.maxage<={request_max})or (c.minage<={request_min} and c.maxage>={request_min})or (c.minage<={request_max} and c.maxage>={request_max})";

                        }
                        idx++;

                    }
                    where += $@"and (exists(select 1 from EvaluationBind b where b.IsValid=1 

    and b.evaluationid = evlt.id and b.age in ({ string.Join(',', age_arr)}) and b.courseid is null)
or exists(select 1 from EvaluationBind b join Course c on c.id = b.courseid and c.IsValid = 1

    where b.IsValid = 1 and b.evaluationid = evlt.id and b.courseid is not null and  ({ageFilter}) and c.maxage>0))";
                }
            }



            sql = sql.FormatWith(where);

            var dyp = new DynamicParameters(req)
                .Set("PageSize", pagesize)
                .Set("isPlaintext", isPlaintext)
                .Set("status", Consts.EvltOkStatus);
            var qs = await unitOfWork.QueryAsync<Evaluation>(sql, dyp);

            var items = qs.Select(evlt => mapper.Map<EvaluationItemDto>(evlt));
            return items;
        }

        IEnumerable<int> GetMainSubj()
        {
            foreach (var c1 in config.GetSection("AppSettings:elvtMainPage_subjSide").GetChildren())
            {
                var temp = c1["item2"];
                if (!temp.IsNullOrWhiteSpace())
                {
                    var i = Convert.ToInt32(temp);
                    if (i.In(0, (int)SubjectEnum.Other)) continue;
                    yield return i;
                }
            }
        }

        IEnumerable<(string, string)> GetSubjs()
        {
            foreach (var c1 in config.GetSection("AppSettings:elvtMainPage_subjSide").GetChildren())
            {
                yield return (c1["item1"], c1["item2"]);
            }
        }

        static int GetTotalPageCount(long totalItemCount, int pageSize)
        {
            return (int)Math.Ceiling(totalItemCount / (double)pageSize);
        }
    }
}
