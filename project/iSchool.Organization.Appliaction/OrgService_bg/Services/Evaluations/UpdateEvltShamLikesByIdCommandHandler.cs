using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{

    /// <summary>
    /// 更新评测官方点赞数
    /// </summary>
    public class UpdateEvltShamLikesByIdCommandHandler : IRequestHandler<UpdateEvltShamLikesByIdCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public UpdateEvltShamLikesByIdCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public Task<ResponseResult> Handle(UpdateEvltShamLikesByIdCommand request, CancellationToken cancellationToken)
        {
            string updateSql = $@" update [dbo].[Evaluation] set shamlikes=@shamlikes where id=@Id;";

            var count = _orgUnitOfWork.DbConnection.Execute(updateSql, new DynamicParameters().Set("shamlikes", request.ShamLikes).Set("Id", request.Id));
            if (count == 1)
            {               
                return Task.FromResult(ResponseResult.Success("操作成功"));
            }
            else
            {
                return Task.FromResult(ResponseResult.Failed("操作失败"));
            }
        }
    }

}
