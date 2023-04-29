using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Orders
{
    /// <summary>
    /// 订单详情实体【后台】
    /// </summary>
    public class OrderDetailsDto
    {

        /// <summary>订单号</summary>
        public string Code { get; set; }

        /// <summary>orderdetail状态</summary>
        public int Status { get; set; }
        /// <summary>order状态</summary>
        public int OrderStatus { get; set; }

        public decimal TotalPayment { get; set; }

        public int? totalPoints { get; set; }

        public decimal Freight { get; set; }

        /// <summary>
        /// 订单状态详情
        /// </summary>
        public string StatusDec { get; set; }

        /// <summary>下单时间</summary>
        public string CreateTime { get; set; }

        /// <summary>下单账户Id</summary>
        public Guid UserId { get; set; }

        /// <summary>下单账户手机号</summary>
        public string UserMobile { get; set; }

        /// <summary>下单账户昵称</summary>
        public string Nickname { get; set; }

        /// <summary>收货人姓名</summary>
        public string RecvUsername { get; set; }

        /// <summary>收货人电话</summary>
        public string RecvMobile { get; set; }

        /// <summary>收获地址</summary>
        public string Address { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string RecvProvince { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string RecvCity { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        public string RecvArea { get; set; }

        /// <summary>物流最新信息</summary>
        public string LastJStr { get; set; }

        /// <summary>物流订单号</summary>
        public string ExpressCode { get; set; }

        /// <summary>
        /// 物流类型
        /// </summary>
        public string ExpressType { get; set; }

        /// <summary>课程兑换码</summary>
        public string DHCode { get; set; }

        /// <summary>约课状态</summary>
        public string AppointmentStatus { get; set; }

        /// <summary>快递公司名称</summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 订单详情数量
        /// </summary>
        public short DetailCount { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 订单详情id
        /// </summary>
        public Guid DetailId { get; set; }

        public List<GoodsData> GoodsDatas { get; set; }

        public OrderDetailsDto Clone()
        {
            return this.MemberwiseClone() as OrderDetailsDto;
        }
    }


    public class GoodsData
    {
        public Guid GoodId { get; set; }

        public decimal? CouponAmount { get; set; }

        public byte Status { get; set; }
        /// <summary>课程封面图(第一张)</summary>
        public string Banner { get; set; }

        /// <summary>机构名称</summary>
        public string OrgName { get; set; }

        /// <summary>课程标题(名称)</summary>
        public string Title { get; set; }

        /// <summary>课程副标题</summary>
        public string SubTitle { get; set; }

        /// <summary>价格</summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 原单价
        /// </summary>
        public decimal? Origprice { get; set; }

        /// <summary>数量</summary>
        public int Number { get; set; }

        /// <summary>
        /// OrderDetail表里的payment。
        /// </summary>
        public decimal? Payment { get; set; }

        /// <summary>实际支付金额</summary>
        [Obsolete]
        public decimal? TotalPayment { get; set; }
        /// <summary>
        /// 退货数
        /// </summary>
        public int RefundCount { get; set; }

        public int? Point { get; set; }

        public GoodsData Clone()
        {
            return this.MemberwiseClone() as GoodsData;
        }
    }

}
