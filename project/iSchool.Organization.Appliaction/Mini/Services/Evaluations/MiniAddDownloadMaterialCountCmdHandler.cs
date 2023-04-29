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
    public class MiniAddDownloadMaterialCountCmdHandler : IRequestHandler<MiniAddDownloadMaterialCountCmd, bool>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly IUserInfo me;
        private readonly CSRedisClient redis;
        private readonly ILock1Factory _lck1fay;

        public MiniAddDownloadMaterialCountCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, ILock1Factory lck1fay,
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;
            this._lck1fay = lck1fay;
        }

        public async Task<bool> Handle(MiniAddDownloadMaterialCountCmd cmd, CancellationToken cancellation)
        {
            var evlt = await _mediator.Send(new GetEvltBaseInfoQuery { EvltId = cmd.EvltId });
            if (evlt == null) return false;

            await using var lck0 = await _lck1fay.LockAsync($"org:lck2:mini_evlt_AddDownloadMaterialCount:evltid_{cmd.EvltId}");
            if (!lck0.IsAvailable) throw new CustomResponseException("系统繁忙", 555);

            // up db
            {
                var sql = "update Evaluation set DownloadMaterialCount=isnull(DownloadMaterialCount,0)+1 where id=@EvltId";
                await _orgUnitOfWork.ExecuteAsync(sql, new { cmd.EvltId });
            }

            return true;
        }

    }
}
