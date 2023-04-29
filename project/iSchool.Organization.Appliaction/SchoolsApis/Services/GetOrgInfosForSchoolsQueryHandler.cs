using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.SchoolsApis
{
    public class GetOrgInfosForSchoolsQueryHandler : IRequestHandler<GetOrgInfosForSchoolsQuery, GetOrgInfosForSchoolsQryResult>
    {
        IConfiguration _config;
        CSRedisClient _redis;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;

        public GetOrgInfosForSchoolsQueryHandler(IConfiguration config, CSRedisClient redis, IOrgUnitOfWork orgUnitOfWork,
            IMediator mediator)
        {
            this._config = config;
            this._redis = redis;
            this._mediator = mediator;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<GetOrgInfosForSchoolsQryResult> Handle(GetOrgInfosForSchoolsQuery query, CancellationToken cancellation)
        {
            var result = new GetOrgInfosForSchoolsQryResult();
            var orgs = new List<PcOrgItemDto>();

            for (var i = 0; i < (query.Ids?.Length ?? 0); i++)
            {
                try
                {
                    var org = await _mediator.Send(new OrgzBaseInfoQuery { OrgId = query.Ids[i], AllowNotValid = true });
                    orgs.Add(new PcOrgItemDto
                    {
                        Id = org.Id,
                        Id_s = UrlShortIdUtil.Long2Base32(org.No),
                        Name = org.Name,
                        Logo = org.Logo,
                        Authentication = org.Authentication,
                        Desc = org.Desc,
                        Subdesc = org.Subdesc,
                        IsOnline = org.IsValid && org.Status == OrganizationStatusEnum.Ok.ToInt(),
                    });
                }
                catch { }
            }

            for (var i = 0; i < (query.SIds?.Length ?? 0); i++)
            {
                try
                {
                    var org = await _mediator.Send(new OrgzBaseInfoQuery { No = UrlShortIdUtil.Base322Long(query.SIds[i]), AllowNotValid = true });
                    orgs.Add(new PcOrgItemDto
                    {
                        Id = org.Id,
                        Id_s = UrlShortIdUtil.Long2Base32(org.No),
                        Name = org.Name,
                        Logo = org.Logo,
                        Authentication = org.Authentication,
                        Desc = org.Desc,
                        Subdesc = org.Subdesc,
                        IsOnline = org.IsValid && org.Status == OrganizationStatusEnum.Ok.ToInt(),
                    });
                }
                catch { }
            }

            // counts
            {
                var dict = await _mediator.Send(new PcGetOrgsCountsQuery { OrgIds = orgs.Select(_ => _.Id) });
                foreach (var item in orgs)
                {
                    if (!dict.TryGetValue(item.Id, out var m)) continue;
                    item.CourceCount = m.CourceCount;
                    item.EvaluationCount = m.EvaluationCount;
                    item.GoodsCount = m.GoodsCount;
                }
            }

            // urls            
            foreach (var item in orgs)
            {
                item.MUrl = $"{_config["BaseUrls:org-m"]}/course/detail/{item.Id_s}";
                item.PcUrl = $"{_config["BaseUrls:org-pc"]}/course/detail/{item.Id_s}";

                // mpqrcode
                if (query.Mp)
                {
                    try
                    {
                        var cmd1 = _config.GetSection("AppSettings:CreateMpQrcode:course-detail").Get<CreateMpQrcodeCmd>();
                        cmd1.Scene = cmd1.Scene.FormatWith(item.Id_s);
                        item.MpQrcode = (await _mediator.Send(cmd1)).MpQrcode;
                    }
                    catch { /* ignore */ }
                }

                // h5ToMpUrl
                try
                {
                    var cakey = CacheKeys.H5ToMpUrl.FormatWith(_config["AppSettings:CreateMpQrcode:org-detail:Page"], $"orgnoid={item.Id_s}");
                    item.H5ToMpUrl = await _redis.GetAsync(cakey);
                }
                catch { /* ignore */ }
                item.H5ToMpUrl = item.H5ToMpUrl.IsNullOrEmpty() ? null : item.H5ToMpUrl;
            }

            result.Orgs = orgs;
            return result;
        }



    }
}
