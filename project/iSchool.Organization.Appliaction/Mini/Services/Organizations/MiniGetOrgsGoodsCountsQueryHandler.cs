using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class MiniGetOrgsGoodsCountsQueryHandler : IRequestHandler<MiniGetOrgsGoodsCountsQuery, MiniGetOrgsGoodsCountsQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient redis;
        IConfiguration _config;

        public MiniGetOrgsGoodsCountsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IConfiguration config,
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.redis = redis;
            this._config = config;
        }

        public async Task<MiniGetOrgsGoodsCountsQryResult> Handle(MiniGetOrgsGoodsCountsQuery query, CancellationToken cancellation)
        {
            var result = new MiniGetOrgsGoodsCountsQryResult();

            result.Dict = (await _mediator.Send(new PcGetOrgsCountsQuery { OrgIds = query.OrgIds }))
                .ToDictionary(_ => _.Key, _ => _.Value.GoodsCount);

            var notIn = query.OrgIds.Where(orgid => !result.Dict.ContainsKey(orgid)).ToArray();
            foreach (var k in notIn)
            {
                result.Dict.TryAdd(k, 0);
            }

            return result;
        }

    }
}
