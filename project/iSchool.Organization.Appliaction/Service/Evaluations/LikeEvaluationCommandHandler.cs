using CSRedis;
using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.Evaluations;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class LikeEvaluationCommandHandler : IRequestHandler<LikeEvaluationCommand, bool>
    {
        OrgUnitOfWork unitOfWork;
        IUserInfo me;
        IMediator mediator;
        private readonly IRepository<Like> _likeRepository;
        private readonly CSRedisClient _redisClient;
        private readonly IRepository<Evaluation> _evaluationRepository;


        public LikeEvaluationCommandHandler(IOrgUnitOfWork unitOfWork, IUserInfo me, IMediator mediator,
        IRepository<Like> likeRepository, CSRedisClient redisClient, IRepository<Evaluation> evaluationRepository)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            this.mediator = mediator;
            _likeRepository = likeRepository;
            _redisClient = redisClient;
            _evaluationRepository = evaluationRepository;
        }

        public async Task<bool> Handle(LikeEvaluationCommand cmd, CancellationToken cancellation)
        {
            //评测点赞
            //先入缓存
            //然后定时从缓存中更新到数据库

            //从redis中获取evaluation信息
            var evaluation = await mediator.Send(new EvaluationSimpleQuery { Id = cmd.EvaluationId });


            if (evaluation.Status != (byte)EvaluationStatusEnum.Ok)
                throw new CustomResponseException("当前状态不能点赞！");




            try
            {
                //判断key 是否存在
                var exists = _redisClient.HExists(string.Format(CacheKeys.EvaluationLikesCount, cmd.EvaluationId), "like");
                if (!exists)
                {
                    var eval = _evaluationRepository.Get(p => p.IsValid == true && p.Id == cmd.EvaluationId);

                    _redisClient.HSet(string.Format(CacheKeys.EvaluationLikesCount, cmd.EvaluationId), "like", eval.Likes);
                }

                //判断用户是否已经点赞了，点赞了则不操作
                if (_redisClient.SetNx($"org:EvalLock_{me.UserId.ToString()}_{cmd.EvaluationId.ToString()}", 1))
                {
                    _redisClient.Expire($"org:EvalLock_{me.UserId.ToString()}_{cmd.EvaluationId.ToString()}", 3);
                    var evalLikeExist = _redisClient.HExists(string.Format(CacheKeys.MyEvaluationLikes, me.UserId), cmd.EvaluationId.ToString());
                    if ((cmd.IsLike && !evalLikeExist) || (!cmd.IsLike && evalLikeExist))
                    {

                        //评测点赞总数量加一或减一
                        long sum = _redisClient.HIncrBy(string.Format(CacheKeys.EvaluationLikesCount, cmd.EvaluationId), "like", cmd.IsLike ? 1 : -1);

                        //我的评测hash 操作
                        if (cmd.IsLike)
                            _redisClient.HSet(string.Format(CacheKeys.MyEvaluationLikes, me.UserId), cmd.EvaluationId.ToString(), DateTime.Now.ToString());
                        else
                            _redisClient.HDel(string.Format(CacheKeys.MyEvaluationLikes, me.UserId), cmd.EvaluationId.ToString());

                        //记录点赞的行为
                        _redisClient.HSet(CacheKeys.EvaluationLikeAction, $"{cmd.EvaluationId}|{me.UserId}", $"{Infrastructure.Common.TimeHelp.ToUnixTimestampByMilliseconds(DateTime.Now)}-{(cmd.IsLike ? 1 : 0)}");

                        _redisClient.Del($"org:EvalLock_{me.UserId.ToString()}_{cmd.EvaluationId.ToString()}");
                    }
                    else
                    {
                        _redisClient.Del($"org:EvalLock_{me.UserId.ToString()}_{cmd.EvaluationId.ToString()}");
                        return false;

                    }
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception)
            {
                _redisClient.Del($"org:EvalLock_{me.UserId.ToString()}_{cmd.EvaluationId.ToString()}");
                //如果redis 发生异常直接操作数据库
                if (cmd.IsLike)
                {
                    _likeRepository.Insert(new Like
                    {
                        Id = Guid.NewGuid(),
                        Commentid = null,
                        Evaluationid = cmd.EvaluationId,
                        Useid = me.UserId,
                        CreateTime = DateTime.Now,
                        Type = (byte)LikeType.Evaluation
                    });
                }
                else
                {
                    unitOfWork.DbConnection.Execute(
                        "DELETE dbo.[Like] WHERE type=@type AND evaluationid=@evalid  AND useid=@userid",
                        new
                        {
                            type = (byte)LikeType.Evaluation,
                            userid = me.UserId,
                            evalid = cmd.EvaluationId
                        });
                }
            }
            return true;
        }
    }
}
