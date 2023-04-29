using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 根据收货人/下单人手机号，查询用户信息及其小课订单信息列表返回实体Model
    /// </summary>
    public class OrdersByMobileQueryResponse
    {
        /// <summary>
        /// 下单人Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 下单人姓名
        /// </summary>
        public string NickName { get; set; }

       
        /// <summary>
        /// 体验课订单信息集合
        /// </summary>
        public List<SmallCourseOrder> SmallCoursesOrders { get; set; }
    }



    /// <summary>
    /// 体验课实体订单信息
    /// </summary>
    public class SmallCourseOrder
    {
        /// <summary>
        /// 下单人Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 实际支付金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 实际支付优惠金额(目前没有优惠)
        /// </summary>
        public decimal PayDisccountAmount { get; set; } = 0;

        /// <summary>
        /// 分销订单类型(商品类型)
        /// (1:课程购买小课;2:课程购买大课;)
        /// </summary>
        public int OrderType { get; set; } = 1;

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrdeId { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 购买商品的数量
        /// </summary>
        public int GoodsCount { get; set; }
    }

    #region DB
    public class OrdersByMobileQueryDB
    {
        /// <summary>
        /// 下单人Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 下单人姓名
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 实际支付金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 实际支付优惠金额(目前没有优惠)
        /// </summary>
        public decimal PayDisccountAmount { get; set; } = 0;

        /// <summary>
        /// 分销订单类型(商品类型)
        /// (1:课程购买小课;2:课程购买大课;)
        /// </summary>
        public int OrderType { get; set; } = 1;

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrdeId { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 商品数量
        /// </summary>
        public int GoodsCount { get; set; }
               
    } 
    #endregion

}
