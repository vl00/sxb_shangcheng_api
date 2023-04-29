using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
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

namespace iSchool.Organization.Appliaction.Services
{
    public class MiniDeleteChildArchiveCommandHandler : IRequestHandler<MiniDeleteChildArchiveCommand, ResponseResult>
    {
        private readonly IUserInfo me;
        private readonly IRepository<ChildArchives> _childArchivesRepository;
        private CSRedisClient redis;

        public MiniDeleteChildArchiveCommandHandler(IUserInfo me, IRepository<ChildArchives> childArchivesRepository, CSRedisClient redis)
        {
            this.me = me;
            _childArchivesRepository = childArchivesRepository;
            this.redis = redis;
        }

        public Task<ResponseResult> Handle(MiniDeleteChildArchiveCommand request, CancellationToken cancellationToken)
        {
            var data = _childArchivesRepository
                .Get(p => p.IsValid == true && p.UserId == me.UserId && p.Id == request.Id);

            if (data == null)
            {
                throw new CustomResponseException("查询不到档案.");
            }

            data.IsValid = false;
            _childArchivesRepository.Update(data);

            //删除缓存
            var key = string.Format(CacheKeys.MyChildArchives, me.UserId);
            redis.Del(key);


            return Task.FromResult(ResponseResult.Success(true));
        }
    }
}
