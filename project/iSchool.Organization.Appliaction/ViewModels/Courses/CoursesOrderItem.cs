using iSchool.Organization.Domain.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Courses
{
    /// <summary>
    /// 课程留资/订单
    /// </summary>
    public class CoursesOrderItem
    {

        #region 导出顺序对应字段
        /// <summary>订单Id </summary>
        public Guid Id { get; set; }

        public Guid OrderDetailId { get; set; }

        /// <summary>订单号</summary>
        public string Code { get; set; }

        /// <summary>支付时间</summary>
        public DateTime? PaymentTime { get; set; }

        /// <summary>机构</summary>
        public string OrgName { get; set; }

        /// <summary>课程名称</summary>
        public string Title { get; set; }

        /// <summary>套餐(目前只有属性1)</summary>
        public string SetMeal { get; set; }

        /// <summary>(单)价格</summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 兑换积分
        /// </summary>
        public int? Point { get; set; }

        /// <summary>原(单)价格</summary>
        public decimal Origprice { get; set; }
        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal CouponAmount { get; set; }
        /// <summary>数量</summary>
        public int Number { get; set; }

        /// <summary>电话(下单人)</summary>
        public string Mobile { get; set; }

        /// <summary>姓名(下单人)</summary>
        public string UserName { get; set; }

        /// <summary>年龄</summary>
        public int? Age { get; set; }

        /// <summary>收货人姓名</summary>
        public string RecvUserName { get; set; }

        /// <summary>收货人电话</summary>
        public string RecvMobile { get; set; }

        /// <summary>省份</summary>
        public string RecvProvince { get; set; }

        /// <summary>市区</summary>
        public string RecvCity { get; set; }

        /// <summary>区/县</summary>
        public string RecvArea { get; set; }

        /// <summary>具体地址</summary>
        public string Address { get; set; }

        /// <summary>订单状态</summary>
        public int Status { get; set; }
        /// <summary>订单状态(desc)</summary>
        public string StatusEnumDesc { get; set; }

        /// <summary>约课状态</summary> 
        public int? AppointmentStatus { get; set; }

        /// <summary>物流单号</summary>
        public string ExpressCode { get; set; }

        /// <summary>兑换码</summary>
        public string ExchangeCode { get; set; }

        /// <summary>
        /// 微信昵称
        /// </summary>
        public string WXNickName { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 机构反馈
        /// </summary>
        public string SystemRemark { get; set; }


        #endregion

        /// <summary>
        /// 发送物流时间
        /// </summary>
        public DateTime? SendExpressTime { get; set; }

        /// <summary>
        /// 物流公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 商品信息-json
        /// </summary>
        public string Ctn0 { get; set; }

        public JObject Ctn { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>课程Id</summary>
        public Guid CourseId { get; set; }

        /// <summary>留资时间(格式：yyy/MM/dd HH:mm:ss)</summary>
        public string CreateTime { get; set; }

        /// <summary>科目</summary>
        public string Subject { get; set; }

        /// <summary>用户Id</summary>
        public Guid UserId { get; set; }

        /// <summary>约课状态列表 </summary> 
        public List<SelectListItem> AppointmentStatusList { get; set; }

        /// <summary>物流公司编号</summary>
        public string ExpressType { get; set; }

        /// <summary>支付总金额(用于退款)</summary>
        public decimal? TotalPayment { get; set; }
        /// <summary>
        /// 订单详情支付金额
        /// </summary>
        public decimal? Payment { get; set; }


        /// <summary>支付总积分</summary>
        public decimal? TotalPoints { get; set; }

        /// <summary>上课电话(用于发送兑换码)</summary>
        public string BeginClassMobile { get; set; }

        /// <summary>
        /// 是否小程序订单
        /// </summary>
        public string IsMinOrder { get; set; }


        /// <summary>预订单号</summary> 
        public string AdvanceOrderNo { get; set; }
        /// <summary>预订单id</summary> 
        public Guid AdvanceOrderId { get; set; }
        /// <summary>
        /// 成本价
        /// </summary>
        public decimal Costprice { get; set; }
        /// <summary>
        /// 货号
        /// </summary>
        public string ArticleNo { get; set; }

        /// <summary>
        /// 是否是多物流
        /// </summary>
        public bool? IsMultipleExpress { get; set; }

        /// <summary>
        /// 供应商的名字
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// 商品类型  1.网课   2.好物
        /// </summary>
        public byte ProductType { get; set; }
    }
}
