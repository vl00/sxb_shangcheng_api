using AutoMapper;
using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Evaluations;
using iSchool.Organization.Appliaction.ResponseModels.Evaluations;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    public class EvaluationSimpleQueryHandler : IRequestHandler<EvaluationSimpleQuery, EvaluationSimpleDto>
    {

        private readonly IRepository<Evaluation> _evaluationRepository;
        private readonly CSRedisClient _redisClient;
        private readonly IUserInfo _userInfo;
        private readonly IMapper _mapper;

        public EvaluationSimpleQueryHandler(IRepository<Evaluation> evaluationRepository, CSRedisClient redisClient, IUserInfo userInfo, IMapper mapper)
        {
            _evaluationRepository = evaluationRepository;
            _redisClient = redisClient;
            _userInfo = userInfo;
            _mapper = mapper;
        }

        public Task<EvaluationSimpleDto> Handle(EvaluationSimpleQuery request, CancellationToken cancellationToken)
        {
            var redisKey = string.Format(CacheKeys.simpleevaluation, request.Id);
            var data = _redisClient.Get<EvaluationSimpleDto>(redisKey);
            if (data == null)
            {
                var evaluation = _evaluationRepository.Get(p => p.IsValid == true && p.Id == request.Id);
                if (evaluation == null) throw new CustomResponseException("评测不存在！",404);
                data = _mapper.Map<EvaluationSimpleDto>(evaluation);
                _redisClient.Set(redisKey, data, TimeSpan.FromDays(1));
            }

            return Task.FromResult(data);
        }
    }
}
