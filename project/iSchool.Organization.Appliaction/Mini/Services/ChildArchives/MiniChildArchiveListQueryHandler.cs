using iSchool.Infrastructure;
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
    public class MiniChildArchiveListQueryHandler : IRequestHandler<MiniChildArchiveListQuery, List<MiniChildArchiveItemDto>>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;

        public MiniChildArchiveListQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public Task<List<MiniChildArchiveItemDto>> Handle(MiniChildArchiveListQuery request, CancellationToken cancellationToken)
        {
            var sql = @"SELECT * FROM dbo.ChildArchives  AS child WHERE
 id=(SELECT TOP 1 id FROM dbo.ChildArchives WHERE UserId=child.UserId AND IsValid=1 ORDER BY Sort,CreateTime)
 AND child.UserId IN @userid AND child.IsValid=1";

            var data = _orgUnitOfWork.Query<ChildArchives>(sql, new { userid = request.UserIds })
                .Select(p => new MiniChildArchiveItemDto
                {
                    UserId=p.UserId,
                    Id = p.Id,
                    BirthDate = p.BirthDate,
                    HeadImg = p.HeadImg,
                    NikeName = p.NikeName,
                    Sex = p.Sex,
                    Subjs = JsonSerializationHelper.JSONToObject<IEnumerable<KeyValuePair<string, string>>>(p.Subjects)
                }).ToList();
            return Task.FromResult(data);

        }
    }
}
