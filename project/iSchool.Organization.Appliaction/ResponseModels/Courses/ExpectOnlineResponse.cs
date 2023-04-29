using iSchool.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Courses
{
    /// <summary>
    /// 期待上线返回实体类
    /// </summary>
    public class ExpectOnlineResponse
    {
        /// <summary>
        /// 订阅状态（true:已订阅；false:未订阅）
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 课程短Id
        /// </summary>
        public string No { get; set; }
        //{
        //    get { return no; }
        //    set {
        //        try
        //        {
        //            no = UrlShortIdUtil.Long2Base32(Convert.ToInt64(value));
        //        }
        //        catch
        //        {
        //            no = value;
        //        }

        //    }
        //}

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Name { get; set; }

        //每次请求都需要重新获取，缓存中不存二维码
        /// <summary>
        /// 二维码url，未订阅使用
        /// </summary>
        public string QRCode { get; set; }

        /// <summary>
        /// 订阅数N
        /// </summary>
        public int Subscribe { get; set; }


        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }
    }
}
