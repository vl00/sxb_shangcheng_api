using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
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

namespace iSchool.Organization.Appliaction.Services
{
    public class GetFreightCityAreasTypeQyArgsHandler : IRequestHandler<GetFreightCityAreasTypeQyArgs, GetFreightCityAreasTypeQyResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public GetFreightCityAreasTypeQyArgsHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<GetFreightCityAreasTypeQyResult> Handle(GetFreightCityAreasTypeQyArgs query, CancellationToken cancellation)
        {
            var result = new GetFreightCityAreasTypeQyResult();
            await default(ValueTask);

            var sql = $@"
select top 1 id,name,FreightAreaType from cityarea where FreightAreaType>0 and IsValid=1 {"and id=@Pr".If(query.Pr != null)} {"and left(name,2)=@Name".If(!query.Province.IsNullOrEmpty())}
";
            var (code, name, fatype) = await _orgUnitOfWork.QueryFirstOrDefaultAsync<(int, string, int?)>(sql, new { query.Pr, Name = query.Province?.Length > 2 ? query.Province[..2] : null });
            result.Code = code;
            result.Name = name != null ? name : (code == 0 ? "全国" : null);
            result.Ty = fatype == null ? FreightAreaTypeEnum.Other 
                : fatype <= 0 ? FreightAreaTypeEnum.Other
                : fatype >= FreightAreaTypeEnum.Custom.ToInt() ? FreightAreaTypeEnum.Other
                : (FreightAreaTypeEnum)fatype;

            return result;
        }

    }
}
