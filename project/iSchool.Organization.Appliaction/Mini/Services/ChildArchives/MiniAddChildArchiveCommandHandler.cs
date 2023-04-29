using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class MiniAddChildArchiveCommandHandler : IRequestHandler<MiniAddChildArchiveCommand, ResponseResult>
    {

        private readonly IUserInfo me;
        private readonly IRepository<ChildArchives> _childArchivesRepository;
        private CSRedisClient redis;

        public MiniAddChildArchiveCommandHandler(IUserInfo me, IRepository<ChildArchives> childArchivesRepository, CSRedisClient redis)
        {
            this.me = me;
            _childArchivesRepository = childArchivesRepository;
            this.redis = redis;
        }

        public Task<ResponseResult> Handle(MiniAddChildArchiveCommand request, CancellationToken cancellationToken)
        {
            var childArchives = new ChildArchives()
            {
                Id = Guid.NewGuid(),
                HeadImg = request.HeadImg,
                BirthDate = request.BirthDate,
                CreateTime = DateTime.Now,
                Creator = me.UserId,
                Modifier = me.UserId,
                ModifyDateTime = DateTime.Now,
                NikeName = request.NikeName,
                Sort = 99,
                UserId = me.UserId,
                IsValid = true,
                Sex = request.Sex,
                Subjects = request.Subjs.ToJsonString()
            };
            var res = _childArchivesRepository.Insert(childArchives);

            //删除缓存
            var key = string.Format(CacheKeys.MyChildArchives, me.UserId);
            redis.Del(key);

            // 添加后 发积分?等
            {
                AsyncUtils.StartNew((sp, _) =>
                {
                    return sp.GetService<IntegrationEvents.IOrganizationIntegrationEventService>().PublishEventAsync(new IntegrationEvents.Events.AddChildIntegrationEvent
                    {
                        Id = childArchives.Id,
                        UserId = me.UserId,
                        Name = childArchives.NikeName,
                        CreateTime = childArchives.CreateTime ?? default,
                    });
                });
            }

            return Task.FromResult(ResponseResult.Success(childArchives.Id, "添加成功"));
        }
    }
}
