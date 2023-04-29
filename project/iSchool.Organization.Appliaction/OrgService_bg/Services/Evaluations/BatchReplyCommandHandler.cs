using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using iSchool.Infrastructure;
using CSRedis;
using iSchool.Organization.Domain;
using System.Linq;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 批量更新回复内容
    /// </summary>
    public class BatchReplyCommandHandler : IRequestHandler<BatchReplyCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public BatchReplyCommandHandler(IOrgUnitOfWork unitOfWork , CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }

        public Task<ResponseResult> Handle(BatchReplyCommand request, CancellationToken cancellationToken)
        {
            if (request.ListReplys?.Any() == true)
            {
                string key = "org:evlt:comment:top20:evlt_{0}".FormatWith(request.EvltId);
                var dp = new DynamicParameters();
                StringBuilder sBuilder = new StringBuilder();
                var replys = request.ListReplys.ToList();
                for (int i = 0; i < replys.Count(); i++)
                {
                    sBuilder.AppendFormat($" update EvaluationComment set comment=@comment_{i} where id=@id_{i} ");
                    dp.Set($"comment_{i}", replys[i].Comment)
                      .Set($"id_{i}", replys[i].Id);
                }
                
                try
                {
                    _orgUnitOfWork.BeginTransaction();
                    _orgUnitOfWork.DbConnection.Execute(sBuilder.ToString(), dp,_orgUnitOfWork.DbTransaction);
                    _orgUnitOfWork.CommitChanges();
                    _redisClient.Del(key);
                    return Task.FromResult(ResponseResult.Success("回复已更新"));
                }
                catch(Exception ex)
                {
                    _orgUnitOfWork.Rollback();
                    return Task.FromResult(ResponseResult.Failed($"系统错误{ex.Message}"));
                }
               
            }
            else
            {
                return Task.FromResult(ResponseResult.Failed("回复内容没有变更"));
            }
            
        }
    }
}
