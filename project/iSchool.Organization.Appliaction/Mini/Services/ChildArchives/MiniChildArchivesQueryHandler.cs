using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Infrastructure;
using System.Linq;

namespace iSchool.Organization.Appliaction.Services
{
    public class MiniChildArchivesQueryHandler : IRequestHandler<MiniChildArchivesQuery, List<MiniChildArchiveItemDto>>
    {
        private readonly IUserInfo me;
        private readonly IRepository<ChildArchives> _childArchivesRepository;
        private CSRedisClient redis;

        public MiniChildArchivesQueryHandler(IUserInfo me, IRepository<ChildArchives> childArchivesRepository, CSRedisClient redis)
        {
            this.me = me;
            _childArchivesRepository = childArchivesRepository;
            this.redis = redis;
        }

        public Task<List<MiniChildArchiveItemDto>> Handle(MiniChildArchivesQuery request, CancellationToken cancellationToken)
        {
            var key = string.Format(CacheKeys.MyChildArchives, me.UserId);
            var data = redis.Get<List<MiniChildArchiveItemDto>>(key);
            if (data == null)
            {
                data = _childArchivesRepository
                    .GetAll(p => p.IsValid == true && p.UserId == me.UserId)
                    .OrderBy(p => p.Sort)
                    .ThenBy(p => p.CreateTime)
                    .Select(p => new MiniChildArchiveItemDto
                    {
                        Id = p.Id,
                        BirthDate = p.BirthDate,
                        HeadImg = p.HeadImg,
                        NikeName = p.NikeName,
                        Sex = p.Sex,
                        ChildrenAge = p.BirthDate == null ?
                        0 : 12 * (DateTime.Now.Year - p.BirthDate.Value.Year) + (DateTime.Now.Month - p.BirthDate.Value.Month) + 1,
                        Subjs = string.IsNullOrEmpty(p.Subjects) ? new List<KeyValuePair<string, string>>() :
                        JsonSerializationHelper.JSONToObject<IEnumerable<KeyValuePair<string, string>>>(p.Subjects),
                        UserId = me.UserId
                    }).ToList();

                redis.Set(key, data, 60 * 60);
            }

            return Task.FromResult(data);
        }
    }
}
