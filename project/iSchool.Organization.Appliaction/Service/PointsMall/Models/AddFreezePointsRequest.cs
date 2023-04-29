using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.PointsMall.Models
{
    public class AddFreezePointsRequest
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// 冻结积分额
        /// </summary>
        public int freezePoints { get; set; }
        /// <summary>
        /// 来源类型  
        /// 兑换 Exchange  =1,
        /// 日常任务 DayTask = 2,
        /// 运营任务 OperationTask =3,
        /// 活动 Activity = 4,
        /// 下单 Orders = 5,
        /// 订单失效OrderExpire = 6,
        ///</summary>
        public int originType { get; set; }
        /// <summary>
        /// 来源ID
        /// </summary>
        public string originId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }
}
