
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    [Obsolete("可用旧的删除方法")]
    public class MiniDelEvaluationCommandHandler : IRequestHandler<MiniDelEvaluationCommand, bool>
    {
        private readonly IUserInfo _me;
        private readonly IRepository<Evaluation> _evaluationRepository;

        public MiniDelEvaluationCommandHandler(IUserInfo me, IRepository<Evaluation> evaluationRepository)
        {
            _me = me;
            _evaluationRepository = evaluationRepository;
        }

        public Task<bool> Handle(MiniDelEvaluationCommand request, CancellationToken cancellationToken)
        {
            var eval = _evaluationRepository.Get(p => p.IsValid == true
            && p.Id == request.Id && p.Userid == _me.UserId);
            if (eval == null)
                throw new CustomResponseException("查询不到该评测。");

            eval.IsValid = false;
            _evaluationRepository.Update(eval);
            return Task.FromResult(true);
        }
    }
}
