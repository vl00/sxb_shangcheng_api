using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class MiniAddDownloadCourseMaterialCountCmdHandler : IRequestHandler<MiniAddDownloadCourseMaterialCountCmd, bool>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly IUserInfo me;
        private readonly CSRedisClient redis;
        private readonly ILock1Factory _lck1fay;

        public MiniAddDownloadCourseMaterialCountCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, ILock1Factory lck1fay,
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;
            this._lck1fay = lck1fay;
        }

        public async Task<bool> Handle(MiniAddDownloadCourseMaterialCountCmd cmd, CancellationToken cancellation)
        {
           

            await using var lck0 = await _lck1fay.LockAsync($"org:lck2:mini_evlt_AddDownloadCourseMaterialCount:materialid_{cmd.Id}");
            if (!lck0.IsAvailable) throw new CustomResponseException("系统繁忙", 555);

            // up db
            {
                var sql = "update MaterialLibrary set DownloadTime=isnull(DownloadTime,0)+1 where id=@Id";
                await _orgUnitOfWork.ExecuteAsync(sql, new { cmd.Id });
            }

            return true;
        }

    }
}
