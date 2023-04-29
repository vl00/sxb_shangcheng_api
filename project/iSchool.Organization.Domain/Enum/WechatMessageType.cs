using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    public enum WechatMessageType
    {

        /// <summary>
        /// 
        /// </summary>
        订单已发货 = 1,
        订单已完成 = 2,
        订单退款 = 3,
        种草审核通过 = 4,
        种草审核不通过 = 5,
        /// <summary>
        /// 
        /// </summary>
        物流 = 6,
        部分发货 = 7,
        全部发货 = 8,
        /// <summary>买课后判断是否满49元,如是,通知升级为顾问</summary>
        升级顾问通知 = 20,

        好物新人立返佣金 = 26,

        支付成功回调发现订单已关闭而进行退款 = 28,

        成功发起退货or退款申请时 = 29,
        退款or退货退款申请已取消 = 30,
        确认收货导致退款申请取消 = 31,
        填写退货物流信息即将超时 = 32,
    }
}
