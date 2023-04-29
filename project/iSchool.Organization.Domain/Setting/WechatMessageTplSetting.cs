using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Organization.Domain.Security.Settings
{
    public class WechatMessageTplSetting
    {
        /// <summary>
        /// 订单已发货
        /// </summary>
        public TplMsgCofig Odershipped { get; set; }

        /// <summary>
        /// 订单已发货（仅物流）
        /// </summary>
        public TplMsgCofig OderdWL { get; set; }

        /// <summary>
        /// 订单已完成
        /// </summary>
        public TplMsgCofig OrderFinished { get; set; }
        /// <summary>
        /// 订单退款
        /// </summary>
        public TplMsgCofig OrderRefund { get; set; }

        /// <summary>
        /// 种草审核通过
        /// </summary>
        public TplMsgCofig EvltRewardPass { get; set; }

        /// <summary>
        /// 种草审核不通过
        /// </summary>
        public TplMsgCofig EvltRewardUnPass { get; set; }

        /// <summary>
        /// 买课后判断是否满49元,如是,通知升级为顾问
        /// </summary>
        public TplMsgCofig CheckAndNotifyUserToDoFxlvup { get; set; }

        /// <summary>(好物)新用户立即返还</summary>
        public TplMsgCofig NewUserRewardOfBuyGoodthing { get; set; }

        /// <summary>支付成功回调发现订单已关闭而进行退款</summary>
        public TplMsgCofig RefundByOrderCancelledOnHandlePaided { get; set; }

        /// <summary>退款申请成功</summary>
        public TplMsgCofig RefundApplyOk { get; set; }
        /// <summary>退款or退货退款申请已取消</summary>
        public TplMsgCofig RefundApplyCancel { get; set; }
        /// <summary>确认收货导致退款申请取消</summary>
        public TplMsgCofig RefundApplyCancelByShipped { get; set; }
        /// <summary>退货通过后填写寄回物流时间剩余1日</summary>
        public TplMsgCofig RefundApplySendbackWL1 { get; set; }
    }

    public class TplMsgCofig
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public string tplid { get; set; }
        /// <summary>
        /// 点击模板消息跳转的地址
        /// </summary>
        public string link { get; set; }

    }
}
