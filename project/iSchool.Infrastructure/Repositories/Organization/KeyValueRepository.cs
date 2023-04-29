using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using System.Text;
using iSchool.Organization.Domain;
using CSRedis;
using iSchool.Organization.Domain.Modles;

namespace iSchool.Infrastructure.Repositories
{
    public class KeyValueRepository : IKeyValueReposiory
    {
       
        private OrgUnitOfWork _orgUnitOfWork { get; set; }
        CSRedisClient _redisClient;


        public KeyValueRepository( IOrgUnitOfWork orgUnitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _redisClient = redisClient;
        }
        public List<OrgSelectItemsKeyValues> GetSubjects(int type)
        {
            string key = string.Format(Consts.PreFixKey, type);
            var data = _redisClient.Get<List<OrgSelectItemsKeyValues>>(key);
            if (data != null)
            {
                return data;
            }
            else
            {
                string insertSql = $@" select [Key] as [Key] ,[name] as [Value],sort from [dbo].[KeyValue] where type=@Type order by sort ;";
                data = _orgUnitOfWork.DbConnection.Query<OrgSelectItemsKeyValues>(insertSql, new DynamicParameters().Set("Type", type)).ToList();
                if (data != null && data.Count > 0)
                {
                    _redisClient.Set(key, data);
                    return data;
                }
                else
                {
                    return new List<OrgSelectItemsKeyValues>();
                }
            }
        }

    }
}
