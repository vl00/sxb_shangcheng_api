using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public partial class UpbackCourseStockCommandHandler : IRequestHandler<UpbackCourseStockCommand, bool>
    {
        IMediator _mediator;
        CSRedisClient redis;
        OrgUnitOfWork _orgUnitOfWork;

        public UpbackCourseStockCommandHandler(IMediator mediator,
            IOrgUnitOfWork orgUnitOfWork,
            CSRedisClient redis)
        {
            this._mediator = mediator;
            this.redis = redis;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<bool> Handle(UpbackCourseStockCommand cmd, CancellationToken cancellation)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            if (cmd.DoSec != null) cts.CancelAfter(cmd.DoSec.Value * 1000);
            try
            {
                var b = await Upback_Courses(cts.Token);
                return b;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> Upback_Courses(CancellationToken cancellation)
        {
            await foreach (var key in redis.ScanKeys(CacheKeys.CourseGoodsStock.FormatWith("*"), cancellation))
            {
                var goodsId = key.Length > 36 && Guid.TryParse(key[^36..], out var _gid) ? _gid : default;
                if (goodsId == default) continue;

                await _mediator.Send(new CourseGoodsStockRequest
                {
                    SyncSetStock = new SyncSetGoodsStockCommand
                    {
                        Id = goodsId,
                    }
                });
            }
            return true;
        }
    }
}
