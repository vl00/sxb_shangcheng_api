using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain
{
    public static partial class CacheKeys
    {
        /// <summary>
        /// 不同根级的所有板块
        /// </summary>
        public const string CatogoryItemsForDiffrentRootLevel = "org:catogoryItems:rootlevel:{0}";
       
        /// <summary>
        /// 手机验证码
        /// </summary>
        public const string VerifyCodeKey = "RNDCode-86{0}-{1}";
        //cache rule：1.系统   2.模块   3.参数名或者功能模块   4.附带参数
        //eg. org:course:courseid:{0}  
        //eg. org:course:index:sore:{0}:page:{1}
        //eg. org:eval:total
        //eg. org:eval:index:page:{1}

        /// <summary>
        /// 用户基本信息
        /// </summary>
        public const string UserSimpleInfo = "org:user:simple_{0}";

        /// <summary>评测首页分页总数</summary>
        public const string RdK_evltsTotal = "org:evltsMain:total:subj_{0}age_{1}stick_{2}";
        /// <summary>评测首页缓存</summary>
        public const string RdK_evlts = "org:evltsMain:subj_{0}age_{1}stick_{2}";
        /// <summary>del评测首页缓存</summary>
        public const string Del_evltMain = "org:evltsMain:*";

        /// <summary>
        /// 一个(小)专题页里评测ls分页总数
        /// </summary>
        public const string Rdk_spclLsTotal = "org:spcl:id_{0}:total:orderby_{1}";
        /// <summary>
        /// 一个(小)专题页里评测ls首页缓存
        /// </summary>
        public const string Rdk_spclLs = "org:spcl:id_{0}:orderby_{1}";

        /// <summary>
        /// 一个(大)专题页里评测ls分页总数
        /// </summary>
        public const string Rdk_big_spclLsTotal = "org:spcl:id_{0}:total:orderby_{1}:smalid_{2}";
        /// <summary>
        /// 一个(大)专题页里评测ls首页缓存
        /// </summary>
        public const string Rdk_big_spclLs = "org:spcl:id_{0}:orderby_{1}:smalid_{2}";


        /// <summary>
        /// 一个专题页专题信息
        /// </summary>
        public const string Rdk_spcl = "org:spcl:id_{0}";

        /// <summary>
        /// 一个大专题的小专题集合
        /// </summary>
        public const string Rdk_Big_spcl = "org:spcl:id_{0}:bigspclid";

        public const string SpclNo = "org:spclNo:{0}";

        /// <summary>
        /// 用于根据评测短id查真id
        /// </summary>
        public const string EvltNo = "org:evltNo:{0}";
        /// <summary>
        /// 评测详情基本info
        /// </summary>
        public const string Evlt = "org:evlt:info:{0}";
        /// <summary>
        /// 评测评论详情基本info
        /// </summary>
        public const string EvltCommnet = "org:evlt:comment:info:{0}";
        /// <summary>
        /// 评测投票
        /// </summary>
        public const string EvltVote = "org:evlt:vote:voteid_{0}";
        /// <summary>
        /// 我在某评测下的投票
        /// </summary>
        public const string MyEvltVote = "org:evlt:byme_vote:userid_{userid}:evlid_{evltId}";
        /// <summary>
        /// 投票中
        /// </summary>
        public const string IamVoting = "org:tmp:uservote:uid_{userid}:vote_{voteId}";

        /// <summary>评测评论前20条</summary>
        public const string EvltCommentTop20 = "org:evlt:comment:top20:evlt_{0}";
        /// <summary>评测评论前n条</summary>
        public const string EvltCommentTopN = "org:evlt:comment:top{0}:evlt_{1}";
        /// <summary>del评测评论前n条</summary>
        public const string Del_EvltCommentTopN = "org:evlt:comment:top*";
        /// <summary>
        /// 评论详情页回复前10条
        /// </summary>
        public const string EvltCommentChildrendCommentTop10 = "org:evlt:childrencomment:top10:evltcmt_{0}";
        /// <summary>
        /// 用户发评测评论时间间隔 [ 0:userid ] 3s
        /// </summary>
        public const string Tdiff4UserAddEvltComment = "org:userAddEvltComment_tdiff:uid_{0}";

        public const string UV_evlt_user = "org:uv:date_{date}:u_{userid}:evlt_{evltid}";
        public const string UV_evlt_total = "org:uv:date_{date}:total:evlt_{evltid}";
        public const string UV_evlt_diff = "org:uv:date_{date}:diff:evlt_{evltid}";

        public const string PV_total = "org:pv:date_{date}:total:{ctt}_{cttid}";
        public const string PV_diff = "org:pv:diff:{ctt}_{cttid}";
        public const string PV_diffAll = "org:pv:diff:*";

        /// <summary>专题列表</summary>
        public const string simplespecial = "org:special:simple";
        /// <summary>专题列表</summary>
        public const string simplespecial_acd = "org:special:simple:acd_{0}";

        /// <summary>
        /// 评测简单模型
        /// </summary>
        public readonly static string simpleevaluation = "org:evaluation:simple:id:{0}";

        #region 课程
        /// <summary>
        /// 用户购买过课程
        /// </summary>
        public const string UeserCourseBuyExist = "org:course:coursebuy:courseid:{0}:phonenumber:{1}";
        /// <summary>
        /// 登录用户收藏的某课程
        /// </summary>
        public const string MyCollectionCourse = "org:course:byme_collection:userid_{0}:courseid_{1}";

        /// <summary>
        /// 课程详情
        /// </summary>
        public const string CourseDetails = "org:course:courseid:{0}";

        /// <summary>
        /// 课程订阅数
        /// </summary>
        public const string CourseSubscribeCount = "org:course:subscribecount:courseid:{0}";

        /// <summary>
        /// 用户订阅课程状态与公众号相关
        /// </summary>
        public const string SubscribeStatus = "org:user:{0}:courseid:{1}";

        /// <summary>
        /// 期待上线公众号回调
        /// </summary>
        public const string gzhbackinfo = "org:course:ticket:courseid:{0}";
        /// <summary>
        /// 购买课程公众号回调
        /// </summary>
        public const string gzhbackinfo_coursebuy = "org:course:ticket:buycourse:courseid:{0}:identity:{1}";
        /// <summary>
        /// 课程中心--pageIndex&pageSize,  subjectId,  ageGroupId
        /// </summary>
        public const string CourseCenter = "org:courses:subid:{0}:ageid:{1}:page:{2}:auth:{3}";
        /// <summary>
        /// 小程序课程中心--pageIndex&pageSize,  subjectId,  ageGroupId
        /// </summary>
        public const string MiniCourseCenter = "org:courses:xcxmp:subid:{0}:ageid:{1}:page:{2}:sort:{3}:type{4}:ct{5}:gtt{6}";

        /// <summary>
        /// 小程序课程中心--pageIndex&pageSize,  subjectId,  ageGroupId
        /// </summary>
        public const string MiniCourseCenterV1 = "org:courses:xcxmp:catogry:{0}:ageid:{1}:page:{2}:sort:{3}:ct{4}:ps:{5}:pe{6}";
        /// <summary>
        /// 小程序好物中心--pageIndex&pageSize,
        /// </summary>
        public const string MiniGoodThingCenter = "org:courses:xcxmp:goodthing:type:{0}:page:{1}";

        /// <summary>
        ///限时低价体验
        /// </summary>
        public const string LowPriceRecomend = "org:courses:xcxmp:goodthing:lowprice:recomend";

        ///// <summary>
        ///// 课程中心-总条数--，pageIndex, pageSize，TotalCount
        ///// </summary>
        //public const string CourseCenterPageInfo = "org:courses:totalcount:subid:{0}:ageid:{1}";

        /// <summary>
        /// 机构--相关课程 page=pageindex&pagesize
        /// </summary>
        public const string CoursesByOrg = "org:courses:org:{0}:page{1}:useagent{2}";

        /// <summary>
        /// 选择品牌品牌--相关课程 page=pageindex&pagesize
        /// </summary>
        public const string BrandCoursesByOrg = "org:courses:org:{0}:page{1}:brand";

        /// <summary>
        /// 课程列表缓存--批量清除
        /// </summary>
        public const string Del_Courses = "org:courses:{0}";

        /// <summary>
        /// 获取课程长Id（Guid） no-短Id
        /// </summary>
        public const string courseidbyno = "org:course:no:{0}";

        /// <summary>某个课程的基本信息</summary>
        public const string CourseBaseInfo = "org:course:courseid:{0}:info";

        /// <summary>
        /// 精品课程
        /// </summary>
        public const string ExcellentCourses = "org:course:excellentcourses";

        /// <summary>
        /// 课程扩展字段（购前须知，大家种草，标签）
        /// </summary>
        public const string CourseExtend = "org:course:extend:{0}";

        #endregion

        #region 机构
        /// <summary>
        /// 机构大全 page=pageindex&pagesize
        /// </summary>
        public const string OrgList = "org:organizations:coName:{0}:subid:{1}:authentication{2}:page{3}";
        /// <summary>
        /// 不同商品分类的机构
        /// </summary>
        public const string CatogoryOrgList = "org:organizations:catogry:{0}:authentication{1}:page{2}";
        /// <summary>
        /// 评测品牌-根据品牌查询机构列表
        /// </summary>
        public const string OrgByNameList = "org:organizations:orgname:{0}:page:{1}";

        /// <summary>
        /// 机构详情 
        /// </summary>
        public const string OrgDetails = "org:organization:orgid:{0}";

        /// <summary>
        /// 机构详情--相关评测
        /// </summary>
        public const string OrgRelatedEvaluation = "org:organization:evlts:orgid:{0}";

        /// <summary>
        ///  机构详情--相关评测--评测分页总数
        /// </summary>
        public const string OrgRelatedEvaluation_Total = "org:organization:evlts:total:{0}";

        /// <summary>
        /// 机构列表缓存--批量清除
        /// </summary>
        public const string Del_Organizations = "org:organizations:{0}";

        /// <summary>
        /// 用于获取机构长id
        /// </summary>
        public const string orgidbyno = "org:organization:no:{0}";

        /// <summary>某个机构的基本信息</summary>
        public const string OrgzBaseInfo = "org:organization:orgz:{0}:info";

        #endregion

        #region SelectItems api 下拉列表api
        public const string selectItems = "org:selectItems:{0}";
        #endregion

        #region 订阅表

        /// <summary>
        /// 订阅列表--Subscribe
        /// </summary>
        public const string SubscribeList = "org:subscribes:contion:{0}:page:{1}";


        #endregion

        #region 点赞

        /// <summary>
        /// 我的评测点赞  hash   field: evalid
        /// </summary>
        public const string MyEvaluationLikes = "org:evlt_byme_like:userid_{0}";


        //评测中点赞数  hash  field【 collect  like  viewer 】
        public const string EvaluationLikesCount = "org:evlt_statistics:eval_{0}";


        //我的评论点赞列表   hash  {(user evid)  commentid   1}
        public const string MyCommentLikes = "org:evltcomment_byme_like:userid_{0}:evlid_{1}";


        //评测评论点赞数（evid） commentid  [count]
        public const string EvaluationCommentLikesCount = "org:evltcomment_like:evlid_{0}";


        //【定时任务】  评测点赞行为
        public const string EvaluationLikeAction = "org:evallike:action";

        //【定时任务】  评论点赞行为
        public const string CommentLikeAction = "org:evalcommentlike:action";


        #endregion


        /// <summary>活动simple info</summary>
        public const string ActivitySimpleInfo = "org:hd_sinfo:id_{0}";
        /// <summary>活动号 to id</summary>
        public const string Acd_id = "org:acd_id:{0}";
        /// <summary>根据专题id查找对应的活动s</summary>
        public const string Hd_spcl_acti = "org:spcl:id_{0}:activitys";
        /// <summary>活动审核通过后几日内评测不能编辑</summary>
        public const string Editdisable_evlt = "org:editdisable:evlt:id_{0}";

        /// <summary>生成微信小程序二维码</summary>
        public const string CreateMpQrcode = "org:mpqrcode:{0}";

        /// <summary>wx分段上传接口的每块最大大小，默认20m</summary>
        public const string WxUploadBlockSize = "org:wxupload_blocksize";

        #region 兑换码
        /// <summary>根据订单Id,获取预分配的兑换码，发送成功则删除该缓存</summary>
        public const string notUsedSingleCode = "org:redeemcode:courseid_{0}:orderid_{1}";

        /// <summary>兑换码被占用了</summary>
        public const string CodeIsLock = "org:redeemcode:courseid_{0}:code_{1}";
        #endregion

        #region 用户课程浏览记录
        //persist 这个不能被模糊匹配删除
        public const string CourseVisitLog = "persist:org:coursevisitlog:userid_{0}";
        #endregion

        /// <summary>
        /// mp首页运营专区4大项-限时优惠s
        /// </summary>
        public const string MpMallOperateArea_LimitedTimeOffers = "org:courses:MpMallOperateArea:LimitedTimeOffers";
        /// <summary>
        /// mp首页运营专区4大项-新人专享s
        /// </summary>
        public const string MpMallOperateArea_NewUserExclusives = "org:courses:MpMallOperateArea:NewUserExclusives";
        /// <summary>
        /// mp首页运营专区4大项-热销榜单s
        /// </summary>
        public const string MpMallOperateArea_HotSells = "org:courses:MpMallOperateArea:HotSells";
        /// <summary>
        /// mp首页运营专区4大项-本周上新s
        /// </summary>
        public const string MpMallOperateArea_NewOnWeeks = "org:courses:MpMallOperateArea:NewOnWeeks";
    }

    #region 活动1
    public static partial class CacheKeys
    {
        /// <summary>首页</summary>
        public const string Hd1_main = "org:hd1:main";
        /// <summary>优秀案例</summary>
        public const string Hd1_excc = "org:hd1:excc";
        /// <summary>活动1用户最新的添加评测</summary>
        public const string Hd1_UserLastestEvltAdded = "org:hd1:user_lastest_evlt_added:me_{0}";
        /// <summary>活动1用户添加评测后放参数入redis的key</summary>
        public const string Hd1_UserEvltAddedArgs = "org:hd1:evlt_added_args:evlt_{0}";
        /// <summary>活动1用户评测点赞排名数据</summary>
        public const string Hd1_UserEvltLikeRankData = "org:hd1:userelvtlikerankdata:uid_{0}";

        /// <summary>分销活动01扫公众号码自动回复图片 </summary>
        public const string Hddrpfx01_gzhhf_pic = "org:hddrpfx01:gzhhf_pic:{0}";
    }
    #endregion 活动1

    #region rw邀请活动 顾问微信群拉粉丝
    public static partial class CacheKeys
    {
        /// <summary>给定的隐形上架的商品s</summary>
        public const string RwInviteActivity_InvisibleOnlineCourses = "org:hdrw_InvisibleOnlineCourses";
        /// <summary>(城市)给定的隐形上架的商品s</summary>
        public const string RwInviteActivity_InvisibleOnlineCoursesWithCity = "org:hdrw_InvisibleOnlineCourses:city{0}";

        /// <summary>发展人购买资格积分------>推广积分制</summary>
        public const string RwInviteActivity_InviterBonusPoint = "org:InviteActivity:InviterBonusPoint";
        /// <summary>被发展人购买资格------>付费机会制</summary>
        public const string RwInviteActivity_InviteeBuyQualify = "org:InviteActivity:InviteeBuyQualify";
    }
    #endregion

    #region PC
    public static partial class CacheKeys
    {
        /// <summary>pc评测首页</summary>
        public const string PC_EvltsMain = "org:evltsMain:pc:1p{0}:subj_{1}|age_{2}|stick_{3}";
        public const string PC_EvltsMain2 = "org:evltsMain:pc:1p{0}:subj_{1}|age_{2}|stick_{3}|orgid_{4}";
        /// <summary>pc评测首页科目栏(需要只显示机构有评测的科目)</summary>
        public const string PC_EvltsMain_subjs = "org:evltsMain:pc:subjs:orgid_{0}";
        /// <summary>pc主题ls页</summary>
        public const string PC_SpclLs = "org:spcl:id_{0}:pc:1p{1}:orderby_{2}";
        /// <summary>pc课程列表页</summary>
        public const string PC_CourseIndexLs = "org:courses:pc:1p{0}:subj_{1}|auth_{2}|orgid_{3}";
        /// <summary>pc机构列表页</summary>
        public const string PC_OrgIndexLs = "org:organizations:pc:1p{0}:type_{1}|auth_{2}";

        /// <summary>pc评测详情页-相关评测s</summary>
        public const string PC_EvltRelateds = "org:pc:relatedEvlts:evlt_{0}";
        /// <summary>pc课程详情页-相关评测s</summary>
        public const string PC_CourseRelatedEvlts = "org:pc:relatedEvlts:course_{0}:subj_{1}";
        /// <summary>pc机构详情-相关评测s</summary>
        public const string PC_OrgRelatedEvlts = "org:pc:relatedEvlts:org_{0}";

        /// <summary>pc课程详情-机构(相关)课程s</summary>
        public const string PC_CourseRelatedCourses = "org:course:courseid:{0}:pc:relatedcourses";
        /// <summary>pc机构详情-机构(相关)课程s</summary>
        public const string PC_OrgRelatedCourses = "org:organization:orgid:{0}:pc:relatedcourses";

        /// <summary>pc评测详情作者信息统计数s</summary>
        public const string PC_EvltAuthorCounts = "org:evlt:info:{evltId}:pc:author_{userid}:counts";

        /// <summary>pc机构信息统计数-课程数</summary>
        public const string PC_OrgCounts_Course = "org:organization:orgid:{0}:pc:counts:cource";
        /// <summary>pc机构信息统计数-评测数</summary>
        public const string PC_OrgCounts_Evlt = "org:organization:orgid:{0}:pc:counts:evlt";
        /// <summary>机构信息统计数-商品数</summary>
        public const string Mp_OrgCounts_Goods = "org:organization:orgid:{0}:mp:counts:goods";
    }
    #endregion

    #region 订单
    public static partial class CacheKeys
    {
        [Obsolete] public const string HLL4norepear = "org:HLL4norepear";
        public const string PayNorepear = "org:paynorepear:{0}";

        /// <summary>课程库存</summary>
        [Obsolete] public const string CourseStock = "org:course_stock2:courseid:{0}";
        /// <summary>课程库存s</summary>
        [Obsolete] public const string CourseStocks = "org:course_stock2:courseid:*";

        /// <summary>用于轮询-微信支付</summary>
        public const string OrderPoll_wxpay_order = "org:poll:{0}"; // {0}=OrderId.ToString("n")    

        /// <summary>课程商品库存</summary>
        public const string CourseGoodsStock = "org:course_goods_stock:{0}";
        /// <summary>课程商品与属性分组与属性项的关系sm表</summary>
        public const string CourseGoodsProps = "org:course:courseid:{0}:goods_props";
        /// <summary>由属性项查找课程商品id</summary>
        public const string CourseGoodsPropItemsSha1 = "org:course_goods_propitems_sha1:{0}";
        /// <summary>课程商品simple info</summary>
        public const string CourseGoodsInfo = "org:course_goods:goodsid_{0}";

        /// <summary>课程分销信息</summary>
        public const string CourseDrpfxInfo = "org:course:courseid:{0}:drpfxinfo";
        /// <summary>课程积分信息</summary>
        public const string CourseIntegrlInfo = "org:course:courseid:{0}:integralinfo";
        /// <summary>sku分销信息</summary>
        public const string SkuDrpfxInfo = "org:course:skuid:{0}:drpfxinfo";

        /// <summary>lck for 新用户</summary>
        public const string OrderCreate_NewUserExclusive = "org:lck2:ordercreate_newuserbuying:cty{0}_u_{1}";
        /// <summary>新用户的未支付的新人专享订单</summary>
        public const string UnpaidOrderOfNewUserExclusive = "org:newuser_unpaid_order:ty{0}_u_{1}";

        /// <summary>lck for 退款申请</summary>
        public const string Refund_applyLck = "org:lck2:refund_apply:uid_{0}";
        /// <summary>lck for 退款申请</summary>
        public const string Refund_SendbackKdLck = "org:lck2:refund_sendbackkd:uid_{0}";
        /// <summary>
        /// 更新优惠券库存锁
        /// </summary>
        public const string Coupon_UpdateStockLck = "org:lck2:Coupon_UpdateStock:couponId_{0}";
        /// <summary>
        /// 优惠券预使用锁
        /// </summary>
        public const string Coupon_ReceiveUseLck = "org:lck2:Coupon_ReceiveUse:couponId_{0}";
        /// <summary>
        /// 领取优惠券锁，防止超发。
        /// </summary>
        public const string Coupon_ReceiveLck = "org:lck2:Coupon_Receive:couponId_{0}";


        public const string WxPayFlowType = "org:wxpayflowtype";

    }
    #endregion 订单

    #region 快递
    public static partial class CacheKeys
    {
        /// <summary>获取百度快递单查询接口url tokenV2</summary>
        public const string BaiduExpressApiUrlTk = "org:baidukuaidi_apiurl";
        /// <summary>获取百度快递单查询接口url的运单号</summary>
        public const string BaiduExpressApiUrl_LastNu = "org:baidukuaidi_last_nu";
        /// <summary>获取百度快递单查询接口url tokenV2</summary>
        public const string BaiduKuaidiApiTk = "org:BaiduKuaidiApiTk"; // v c
        /// <summary>用于限制调用txc17972接口</summary>
        public const string Kuaidi_Txc17972_LimitedKey = "org:kuaidiapi_txc17972:limit";
        /// <summary>用于限制调用TxcKdniao接口</summary>
        public const string Kuaidi_TxcKdniao_LimitedKey = "org:kuaidiapi_txckdniao:limit";
    }
    #endregion 快递

    #region  孩子档案
    public static partial class CacheKeys
    {
        public const string MyChildArchives = "org:childarchives:my:id_{0}";
    }
    #endregion

    #region  种草
    public static partial class CacheKeys
    {
        /// <summary>种草分享数</summary>
        public const string MiniEvltSharedCount = "org:evlt:statistics:eval_{0}:minishared";
        /// <summary>限制种草分享</summary>
        public const string MiniDoEvltSharedIncr = "org:do_evlt_minishared:userid_{0}&evltid_{1}";
        /// <summary>种草关联主体(课程|机构)</summary>
        public const string MiniEvltRelateds = "org:evlt:info:{0}:mini_relateds:agenttype_{1}";
        /// <summary>种草圈+大家的种草</summary>
        public const string MiniEvltGrassIndex = "org:evltsMain:mini_grass:1p{0}:orderby_{1}|orgid_{2}|subj_{3}|ctt_{4}|courseid_{5}";
    }
    #endregion

    #region ToSchools
    public static partial class CacheKeys
    {
        /// <summary>提供录入api-热卖课程和推荐机构</summary>
        public const string Toschool_HotsellCourses = "org:courses:toschool_hotsell:ages_{0}_{1}";
        /// <summary>学校pc首页推荐机构</summary>
        public const string ToSchool_SubjRecommendOrgsLs = "org:organizations:pc:toschool:1p{0}:type_{1}";

        /// <summary>学校pc首页推荐机构v2</summary>
        //public const string ToSchoolsv2_RecommendOrgsLs = "org:organizations:pc:toschools_v2_RecommendOrgs:1p{0}:type_{1}";
        /// <summary>热卖课程v2</summary>
        public const string Toschoolsv2_HotsellCourses = "org:courses:toschools_v2_hotsell:1p{0}:ages_{1}_{2}";
        /// <summary>热卖课程v2</summary>
        public const string Toschoolsv2_HotsellCourses2 = "org:courses:toschools_v2_hotsell2:p{0}|ages_{1}_{2}";

        /// <summary>广告21个课程</summary>
        public const string Toschool_gg21 = "org:courses:toschool_gg21:ages{0}|price{1}|subjs_{2}";

        /// <summary>
        /// 普通h5跳小程序url cache <br/>
        /// market:mpmakeurl:pathpagesA/pages/course_detail/course_detail:query:id=v6gf&__id__=1
        /// </summary>
        public const string H5ToMpUrl = "market:mpmakeurl:path{0}:query:{1}";
    }
    #endregion ToSchools


    #region 购物车
    public static partial class CacheKeys
    {
        /// <summary>用户购物车</summary>
        public const string ShoppingCart = "org:shoppingcart:userid_{0}";
        /// <summary>lock用户购物车</summary>
        public const string Lck_ShoppingCart = "org:lck2:shoppingcart:userid_{0}";
    }
    #endregion 购物车

    #region 数据中心后台统计
    public static partial class CacheKeys
    {
        /// <summary>
        /// 今日销售数据
        /// </summary>
        public const string ToDaySaleData = "org:statistics:saledata:day_{0}";

        /// <summary>
        /// 销售条形图
        /// </summary>
        public const string SalesViewData = "org:statistics:saleviewdata:day_{0}_long_{1}";
    }
    #endregion

    #region 商城主题专题
    public partial class CacheKeys
    {
        public const string MpMallHomeCoursePageLs = "org:courses:mallhomecoursepagels:page:{0}";

        /// <summary>用于删除主题cache</summary>
        public const string DelMallThemes = "org:mallthemes:*";

        /// <summary>主题个数</summary>
        public const string MallThemes_counts = "org:mallthemes:counts";
        /// <summary>主题的专题s</summary>
        public const string MallThemes_Theme_specials = "org:mallthemes:theme_{0}:specials";
        /// <summary>pc主题的专题s</summary>
        public const string MallThemes_Theme_pc_specials = "org:mallthemes:theme_{0}:pc_specials";
        /// <summary>专题的概念图片-锚点s</summary>
        public const string MallThemes_Special_concepts = "org:mallthemes:special_{0}:concepts";
        /// <summary>专题下商品s</summary>
        public const string MallThemes_Special_courses = "org:mallthemes:special_{0}:courses";
        /// <summary>pc主题的专题s下商品s</summary>
        public const string MallThemes_Theme_pc_specials_courses = "org:mallthemes:theme_{0}:pc_specials_courses";
        /// <summary>(pc)主题的下期主题</summary>
        public const string MallThemes_Theme_NextTheme = "org:mallthemes:theme_{0}:next_theme";

        /// <summary>lck商城分类保存add</summary>
        public const string MallFenleiLck_saveadd = "org:lck2:mallfenlei_bg_saveadd";
        /// <summary>lck商城分类保存update</summary>
        public const string MallFenleiLck_saveup = "org:lck2:mallfenlei_bg_saveup:code{0}";
        /// <summary>lck商城分类修改排序</summary>
        public const string MallFenleiLck_upsort = "org:lck2:mallfenlei_bg_upsort:pcode{0}";
        /// <summary>lck商城热门分类修改</summary>
        public const string MallFenleiLck_edithot = "org:lck2:mallfenlei_bg_eidthot";

        /// <summary>删除前端商城分类cache</summary>
        public const string MallFenlei_DelFontKeys = "org:catogoryItems:*";
    }
    #endregion 商城主题专题


    #region 商城积分
    public static partial class CacheKeys {

        public static string FreezeIdCacheKey(Guid advanceId)
        {
            return $"pointsmall:freeze:advanceId:{advanceId}";
        }

        public static string PresentedFreezePointsCacheKey(Guid orderDetailId)
        {
            return $"Presented:FreezePoints:orderDetailId:{orderDetailId}";
        }
    }
    #endregion
}
