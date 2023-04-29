using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Modles;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetLastKdnusDescQueryArgsHandler : IRequestHandler<GetLastKdnusDescQueryArgs, IEnumerable<(string, string, string, DateTime)>>
    {
        IConfiguration _config;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;

        public GetLastKdnusDescQueryArgsHandler(IConfiguration config,
            IOrgUnitOfWork orgUnitOfWork,
            IMediator mediator)
        {
            this._config = config;
            this._mediator = mediator;
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<IEnumerable<(string, string, string, DateTime)>> Handle(GetLastKdnusDescQueryArgs query, CancellationToken cancellation)
        {
            if (query.Nus?.Length < 1) return Enumerable.Empty<(string, string, string, DateTime)>();

            if (query.ReqApi)
            {
                var ls = new List<(string Nu, string Comcode, string Desc, DateTime Time)>();
                foreach (var nu in query.Nus)
                {
                    var dto = await _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery { Nu = nu.Nu, Com = nu.Comcode, ReadUseDb = true, WriteUseDb = true });
                    if (dto.Errcode != 0) continue;
                    var lastT = dto.Items?.FirstOrDefault();
                    if (lastT == null) continue;
                    ls.Add((nu.Nu, dto.CompanyCode, lastT.Desc, DateTime.Parse(lastT.Time)));
                }
                return ls;
            }
            else
            {
                var sql = $@"
select Nu,Company,LastJStr from KuaidiNuData where ( {(string.Join(" or ", query.Nus.Select(x => $"(Nu='{x.Nu}' and Company='{x.Comcode}')")))} )
";
                var ls = await _orgUnitOfWork.QueryAsync<(string, string, string)>(sql, new { query.Nus });
                return ls.Select(x =>
                {
                    if (x.Item3?.ToObject<KuaidiNuDataItemDto>() is KuaidiNuDataItemDto item)
                    {
                        return (x.Item1, x.Item2, item.Desc, DateTime.Parse(item.Time));
                    }
                    return default;
                }).Where(_ => _.Item1 != null).AsArray();
            }
        }

    }
}

