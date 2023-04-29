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
    /// 更新单字段后台通用
    /// </summary>
    public class ChangeSingleFieldCommandHandler:IRequestHandler<ChangeSingleFieldCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public ChangeSingleFieldCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(ChangeSingleFieldCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string updateSql = $" UPDATE  [dbo].[{request.TableName}] set {request.FieldName}=@FieldValue,ModifyDateTime=@time,Modifier=@userId  where id=@id and IsValid=1;";
            try
            {
                var count = _orgUnitOfWork.DbConnection.Execute(updateSql, new DynamicParameters()
                .Set("FieldValue", request.FieldValue)
                .Set("id", request.Id)
                .Set("time", DateTime.Now)
                .Set("userId", request.UserId));
                if (count == 1)//清除API那边相关的缓存
                {
                    if (request.BatchDelCache?.Any() == true)
                    {
                        await _redisClient.BatchDelAsync(request.BatchDelCache,10);
                    }
                    return ResponseResult.Success("操作成功");
                }
                else
                {
                    return ResponseResult.Failed("操作失败");
                }
            }
            catch(Exception ex)
            {
                return ResponseResult.Failed($"系统错误：【{ex.Message}】");
            }
            
        }
    }
}
