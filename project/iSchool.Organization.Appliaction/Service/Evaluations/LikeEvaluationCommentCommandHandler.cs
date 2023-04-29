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
    public class LikeEvaluationCommentCommandHandler : IRequestHandler<LikeEvaluationCommentCommand>
    {
        OrgUnitOfWork unitOfWork;
        IUserInfo me;
        IMediator mediator;
        private readonly IRepository<EvaluationComment> _evaluationCommentRepository;
        private readonly CSRedisClient _redisClient;
        private readonly IRepository<Like> _likeRepository;



        public LikeEvaluationCommentCommandHandler(IOrgUnitOfWork unitOfWork,
            IUserInfo me,
            IMediator mediator,
            IRepository<EvaluationComment> evaluationCommentRepository
            , CSRedisClient redisClient,
            IRepository<Like> likeRepository)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            this.mediator = mediator;
            _evaluationCommentRepository = evaluationCommentRepository;
            _redisClient = redisClient;
            _likeRepository = likeRepository;
        }

        public async Task<Unit> Handle(LikeEvaluationCommentCommand cmd, CancellationToken cancellation)
        {
            //// 需要判断是否绑定了手机号
            //await mediator.Send(new CheckUserBindMobileCommand { ThrowIfNoBind = true });
            //评测中评论点赞
            //先入缓存
            //然后定时从缓存中更新到数据库
            //从redis中获取evaluation信息
            var evaluation = await mediator.Send(new EvaluationSimpleQuery { Id = cmd.EvaluationId });

            if (evaluation.Status != (byte)EvaluationStatusEnum.Ok)
                throw new CustomResponseException("当前状态不能点赞！");

            var comment = _evaluationCommentRepository.Get(p => p.IsValid == true && p.Id == cmd.EvltCommentId);
            if (comment == null || comment.Evaluationid != cmd.EvaluationId)
                throw new CustomResponseException("评论不存在！");

            try
            {
                var exists = _redisClient.HExists(string.Format(CacheKeys.EvaluationCommentLikesCount, cmd.EvaluationId), cmd.EvltCommentId.ToString());

                if (!exists) _redisClient.HSet(string.Format(CacheKeys.EvaluationCommentLikesCount, cmd.EvaluationId), cmd.EvltCommentId.ToString(), comment.Likes ?? 0);

                //判断用户是否已经点赞了,点赞了不操作
                var commentLikeExist = _redisClient.HExists(string.Format(CacheKeys.MyCommentLikes, me.UserId, cmd.EvaluationId),
                            cmd.EvltCommentId.ToString());
                if ((cmd.IsLike && !commentLikeExist) || (!cmd.IsLike && commentLikeExist))
                {
                    //我的评测评论下的点赞
                    if (cmd.IsLike)
                        _redisClient.HSet(string.Format(CacheKeys.MyCommentLikes, me.UserId, cmd.EvaluationId),
                            cmd.EvltCommentId.ToString(), DateTime.Now.ToString());
                    else
                        _redisClient.HDel(string.Format(CacheKeys.MyCommentLikes, me.UserId, cmd.EvaluationId),
                            cmd.EvltCommentId.ToString());


                    //评测下评论的数量
                    var count = _redisClient
                        .HIncrBy(string.Format(CacheKeys.EvaluationCommentLikesCount, cmd.EvaluationId),
                        cmd.EvltCommentId.ToString(), cmd.IsLike ? 1 : -1);


                    //记录点赞的行为
                    _redisClient.HSet(CacheKeys.CommentLikeAction, $"{cmd.EvaluationId}|{cmd.EvltCommentId}|{me.UserId}", $"{Infrastructure.Common.TimeHelp.ToUnixTimestampByMilliseconds(DateTime.Now)}-{(cmd.IsLike ? 1 : 0)}");
                    //清回复列表页缓存
                    await _redisClient.DelAsync(CacheKeys.EvltCommentChildrendCommentTop10.FormatWith(cmd.EvltCommentId));
                }
        
            }
            catch (Exception)
            {

                //如果redis 发生异常直接操作数据库
                if (cmd.IsLike)
                {
                    _likeRepository.Insert(new Like
                    {
                        Id = Guid.NewGuid(),
                        Commentid = cmd.EvltCommentId,
                        Evaluationid = cmd.EvaluationId,
                        Useid = me.UserId,
                        CreateTime = DateTime.Now,
                        Type = (byte)LikeType.Comment
                    });
                }
                else
                {
                    unitOfWork.DbConnection.Execute(
                        "DELETE dbo.[Like] WHERE type=@type AND evaluationid=@evalid  AND useid=@userid  AND commentid=@commentid",
                        new
                        {
                            type = (byte)LikeType.Comment,
                            userid = me.UserId,
                            evalid = cmd.EvaluationId,
                            commentid = cmd.EvltCommentId
                        });
                }
            }
            return default;
        }
    }
}
