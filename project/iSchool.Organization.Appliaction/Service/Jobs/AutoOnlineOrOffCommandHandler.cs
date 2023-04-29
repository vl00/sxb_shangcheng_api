using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
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
    public partial class AutoOnlineOrOffCommandHandler : IRequestHandler<AutoOnlineOrOffCommand>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;
        IConfiguration config;        

        public AutoOnlineOrOffCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
            this.config = config;
        }

        public async Task<Unit> Handle(AutoOnlineOrOffCommand cmd, CancellationToken cancellation)
        {
            var now = DateTime.Now;
            cmd.ContentType ??= AutoOnlineOrOffContentType.Course;                  
            var cacheKeys = new HashSet<string>(); // for clear caches      
            await default(ValueTask);

            while (!cancellation.IsCancellationRequested)
            {
                var sql = $@"
select top 10 a.id,a.contentType,a.planstatus,a.plantime,a.executedtime,a.execstatus,a.ExtData,a.CreateTime,a.contentid 
--c.Modifier as Item3,c.ModifyDateTime as Item4
{(cmd.ContentType switch {    
    AutoOnlineOrOffContentType.Evaluation => ",c.status as Item1,c.title as Item2",
    AutoOnlineOrOffContentType.Organization => ",c.status as Item1,c.name as Item2",
    AutoOnlineOrOffContentType.Course => ",c.status as Item1,c.title as Item2",
    _ => ""
})}
from AutoOnlineOrOff a 
{(cmd.ContentType == AutoOnlineOrOffContentType.Evaluation 
    ? $"left join Evaluation c on a.contentid=c.id and a.contentType={AutoOnlineOrOffContentType.Evaluation.ToInt()}"
: cmd.ContentType == AutoOnlineOrOffContentType.Organization 
    ? $"left join Organization c on a.contentid=c.id and a.contentType={AutoOnlineOrOffContentType.Organization.ToInt()}"
: cmd.ContentType == AutoOnlineOrOffContentType.Course 
    ? $"left join Course c on a.contentid=c.id and a.contentType={AutoOnlineOrOffContentType.Course.ToInt()}"
: "")}
where c.IsValid=1 and a.IsValid=1 and a.execstatus in({AutoOnlineOrOffExecStatus.Todo.ToInt()})
{(cmd.ContentType switch { 
    AutoOnlineOrOffContentType.Course => $@"and (case when a.planstatus={CourseStatusEnum.Ok.ToInt()}
        then DATEADD(MS,0,DATEADD(DD,DATEDIFF(DD,0,a.plantime),0))
        else DATEADD(MS,-3,DATEADD(DD,DATEDIFF(DD,-1,a.plantime),0)) end)<=@now",
    _ => "and a.plantime<=@now"
})}	
order by a.plantime asc
";
                var ls = await unitOfWork.DbConnection.QueryAsync<AutoOnlineOrOff, (byte, string), (AutoOnlineOrOff, byte, string)>(sql,
                    param: new { now }, splitOn: "Item1",
                    map: (auto0, t) => (auto0, t.Item1, t.Item2));

                if (!ls.Any()) // 已完成
                    break;
                if (cancellation.IsCancellationRequested) 
                    break;

                // assign                
                var sls = new List<(string tb, Guid id, byte status, string extup)>();
                foreach (var (autof, status, _title) in ls)
                {
                    string tb = default;
                    string err = null;
                    string extup = ""; // `x=y,`

                    // 课程
                    if (autof.Contenttype == (byte)AutoOnlineOrOffContentType.Course)
                    {
                        tb = "dbo.Course";

                        // ???
                        //extup = $"{(status == (byte)CourseStatusEnum.Ok ? $"LastOnShelfTime=@now," : "")}" +
                        //    $"{(status == (byte)CourseStatusEnum.Fail ? $"LastOffShelfTime=@now," : "")}";

                        cacheKeys.Add(CacheKeys.CourseDetails.FormatWith(autof.Contentid));
                        cacheKeys.Add(CacheKeys.CourseDetails.FormatWith(autof.Contentid) + ":*");
                        cacheKeys.Add("org:courses:*");
                        cacheKeys.Add("org:course:*");
                        cacheKeys.Add(CacheKeys.OrgDetails.FormatWith("*") + ":*:counts:course");
                    }
                    // 评测
                    else if (autof.Contenttype == (byte)AutoOnlineOrOffContentType.Evaluation)
                    {
                        tb = "dbo.Evaluation";

                        cacheKeys.Add(CacheKeys.Evlt.FormatWith(autof.Contentid));
                        cacheKeys.Add(CacheKeys.Del_evltMain);
                        cacheKeys.Add(CacheKeys.Rdk_spcl.FormatWith("*"));
                        
                    }
                    // 机构
                    else if (autof.Contenttype == (byte)AutoOnlineOrOffContentType.Organization)
                    {
                        tb = "dbo.Organization";

                        //cacheKeys.Add(CacheKeys.)
                    }

                    if (autof.Planstatus == status) // 计划状态与当前状态一致,直接失败
                    {
                        autof.Execstatus = (byte)AutoOnlineOrOffExecStatus.Failed;
                        err = "计划状态与当前状态一致";
                    }
                    else
                    {
                        autof.Execstatus = (byte)AutoOnlineOrOffExecStatus.Sucessed;
                    }
                    autof.Executedtime = now;
                    autof.ExtData = (new
                    {
                        status0 = status,
                        err,
                    }).ToJsonString(camelCase: true, ignoreNull: true);

                    if (err == null) sls.Add((tb, autof.Contentid, autof.Planstatus, extup));
                }

                if (cancellation.IsCancellationRequested)
                    break;

                // update (all ctt-type)              
                try
                {
                    unitOfWork.BeginTransaction();
                    if (sls.Any())
                    {
                        var sb = new StringBuilder();
                        foreach (var t in sls)
                        {
                            sb.AppendLine($"update {t.tb} set {t.extup}[status]={t.status},ModifyDateTime=@now,Modifier='23333333-3333-3333-3333-333333333332' where id='{t.id}' ;");
                        }
                        unitOfWork.DbConnection.Execute(sb.ToString(), new { now }, unitOfWork.DbTransaction);
                    }
                    sql = @"update AutoOnlineOrOff set executedtime=@Executedtime,execstatus=@Execstatus,ExtData=@ExtData where id=@Id";
                    unitOfWork.DbConnection.Execute(sql, ls.Select(_ => _.Item1), unitOfWork.DbTransaction);

                    unitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    unitOfWork.SafeRollback();
                    //...
                }
            }

            // try clear 'IsValid=0'
            {
                var sql = "delete from AutoOnlineOrOff where IsValid=0 and datediff(dd,CreateTime,getdate())>30";
                await unitOfWork.DbConnection.ExecuteAsync(sql);
            }

            // clear caches       
            if (cacheKeys.Count > 0)
            {
                try
                {
                    await redis.BatchDelAsync(cacheKeys, 60);
                }
                catch { }
            }

            return default;
        }
        

    }
}
