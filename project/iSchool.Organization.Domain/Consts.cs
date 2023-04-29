using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain
{
    public static class Consts
    {

        //cache rule：1.系统   2.模块   3.参数名或者功能模块   4.附带参数
        //eg. org:course:courseid:{0}  
        //eg. org:course:coursesore:{0}:page:{1}
        //eg. org:eval:total:page:{1}
        //eg. org:eval:index:page:{1}


        public const string Hushushu = "虎叔叔";

        public const string PreFixKey = "org:{0}";//

        public const string Prev_RefundCode = "RFD";

        /// <summary>
        /// 表示评测是ok
        /// </summary>
        public const int EvltOkStatus = 1;
        /// <summary>
        /// 用户中心url
        /// </summary>
        public const string BaseUrl_usercenter = "AppSettings:UserCenterBaseUrl";
        /// <summary>
        /// 上传图片url
        /// </summary>
        public const string BaseUrl_UploadUrl = "AppSettings:UploadUrl";

        /// <summary>
        /// 后台上传图片url
        /// </summary>
        public const string OrgBaseUrl_UploadUrl = "AppSettings:OrgUploadUrl";

        /// <summary>
        /// 后台发布评测上传图片url
        /// </summary>
        public const string OrgBaseUrl_evltcrawler_UploadUrl = "AppSettings:OrgeEvltCrawlerUploadUrl";

        /// <summary>活动1 guid</summary>
        public const string Activity1_Guid = "00000000-0000-0000-0000-000000000001";

        /// <summary>
        /// 微信客服消息图片素材的id-需获取上学帮公众号素材库中活动1的小助手微信二维码图片的Media_Id
        /// </summary>
        public const string Media_Id = "ih5M-3iKDsME3MNG3h5Q3qyPlAD-wYhqFUQnMiTcp30";

        /// <summary>
        /// 首页广告位入口baseurl
        /// </summary>
        public const string SpecialBaseUrl = "AppSettings:SpecialBaseUrl";

        /// <summary>
        /// 活动链接
        /// </summary>
        public const string ActivityUrl = "AppSettings:ActivityUrl";

        /// <summary>
        /// 商城分类
        /// </summary>
        public const int Kvty_MallFenlei = 16;
        /// <summary>商城分类最大层级</summary>
        public const int MallFenlei_MaxDepth = 3;


        /// <summary>
        /// errcodes
        /// </summary>
        public partial class Err
        {
            /// <summary>功能已更新,请升级小程序</summary>
            public const int AppIsUpdated = 19861018;

            #region shopping carts
            /// <summary>商品参数为空或无效</summary>
            public const int ShoppingCart_ArgumentNoGoods = 411110;
            /// <summary>商品不在购物车中</summary>
            public const int ShoppingCart_NotInCart = 411111;

            /// <summary>加入购物车时发现价格变动</summary>
            public const int ShoppingCart_PriceChanged = 444003;
            /// <summary>老用户不能添加新人立享</summary>
            public const int ShoppingCart_NewUserExclusiveAndOldUser = 411113;
            /// <summary>加入购物车时发现没库存了</summary>
            public const int ShoppingCart_NoStock = 444004;
            /// <summary>只能加入1个到购物车（网课 新人专享）</summary>
            public const int ShoppingCart_OnlyCanBuy1 = 411114;

            #endregion shopping carts

            #region create order

            /// <summary>已支付</summary>
            public const int PaidBefore = 444001;
            /// <summary>课程(spu)已下架</summary>
            public const int CourseOffline = 44444440;
            /// <summary>价格已改变</summary>
            public const int PriceChanged = 444003;
            /// <summary>没库存了(库存为0)</summary>
            public const int NoStock = 444004;
            /// <summary>库存不足</summary>
            public const int StockNotEnough = 444018;
            /// <summary>商品已下架(数据库根本就没该商品id)</summary>
            public const int CourseGoodsOffline = 44444445;
            /// <summary>商品已下架</summary>
            public const int CourseGoodsIsOffline = 44444444;
            /// <summary>存在无效的孩子信息</summary>
            public const int OrderCreate_ChildrenInfoIds_NotMatch = 444006;
            /// <summary>超过限购数量--本次购买</summary>
            public const int OrderCreate_LimitedBuyNum1 = 444007;
            /// <summary>超过限购数量--本次购买+历史(sku)</summary>
            public const int OrderCreate_LimitedBuyNum2 = 444008;
            /// <summary>参数错误,多个相同商品id</summary>
            public const int OrderCreate_MultiSameGoods = 444009;
            /// <summary>多个网课不能一起结算</summary>
            public const int OrderCreate_MultiCourse1 = 444010;
            /// <summary>网课不能跟好物一起结算</summary>
            public const int OrderCreate_OnlyCanBuyCourse1 = 444011;
            /// <summary>只能购买一个商品（网课 新人专享）</summary>
            public const int OrderCreate_OnlyCanBuy1 = 444012;
            /// <summary>参数错误,参数存在没商品的品牌id</summary>
            public const int OrderCreate_OrgidHasNosku = 444013;
            /// <summary>老用户不能购买新人立享</summary>
            public const int OrderCreate_NewUserExclusiveAndOldUser = 444014;
            /// <summary>新用户的未支付的新人专享订单不能多单</summary>
            public const int OrderCreate_NewUserExclusiveNotAllowMuitlUnpaidOrder = 444015;
            /// <summary>(重新支付时)发现新用户的未支付的新人专享订单读取cache失败</summary>
            public const int OrderCreate_NewUserExclusiveNotInCache = 444016;
            /// <summary>运费已改变</summary>
            public const int FreightChanged = 444017;
            /// <summary>商品运费地区在不发货地区里</summary>
            public const int FreightArea_of_sku_is_in_blacklist = 444019;
            /// <summary>超过限购数量--本次购买+历史(spu)</summary>
            public const int OrderCreate_LimitedBuyNum2_spu = 444018;

            /// <summary>创建订单失败</summary>
            public const int OrderCreateFailed = 444101;
            /// <summary>call预支付接口失败</summary>
            public const int CallPaidApiError = 444102;
            /// <summary>创建轮询缓存</summary>
            public const int PollError = 444103;
            /// <summary>call检查订单状态接口失败</summary>
            public const int CallCheckPaystatusError = 444104;
            /// <summary>更新之前的订单失败</summary>
            public const int PrevOrderUpdateFailed = 444105;
            /// <summary>限购获取锁失败</summary>
            public const int OrderCreate_LimitedBuy_LockFailed = 444106;
            /// <summary>判断新用户获取锁失败</summary>
            public const int OrderCreate_NewUserBuy_LockFailed = 444107;
            /// <summary>使用优惠券失败</summary>
            public const int OrderUseCouponError = 444108;
            /// <summary>
            /// 使用积分兑换失败
            /// </summary>
            public const int OrderUsePointsPayError = 444109;

            /// <summary>无积分配置</summary>
            public const int OrderCreate_CourseExchangeIsNull = 444200;
            /// <summary>积分配置未生效</summary>
            public const int OrderCreate_CourseExchangeNotStarted = 444201;
            /// <summary>积分配置已失效</summary>
            public const int OrderCreate_CourseExchangeIsEnded = 444202;
            /// <summary>不是RwInviteActivity</summary>
            public const int OrderCreate_CourseExchangeIsNotRwInviteActivity = 444203;
            /// <summary>用户没UnionID</summary>
            public const int OrderCreate_UserHasNoUnionID = 444204;
            /// <summary>用户没资格购买该商品(没积分)</summary>
            public const int OrderCreate_UserHasNoScoreToBuy = 444205;
            /// <summary>rw活动暂时不支持与其他商品一起结算</summary>
            public const int OrderCreate_RwInviteActivity_Only1sku = 444206;
            /// <summary>下单失败后归还积分也失败</summary>
            public const int OrderCreate_RwInviteActivity_ErrOnRollBack = 444207;
            /// <summary>订单取消后归还积分失败</summary>
            public const int OrderCancel_RwInviteActivity_ErrOnRollBack = 444208;

            #endregion create order
        }

        public partial class Err
        {
            /// <summary>空的参数</summary>
            public const int Selsku_EmptyPropItemIds = 431111;
            /// <summary>多个课程商品</summary>
            public const int Selsku_MultGoods = 431112;
            /// <summary>课程参数不一致</summary>
            public const int Selsku_CourseNotSame = 431113;
            /// <summary>没结果</summary>
            public const int Selsku_NoResult = 431114;
            /// <summary>不存在</summary>
            public const int Selsku_NotExists = 431115;
            /// <summary>商品属性项与参数个数不一致</summary>
            public const int Selsku_PropItemCountNotSame = 431116;
            /// <summary>商品属性项与参数不一致</summary>
            public const int Selsku_PropItemNotSame = 431117;

            /// <summary>支付成功后的用读连接查询订单发现不同步</summary>
            public const int OrderPayedOk_ReadWriteNotSync = 445230;
            /// <summary>购买后自动发送兑换码-订单状态变成待收货</summary>
            public const int OrderPayedOk_Autosendcode_Upstatus = 445232;
            /// <summary>购买后自动发送兑换码-调用后台发货失败</summary>
            public const int OrderPayedOk_Autosendcode_Failed = 445233;

            /// <summary>不要重复点击确定收货</summary>
            public const int ShippedOrderTooManyTimes = 441300;
            /// <summary>确定收货-订单不存在</summary>
            public const int OrderIsNotValid_OnShipped = 441301;
            /// <summary>确定收货-订单当前状态不是待收货</summary>
            public const int OrderStatus_IsNot_Shipping = 441302;
            /// <summary>确定收货失败-订单当前状态不是待收货</summary>
            public const int ShippedOrderFailed_Status0_noteq_Shipping = 441303;
            /// <summary>确定收货失败-订单是别人的</summary>
            public const int ShippedOrderFailed_UserNotSame = 441304;
            /// <summary>自动确定收货-发送微信通知失败</summary>
            public const int AutoShippedOrder_send_wx_Error = 441305;
            /// <summary>确定收货失败-取消之前的退款申请失败</summary>
            public const int ShippedOrderFailed_CancelRefundError = 441306;
            /// <summary>自动确定收货-失败</summary>
            public const int AutoShippedOrder_error = 441307;

            #region 快递
            /// <summary>获取锁失败</summary>
            public const int Kuaidi_LckFailed = 241111;
            /// <summary>没有可用的百度快递bdtk</summary>
            public const int Kuaidi_NoBaiduTk = 241112;
            /// <summary>获取api结果后写入db失败</summary>
            public const int Kuaidi_ErrOnSyncToDb = 241113;
            /// <summary>有2个Nu</summary>
            public const int Kuaidi_Has2Nu = 241114;
            /// <summary>未知其他错误</summary>
            public const int Kuaidi_OtherError = 249999;
            /// <summary>传入的快递公司编码无效</summary>
            public const int Kuaidi_ComArgsNotmatch = 242115;
            /// <summary>腾讯云-17972Api,此快递需要额外参数(手机后4位)</summary>
            public const int Kuaidi_Txc17972_MissCustomerArgs = 242116;
            /// <summary>WriteToDB时发现接口返回的kdccode不在已知codes中</summary>
            public const int Kuaidi_ComNotmatch_When_WriteToDB = 241115;
            /// <summary>WriteToDB时有多个线程写同一单导致的系统繁忙</summary>
            public const int Kuaidi_BusyForWriteToDB = 241116;
            /// <summary>腾讯云-17972Api,被计数限制</summary>
            public const int Kuaidi_Txc17972_Limited = 241117;
            /// <summary>第3方api,被计数限制</summary>
            public const int Kuaidi_Api3th_Limited = 241117;

            /// <summary>快递详情-订单不可用</summary>
            public const int Kuaidi_OrderIsNotValid = 241001;
            #endregion 快递
        }

        public partial class Err
        {
            #region refund
            /// <summary>获取锁失败</summary>
            public const int RefundApplyCheck_CannotGetLck = 511110;
            /// <summary>数据错误?没支付时间</summary>
            public const int RefundApplyCheck_NoPaytime = 511111;
            /// <summary>申请过多</summary>
            public const int RefundApplyCheck_ApplyCountIsOver = 511112;
            /// <summary>无法判断退款类型</summary>
            public const int RefundApplyCheck_NotFound_RefundType = 511113;

            /// <summary>0元订单不支持退款</summary>
            public const int RefundApply_MoneyIsZero = 512110;
            /// <summary>提交数量已超过当前可申请退款数量</summary>
            public const int RefundApply_OverCount = 512111;
            /// <summary>非法操作,申请退款类型错误</summary>
            public const int RefundApply_TypeError = 512112;
            /// <summary>极速退款申请失败, 已超过支付时间后30分钟</summary>
            public const int RefundApply_CannotFastRefund_30minTimeout = 512113;
            /// <summary>极速退款申请失败, status错误, 可能是商品已出库</summary>
            public const int RefundApply_CannotFastRefund_StatusError = 512114;
            /// <summary>申请类型与检查到的类型不一样</summary>
            public const int RefundApply_TypeNotSame = 512115;
            /// <summary>申请(含极速)退款写入db出错</summary>
            public const int RefundApply_WriteDbError = 512116;
            /// <summary>本次极速退款之前不可能是退款退货...</summary>
            public const int RefundApply_FastRefund_HasOtherApplyBefore = 512117;
            /// <summary>极速退款调用退款接口失败</summary>
            public const int RefundApply_FastRefund_CallApiError = 512118;

            /// <summary>非本人操作取消申请退款</summary>
            public const int RefundCancel_NotSameUser = 521111;
            /// <summary>非本人操作取消申请退款</summary>
            public const int RefundSendback_WriteKd2DbError = 521112;
            /// <summary>没第一步的时间</summary>
            public const int RefundSendback_NoStepOneTime = 521113;
            /// <summary>退货填写物流已超时申请已取消</summary>
            public const int RefundSendback_Timeout = 521114;
            /// <summary>积分支付订单不支持退款</summary>
            public const int RefundApply_PointsPay = 512115;
            #endregion refund
        }
    }
}
