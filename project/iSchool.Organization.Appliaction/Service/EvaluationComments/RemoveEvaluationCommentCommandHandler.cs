using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 删除评测评论
    /// </summary>
    public class RemoveEvaluationCommentCommandHandler : IRequestHandler<RemoveEvaluationCommentCommand, ResponseResult>
    {
        OrgUnitOfWork orgUnitOfWork;
        CSRedisClient _redisClient;
        IUserInfo _me;

        public RemoveEvaluationCommentCommandHandler(IOrgUnitOfWork unitOfWork
            , CSRedisClient redisClient,
           IUserInfo me
            )
        {
            this.orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._redisClient = redisClient;
            this._me = me;

        }


        public async Task<ResponseResult> Handle(RemoveEvaluationCommentCommand request, CancellationToken cancellationToken)
        {
            try
            {
              
                if (!_me.IsAuthenticated) throw new CustomResponseException("未登录"); 
                var dy = new DynamicParameters();
                dy.Add("@Id", request.Id);
                string sql = $@" select * from EvaluationComment where Id=@Id;";
                var removeModel = orgUnitOfWork.Query<EvaluationComment>(sql, dy).FirstOrDefault();
                if (null == removeModel)
                {
                    throw new CustomResponseException("参数错误");
                  
                }

                if (_me.UserId != removeModel.Userid)
                {
                    throw new CustomResponseException("非法操作");
                  
                }

              
                var delSql = $@" update EvaluationComment set IsValid=0 where Id=@Id ;";
               

                if (null != removeModel.Fromid && Guid.Empty != removeModel.Fromid)//回复
                {
                    delSql+= "update EvaluationComment set CommentCount=CommentCount-1 where Id=@EvltCommentId;";
                    dy.Add("@EvltCommentId", removeModel.Fromid);

                }
                orgUnitOfWork.BeginTransaction();
                await orgUnitOfWork.ExecuteAsync(delSql, dy, orgUnitOfWork.DbTransaction);
                var updateEvltCommentCountSql = "update Evaluation set CommentCount=CommentCount-1 where Id=@EvltId";
                await orgUnitOfWork.ExecuteAsync(updateEvltCommentCountSql, new { EvltId = removeModel.Evaluationid }, orgUnitOfWork.DbTransaction);

                orgUnitOfWork.CommitChanges();
              
                // 清理缓存
                await _redisClient.BatchDelAsync(CacheKeys.EvltCommentTopN.FormatWith("*", removeModel.Evaluationid), 5);

                var ro = await _redisClient.StartPipe()
                     .HExists(CacheKeys.EvaluationLikesCount.FormatWith(removeModel.Evaluationid), "comments")
                     .Del(CacheKeys.EvltCommentChildrendCommentTop10.FormatWith(removeModel.Fromid))
                     .EndPipeAsync();
                if (Equals(ro[0], true))
                {
                    await _redisClient.HIncrByAsync(CacheKeys.EvaluationLikesCount.FormatWith(removeModel.Evaluationid), "comments",-1);
                }
                if (null == removeModel.Fromid || Guid.Empty == removeModel.Fromid)
                {
                    //评论数量，不包含回复
                    await _redisClient.HIncrByAsync(CacheKeys.EvaluationLikesCount.FormatWith(removeModel.Evaluationid), "firstcomments", -1);
                }

                return ResponseResult.Success("操作成功");
            }
            catch (Exception ex)
            {
                try { orgUnitOfWork.Rollback(); } catch { }
           
                return ResponseResult.Failed(ex.Message);
            }
        }




    }
}
