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

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 
    /// </summary>
    public class EvltCommentCommandHandler : IRequestHandler<EvltCommentCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public EvltCommentCommandHandler(IOrgUnitOfWork unitOfWork , CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }

        public Task<ResponseResult> Handle(EvltCommentCommand request, CancellationToken cancellationToken)
        {
            string key = "org:evlt:comment:top20:evlt_{0}".FormatWith(request.EvltId);
            _orgUnitOfWork.DbConnection.Execute($" update EvaluationComment set comment=@comment where id=@id ", 
                new DynamicParameters()
                .Set("id",request.Id)
                .Set("comment",request.Comment));
            _redisClient.Del(key);
            return Task.FromResult(ResponseResult.Success("评论已更新"));
        }
    }
}
