using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class OrganizationBrandQueryHandler : IRequestHandler<OrganizationBrandQuery, List<MiniOrganizationItemDto>>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;

        public OrganizationBrandQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public Task<List<MiniOrganizationItemDto>> Handle(OrganizationBrandQuery request, CancellationToken cancellationToken)
        {


            var sql = @"
				SELECT org.id,
                       org.name,
                       org.logo,
                       org.intro,
                       org.status,
                       org.authentication,
                       org.types,
                       org.ages,
                       org.modes,
                       org.CreateTime,
                       org.Creator,
                       org.ModifyDateTime,
                       org.Modifier,
                       org.IsValid,
                       org.No,
                       org.minage,
                       org.maxage,
                       org.hot_score,
                       org.download_score,
                       org.score_total,
                       org.subjects,
                       org.[desc],
                       org.subdesc FROM dbo.OrganizationBrand AS brand LEFT JOIN dbo.Organization AS org
				ON brand.OrgId =org.id WHERE
                 org.IsValid=1 AND org.status=1 ORDER BY brand.Sort";

            var data = _orgUnitOfWork.Query<Domain.Organization>(sql).Select(p => new MiniOrganizationItemDto()
            {
                Id = p.Id,
                Id_s = UrlShortIdUtil.Long2Base32(p.No),
                Logo = p.Logo ?? "https://www3.sxkid.com/pc/_nuxt/img/a4ab5e2.png",
                Authentication = p.Authentication,
                Name = p.Name
            }).ToList();

            return Task.FromResult(data);
        }
    }
}
