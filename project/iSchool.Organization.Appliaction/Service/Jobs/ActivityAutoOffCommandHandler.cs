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
    public class ActivityAutoOffCommandHandler : IRequestHandler<ActivityAutoOffCommand>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;
        IConfiguration config;        

        public ActivityAutoOffCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
            this.config = config;
        }

        public async Task<Unit> Handle(ActivityAutoOffCommand cmd, CancellationToken cancellation)
        {         
            await default(ValueTask);

            var sql = $@"
update Activity set status={ActivityStatus.Fail.ToInt()} 
where  IsValid=1 and endtime<'{DateTime.Now}' 
and status={ActivityStatus.Ok.ToInt()} 
and type={ActivityType.Hd2.ToInt()}
";
            unitOfWork.DbConnection.Execute(sql);

            await redis.BatchDelAsync(new List<string> {
               CacheKeys.Acd_id.FormatWith("*")
              ,CacheKeys.ActivitySimpleInfo.FormatWith("*")
              ,CacheKeys.Hd_spcl_acti.FormatWith("*")
              ,"org:special:simple*"//专题列表
            }, 30);

            return default;
        }
        

    }
}
