using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class AutoLikeEvaluationCommandHandler : IRequestHandler<AutoLikeEvaluationCommand>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;
        IConfiguration config;

        const string diff_day = "1,3,4";

        public AutoLikeEvaluationCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
            this.config = config;
        }

        public async Task<Unit> Handle(AutoLikeEvaluationCommand cmd, CancellationToken cancellation)
        {
            cmd.Nbf ??= new DateTime(2020, 11, 1, 0, 0, 0, DateTimeKind.Local);
            var now = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:00:00"));
            await default(ValueTask);

            // 是否只对精华评测刷赞
            var onlyLikeStick = cmd.StickHours?.Any() == true && now.Hour.In(cmd.StickHours);
            //
            var rules = config.GetSection("AppSettings:AutoLikeEvaluation:rules").Get<KV[]>();

            await foreach (var evlts in GetEvlts(cmd, 10, now, onlyLikeStick))
            {
                foreach (var evl in evlts)
                {
                    var itm = RandomItem(rules);
                    evl.Rndrule = itm.Key;
                    var (from, to) = RegxRuleLikes(itm.Value);
                    evl.Rndaddlikes = RandomItem(FromRange(from, to));                    
                }
                await UpEvltLikes(evlts);
            }

            return default;
        }

        async IAsyncEnumerable<AutoLikeEvaluationLog[]> GetEvlts(AutoLikeEvaluationCommand cmd, int top, DateTime now, bool onlyLikeStick)
        {
            var sql = $@"
delete from AutoLikeEvaluationLog where logtime=@Now
;;
insert AutoLikeEvaluationLog(evaluationid,title,stick,status,likes,shamlikes,EvalCreateTime,rndrule,rndAddLikes,id,logtime)
select e.id as evaluationid,e.title,e.stick,e.status,e.likes,e.shamlikes,e.CreateTime,
    convert(int,null)as rndrule,convert(int,null)as rndAddLikes,newid()as id,@Now as logtime
from Evaluation e
where e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()} and e.CreateTime>=@Nbf --and e.IsOfficial=0
{"and e.stick=1".If(onlyLikeStick)}
and datediff(dd,e.CreateTime,@Now) in ({diff_day}) 
--order by e.CreateTime desc
";
            await unitOfWork.DbConnection.ExecuteAsync(sql, new { cmd.Nbf, Now = now });

            while (true)
            {
                sql = $"select top {top} * from AutoLikeEvaluationLog where logtime=@Now and rndrule is null ";
                var arr = (await unitOfWork.DbConnection.QueryAsync<AutoLikeEvaluationLog>(sql, new { Now = now })).AsArray();
                if (arr?.Any() != true) break;
                yield return arr;
                // 
                // 后续会更新 rndrule, rndaddlikes
                //
            }
        }

        async Task UpEvltLikes(AutoLikeEvaluationLog[] evl)
        {
            var sql = @"
update AutoLikeEvaluationLog set rndrule=@Rndrule,rndAddLikes=@Rndaddlikes where id=@Id ;
update Evaluation set shamlikes=isnull(shamlikes,0)+@Rndaddlikes,ModifyDateTime=@Logtime where id=@Evaluationid ;
";
			await unitOfWork.DbConnection.ExecuteAsync(sql, evl);

            // clear redis cache
            //
            var ids = evl.Select(_ => _.Evaluationid).Distinct();
            if (!ids.Any()) return;
            await mediator.Send(new ClearLikesCachesCommand { Ids = ids, Type = 1, TimeoutSeconds = 30 });
        }

        static T RandomItem<T>(IEnumerable<T> arr)
        {
            var n = arr.Count();
            if (n == 0) return default;
            var hash = Math.Abs(Guid.NewGuid().GetHashCode());
            return arr.ElementAtOrDefault(hash % n);
        }

        static IEnumerable<int> FromRange(int start, int end) => Enumerable.Range(start, end + 1 - start);

        static (int from, int to) RegxRuleLikes(string str)
        {
            var gx = Regex.Match(str, @"^(?<from>\d+)\s*(\-\s*(?<to>\d+)\s*){0,1}$", RegexOptions.IgnoreCase).Groups;
            var from = int.Parse(gx["from"].Value);
            var to = gx["to"].Value is string _to && !string.IsNullOrEmpty(_to) ? int.Parse(_to) : from;
            return (from, to);
        }

        class KV
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
