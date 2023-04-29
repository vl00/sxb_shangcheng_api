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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public partial class AutoOnlineOrOffCommandHandler : IRequestHandler<AutoOnlineOrOffCommand>
    {
        public async Task<Unit> Handle2(AutoOnlineOrOffCommand cmd, CancellationToken cancellation)
        {
            var now = DateTime.Now;
            cmd.ContentType ??= AutoOnlineOrOffContentType.Course;
            var cacheKeys = new HashSet<string>(); // for clear caches      
            await default(ValueTask);            

            while (!cancellation.IsCancellationRequested)
            {
                var sql = $@"
select top 20 a.* from AutoOnlineOrOff a 
where a.IsValid=1 and a.execstatus in({AutoOnlineOrOffExecStatus.Todo.ToInt()})
and a.contentType=@ContentType and a.plantime<=@now
order by a.plantime asc
";
                var ls = await unitOfWork.DbConnection.QueryAsync<AutoOnlineOrOff>(sql, new { now, cmd.ContentType });
                if (!ls.Any()) // 已完成
                    break;
                if (cancellation.IsCancellationRequested)
                    break;

                var dict = ls.GroupBy(_ => _.Contenttype).ToDictionary(x => (AutoOnlineOrOffContentType)x.Key, x => x.OrderBy(_ => _.Plantime).ToArray());
                foreach (var kv in dict)
                {
                    var (ctt, ls1) = kv;
                    var med = this.GetType().GetMethod($"Handle_{nameof(AutoOnlineOrOffContentType)}_{cmd.ContentType.GetName()}", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var task = med.Invoke(this, new object[] { now, ls1, cancellation }) as Task;
                    if (task != null) await task;
                }
            }

            // try clear 'IsValid=0'
            {
                var sql = "delete from AutoOnlineOrOff where IsValid=0 and datediff(dd,CreateTime,getdate())>20";
                await unitOfWork.DbConnection.ExecuteAsync(sql);
            }

            // clear caches       
            if (cacheKeys.Count > 0)
            {
                try
                {
                    await redis.BatchDelAsync(cacheKeys, 30);
                }
                catch { }
            }

            return default;
        }

        // 课程
        async Task Handle_AutoOnlineOrOffContentType_Course(DateTime now, AutoOnlineOrOff[] ls, CancellationToken cancellation)
        {
            var sql = $@"

";
        }
    }
}
