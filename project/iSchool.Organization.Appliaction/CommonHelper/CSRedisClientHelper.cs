using CSRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    /// <summary>
    /// 缓存助手
    /// </summary>
    public static class CSRedisClientHelper
    {
        /// <summary>
        /// 批量删除缓存
        /// </summary>
        /// <param name="redisClient"></param>
        /// <param name="keys"></param>
        public static void BatchDel(this CSRedisClient redisClient, IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                redisClient.Del(redisClient.Keys(key));
            }            
        }

       
        
    }
}
