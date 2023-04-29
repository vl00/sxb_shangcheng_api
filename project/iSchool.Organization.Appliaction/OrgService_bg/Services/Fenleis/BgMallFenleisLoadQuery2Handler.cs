using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services
{
    public class BgMallFenleisLoadQuery2Handler : IRequestHandler<BgMallFenleisLoadQuery2, BgMallFenleisLoadQueryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        const int MaxDepth = 3;

        public BgMallFenleisLoadQuery2Handler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<BgMallFenleisLoadQueryResult> Handle(BgMallFenleisLoadQuery2 query, CancellationToken cancellation)
        {
            var result = await _mediator.Send(new BgMallFenleisLoadQuery { Code = query.Code, UserId = query.UserId, ExpandMode = 2 });

            // 有3级的第2级
            var alls2 = await _orgUnitOfWork.QueryAsync<int>($@"
                select distinct parent from KeyValue where IsValid=1 and type={Consts.Kvty_MallFenlei} and depth=3
            ");

            // 有3级的第1级
            var alls1 = await _orgUnitOfWork.QueryAsync<int>($@"
                select distinct parent from KeyValue where IsValid=1 and type={Consts.Kvty_MallFenlei} and depth=2 
                    and [key] in (select distinct parent from KeyValue where IsValid=1 and type={Consts.Kvty_MallFenlei} and depth=3)
            ");

            if (result.D2s?.AsList() is List<BgMallFenleiItemDto> d2s)
            {
                d2s.RemoveAll(_ => !alls2.Contains(_.Code));
                result.D2s = d2s;
            }
            if (result.D1s?.AsList() is List<BgMallFenleiItemDto> d1s)
            {
                d1s.RemoveAll(_ => !alls1.Contains(_.Code));
                result.D1s = d1s;
            }

            if (result.D1s?.Any(_ => _.Code == result.Selected_d1.Code) != true)
            {
                result.Selected_d1 = result.D1s?.FirstOrDefault();
            }
            if (result.D2s?.Any(_ => _.Code == result.Selected_d2.Code) != true)
            {
                result.Selected_d2 = result.D2s?.FirstOrDefault();
            }
            if (result.D3s?.Any(_ => _.Code == result.Selected_d3.Code) != true)
            {
                result.Selected_d3 = result.D3s?.FirstOrDefault();
            }
            if ((query.Code ?? 0) != 0)
            {
                if (!query.Code.In(result.Selected_d1?.Code, result.Selected_d2?.Code, result.Selected_d3?.Code))
                    throw new CustomResponseException("数据已更新请重新刷新页面", 4000);
            }

            return result;
        }

        
    }
}
