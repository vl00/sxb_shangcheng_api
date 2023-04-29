using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    /// <summary>
    /// 用户信息帮助类
    /// </summary>
    public class UserInfoHelper
    {
        WXOrgUnitOfWork _wXOrgUnitOfWork;

       

        public UserInfoHelper(IWXUnitOfWork wXUnitOfWork)
        {
            this._wXOrgUnitOfWork = (WXOrgUnitOfWork)wXUnitOfWork;
        }
        /// <summary>
        /// 随机产生N个用户
        /// </summary>
        /// <param name="userCount">一次取N条</param>
        /// <param name="adddays">时间间隔天数</param>
        /// <returns></returns>
        public  List<UserInfo> GetUserInfos(int userCount, int adddays)
        {
            return GetUsers(userCount,adddays);
        }
        private List<UserInfo> GetUsers(int userCount, int adddays)
        {
            List<UserInfo> userInfos = new List<UserInfo>();
            var time = DateTimeExchange.GetTandomTime(adddays);
            string sql = $@"  SELECT  top {userCount} id as UserId,nickname FROM [dbo].[userInfo] WHERE channel='1' and regTime between @stime and @etime ; ";
            var  users = _wXOrgUnitOfWork.DbConnection.Query<UserInfo>(sql, new DynamicParameters()
                .Set("stime", time.Item1)
                .Set("etime", time.Item2)
                ).ToList();
            userInfos.AddRange(users);
            while (userInfos.Count< userCount)
            {
                time = DateTimeExchange.GetTandomTime(adddays);
                 users = _wXOrgUnitOfWork.DbConnection.Query<UserInfo>(sql, new DynamicParameters()
                    .Set("stime", time.Item1)
                    .Set("etime", time.Item2)
                    .Set("userCount", userCount)
                    ).ToList();
                userInfos.AddRange(users);
            }
            return userInfos;
        }
        public class UserInfo
        {
            /// <summary>
            /// 用户Id
            /// </summary>
            public Guid UserId { get; set; }

            /// <summary>
            /// 用户名称
            /// </summary>
            public string NickName { get; set; }
        }
    }
}
