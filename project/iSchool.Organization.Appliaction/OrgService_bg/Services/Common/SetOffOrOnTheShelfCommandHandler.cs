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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 后台通用status变更
    /// </summary>
    public class SetOffOrOnTheShelfCommandHandler:IRequestHandler<SetOffOrOnTheShelfCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public SetOffOrOnTheShelfCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public Task<ResponseResult> Handle(SetOffOrOnTheShelfCommand request, CancellationToken cancellationToken)
        {
            string updateSql = $" UPDATE  [dbo].[{request.TableName}] set status=@status,ModifyDateTime=@time,Modifier=@userId  where id=@id and IsValid=1;";
            try
            {
                var count = _orgUnitOfWork.DbConnection.Execute(updateSql, new DynamicParameters()
                .Set("status", request.Status)
                .Set("id", request.Id)
                .Set("time", DateTime.Now)
                .Set("userId", request.UserId));
                if (count == 1)//清除API那边相关的缓存
                {
                    if (request.BatchDelCache?.Any() == true)
                    {
                        _redisClient.BatchDelAsync( request.BatchDelCache,10);
                    }
                    return Task.FromResult(ResponseResult.Success("操作成功"));
                }
                else
                {
                    return Task.FromResult(ResponseResult.Failed("操作失败"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(ResponseResult.Failed($"系统错误：【{ex.Message}】"));
            }
            
        }
    }
}
