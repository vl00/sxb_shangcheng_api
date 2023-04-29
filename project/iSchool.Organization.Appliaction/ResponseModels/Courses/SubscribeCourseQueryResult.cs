using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Courses
{
    /// <summary>
    /// 期待上线-返回实体类
    /// </summary>
    public class SubscribeCourseQueryResult
    {
        /// <summary>
        /// 订阅数N（89+1.2*N）
        /// </summary>
        private int subscribe;

        /// <summary>
        /// 订阅数N
        /// </summary>
        public int Subscribe 
        {
            get 
            {
                return subscribe;
            }

            set 
            {
                subscribe = (int)Math.Ceiling(89 + 1.2 * value) ;
            }
        }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }
    }
}
