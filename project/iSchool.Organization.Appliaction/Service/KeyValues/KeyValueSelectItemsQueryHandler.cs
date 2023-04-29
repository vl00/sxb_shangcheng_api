using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.KeyValues
{
    /// <summary>
    /// 科目分类
    /// </summary>
    public class KeyValueSelectItemsQueryHandler : IRequestHandler<KeyValueSelectItemsQuery, ResponseResult>
    {
        
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
     
        public KeyValueSelectItemsQueryHandler(IOrgUnitOfWork orgUnitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _redisClient = redisClient;
        }

        public Task<ResponseResult> Handle(KeyValueSelectItemsQuery request, CancellationToken cancellationToken)
        {
            string key = string.Format(CacheKeys.selectItems, request.Type);
            var data = _redisClient.Get<List<SelectItemsKeyValues>>(key);
            if (data != null)
            {
                return Task.FromResult(ResponseResult.Success(data));
            }
            else
            {
                if (request.Type == Consts.Kvty_MallFenlei)
                {
                    string insertSql = $@" select [Key] as [Key] ,[name] as [Value],sort,attach from [dbo].[KeyValue] where IsValid=1 and depth=1 and type=@Type order by sort ;";
                    data = _orgUnitOfWork.DbConnection.Query<SelectItemsKeyValues>(insertSql, new DynamicParameters().Set("Type", request.Type)).ToList();
                    if (data != null && data.Count > 0)
                    {
                        _redisClient.Set(key, data);
                        return Task.FromResult(ResponseResult.Success(data));
                    }
                    else
                    {
                        return Task.FromResult(ResponseResult.Failed("暂无数据"));
                    }
                }
                else {
                    string insertSql = $@" select [Key] as [Key] ,[name] as [Value],sort,attach from [dbo].[KeyValue] where IsValid=1 and type=@Type order by sort ;";
                    data = _orgUnitOfWork.DbConnection.Query<SelectItemsKeyValues>(insertSql, new DynamicParameters().Set("Type", request.Type)).ToList();
                    if (data != null && data.Count > 0)
                    {
                        _redisClient.Set(key, data);
                        return Task.FromResult(ResponseResult.Success(data));
                    }
                    else
                    {
                        return Task.FromResult(ResponseResult.Failed("暂无数据"));
                    }
                }

               
            }
        }
    }
}
