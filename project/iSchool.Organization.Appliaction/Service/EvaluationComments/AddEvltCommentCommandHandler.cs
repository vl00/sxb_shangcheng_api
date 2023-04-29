using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class AddEvltCommentCommandHandler : IRequestHandler<AddEvltCommentCommand, AddEvltCommentDto>
    {
        OrgUnitOfWork unitOfWork;
        IUserInfo me;
        IMediator mediator;
        CSRedisClient redis;
        IRepository<iSchool.Organization.Domain.EvaluationComment> _evalCmtRepo;


        public AddEvltCommentCommandHandler(IRepository<iSchool.Organization.Domain.EvaluationComment> evalCmtRepo, IOrgUnitOfWork unitOfWork, IUserInfo me, IMediator mediator, CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            this.mediator = mediator;
            this.redis = redis;
            this._evalCmtRepo = evalCmtRepo;
        }

        void Valid_cmd(AddEvltCommentCommand cmd)
        {
            if (string.IsNullOrEmpty(cmd.Comment) || string.IsNullOrWhiteSpace(cmd.Comment))
                throw new CustomResponseException("评论内容不能为空");
            if (cmd.Comment.Length > 140)
                throw new CustomResponseException("评论内容不能超过140字");
            if (null != cmd.EvltCommentId && Guid.Empty != cmd.EvltCommentId)
            {
                if (!_evalCmtRepo.IsExist(x => x.Id == cmd.EvltCommentId.Value)) throw new CustomResponseException("参数有误_parentid");
            }
        }

        public async Task<AddEvltCommentDto> Handle(AddEvltCommentCommand cmd, CancellationToken cancellation)
        {
            Valid_cmd(cmd);
            do
            {
                var rdk = CacheKeys.Tdiff4UserAddEvltComment.FormatWith(me.UserId);
                var ttl = await redis.TtlAsync(rdk);
                if (ttl > -2) throw new CustomResponseException("发评论后3秒内不能频繁再发");
                await redis.SetAsync(rdk, 1, 3, RedisExistence.Nx);
            }
            while (false);

            // check if 有敏感词
            {
                var txt = cmd.Comment;
                var trst = await mediator.Send(new SensitiveKeywordCmd { Txt = txt });
                if (!trst.Pass)
                {
                    if (trst.FilteredTxt == null)
                        throw new CustomResponseException("您发表的点评有敏感词，请修改后再发", ResponseCode.GarbageContent.ToInt());

                    cmd.Comment = trst.FilteredTxt;
                }
            }
            var isAuthor = false;
            var commentId = Guid.NewGuid();
            var IsChilddrenComment = null != cmd.EvltCommentId && Guid.Empty != cmd.EvltCommentId.Value;

            string query_sql = $@" select * from Evaluation where Id=@Id;";
            var EvaluationModel = unitOfWork.DbConnection.Query<Evaluation>(query_sql, new { Id = cmd.EvltId }).FirstOrDefault();
            if (null == EvaluationModel || !EvaluationModel.IsValid|| EvaluationModel.Status!=(int)EvaluationStatusEnum.Ok)
            {
                throw new CustomResponseException("对应的评测不存在", 404);
            }
            isAuthor = EvaluationModel.Userid == me.UserId;

            var db_EvaluationComment = new EvaluationComment();
            try
            {
                unitOfWork.BeginTransaction();


                if (IsChilddrenComment)
                {
                    db_EvaluationComment.Fromid = cmd.EvltCommentId;
                    var updateCommentCountSql = "update EvaluationComment set CommentCount=CommentCount+1 where Id=@EvltCommentId";
                    await unitOfWork.DbConnection.ExecuteAsync(updateCommentCountSql, new { cmd.EvltCommentId }, unitOfWork.DbTransaction);
                }
                db_EvaluationComment.Id = commentId;
                db_EvaluationComment.Evaluationid = cmd.EvltId;
                db_EvaluationComment.Userid = me.UserId;
                db_EvaluationComment.Username = me.UserName;
                db_EvaluationComment.Comment = cmd.Comment;
                db_EvaluationComment.CreateTime = DateTime.Now;
                db_EvaluationComment.Creator = me.UserId;
                db_EvaluationComment.ModifyDateTime = DateTime.Now;
                db_EvaluationComment.Modifier = me.UserId;
                db_EvaluationComment.IsValid = true;
                await unitOfWork.DbConnection.InsertAsync(db_EvaluationComment, unitOfWork.DbTransaction);

                var sql = @"update Evaluation set commentcount=commentcount+1 where Id=@EvltId ";
                await unitOfWork.DbConnection.ExecuteAsync(sql, new { cmd.EvltId }, unitOfWork.DbTransaction);

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                try { unitOfWork.Rollback(); } catch { }
                throw ex;
            }

            var ro = await redis.StartPipe()
                .HExists(CacheKeys.EvaluationLikesCount.FormatWith(cmd.EvltId), "comments")
                .EndPipeAsync();

            if (Equals(ro[0], true))
            {
                await redis.HIncrByAsync(CacheKeys.EvaluationLikesCount.FormatWith(cmd.EvltId), "comments");
            }

            // del cache
            await redis.BatchDelAsync(CacheKeys.EvltCommentTopN.FormatWith("*", cmd.EvltId), 5);
            if (IsChilddrenComment)
            {
                await redis.DelAsync(CacheKeys.EvltCommentChildrendCommentTop10.FormatWith(cmd.EvltCommentId));
            }

            if (!IsChilddrenComment)
            {
                //评论数量，不包含回复
                await redis.HIncrByAsync(CacheKeys.EvaluationLikesCount.FormatWith(cmd.EvltId), "firstcomments");
            }

            var r = new AddEvltCommentDto();
            r.Id = db_EvaluationComment.Id;
            r.Comment = db_EvaluationComment.Comment;
            r.CreateTime = db_EvaluationComment.CreateTime;
            r.AuthorId = me.UserId;
            r.Username = me.UserName;
            r.UserImg = me.HeadImg;
            r.Likes = 0;//前端要求这个没用的字段
            r.IsAuthor = isAuthor;
            return r;
        }
    }
}
