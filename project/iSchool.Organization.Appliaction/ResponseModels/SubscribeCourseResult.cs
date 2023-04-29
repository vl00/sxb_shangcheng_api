using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 订阅后返回实体
    /// </summary>
    public class SubscribeCourseResult
    {
        /// <summary>
        /// 机构品牌名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 订阅数N（89+1.2*N）
        /// </summary>
        private int subscribe;

        /// <summary>
        /// 订阅数
        /// </summary>
        public int Subscribe
        {
            get
            {
                return subscribe;
            }

            set
            {
                subscribe = (int)Math.Ceiling(89 + 1.2 * value);
            }
        }
    }
}
