using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.Evaluations;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class AddVoteAfterEvaluationAddedCommandHandler : IRequestHandler<AddVoteAfterEvaluationAddedCommand, bool>
    {
        IUserInfo me;
        IMediator mediator;
        OrgUnitOfWork _unitOfWork;
        private readonly IRepository<EvaluationVote> _voteRepository;
        private readonly IRepository<EvaluationVoteItems> _voteItemsRepository;
        CSRedisClient redis;


        public AddVoteAfterEvaluationAddedCommandHandler(
            IUserInfo me,
            IMediator mediator,
            IRepository<EvaluationVote> voteRepository,
            IRepository<EvaluationVoteItems> voteItemsRepository,
            IOrgUnitOfWork unitOfWork,
            CSRedisClient redis)
        {
            this.me = me;
            this.mediator = mediator;
            _voteRepository = voteRepository;
            _voteItemsRepository = voteItemsRepository;
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.redis = redis;
        }

        void valid_cmd(AddVoteAfterEvaluationAddedCommand cmd)
        {
            if (cmd.Title.IsNullOrEmpty())
                throw new CustomResponseException("投票标题不能为空");
            if (cmd.EndTime != null && cmd.EndTime <= DateTime.Now)
                throw new CustomResponseException("投票结束时间不能小于当前时间");
            if (cmd.Items?.Any() != true || cmd.Items.Any(_ => string.IsNullOrEmpty(_)))
                throw new CustomResponseException("投票选项不能为空");
            if (cmd.Items!.Any(_ => _.Length > 10))
                throw new CustomResponseException("投票选项内容不能超过10个字");
        }

        public async Task<bool> Handle(AddVoteAfterEvaluationAddedCommand cmd, CancellationToken cancellation)
        {
            valid_cmd(cmd);

            //从redis中获取evaluation信息
            var evaluation = await mediator.Send(new EvaluationSimpleQuery { Id = cmd.EvltId });

            //没有权限
            if (evaluation.UserId != me.UserId)
                throw new AuthResponseException();

            if (evaluation.Status != (byte)EvaluationStatusEnum.Ok)
                throw new CustomResponseException("当前状态不能发起投票");

            var vote = _voteRepository.Get(p => p.IsValid == true && p.Evaluationid == cmd.EvltId);

            // check if 有敏感词
            {
                var txts = Enumerable.Repeat(cmd.Title, 1).Append(cmd.Detail).Union(cmd.Items).ToArray();
                var trst = await mediator.Send(new SensitiveKeywordCmd { Txts = txts });
                if (!trst.Pass) throw new CustomResponseException("您发表的内容有敏感词，请修改后再发");
            }

            if (vote != null)
                throw new CustomResponseException("投票已经存在！");

            var voteid = Guid.NewGuid();
            try
            {
                _unitOfWork.BeginTransaction();

                vote = new EvaluationVote
                {
                    Id = voteid,
                    Title = cmd.Title,
                    Type = (byte)VoteType.Single,//这一版选项只有单选
                    Detail = cmd.Detail,
                    Endtime = cmd.EndTime,
                    Evaluationid = cmd.EvltId,
                    IsValid = true
                };

                var items = new List<EvaluationVoteItems>();
                for (int i = 0; i < cmd.Items.Count(); i++)
                {
                    items.Add(new EvaluationVoteItems
                    {
                        Id = Guid.NewGuid(),
                        VoteId = voteid,
                        Content = cmd.Items[i],
                        Count = 0,
                        Sort = i,
                        IsValid = true
                    });
                }
                _voteRepository.Insert(vote);
                _voteItemsRepository.BatchInsert(items);

                _unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }

            _ = redis.HDelAsync(string.Format(CacheKeys.Evlt, cmd.EvltId), "vote");

            return true;
        }
    }
}
