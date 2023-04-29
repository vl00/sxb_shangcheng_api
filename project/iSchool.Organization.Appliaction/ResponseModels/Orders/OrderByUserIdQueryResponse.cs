using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 用户订单列表返回实体Model
    /// </summary>
    public class OrdersByUserIdQueryResponse
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid Userid { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal Payment { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 订单状态（在枚举中定义）
        /// </summary>
        public byte status { get; set; }

        /// <summary>
        /// 课程banner图片地址
        /// </summary>
        public string Banner { get; set; }

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Name { get; set; }


    }
}
