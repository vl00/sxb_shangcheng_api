using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Aftersales
{

    public class AftersalesCollection
    {
        public int Total { get; set; }

        public IEnumerable<Aftersales> Datas { get; set; }
    }
    public class Aftersales
    {
        public Guid Id { get; set; }

        /// <summary>
        /// 退款编号
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 退货商品
        /// </summary>
        public SKU SKU { get; set; }

        /// <summary>
        /// 退回数量
        /// </summary>
        public int ReturnCount { get; set; }

        /// <summary>
        /// 售后类型
        /// </summary>
        public AftersalesType Type { get; set; }

        /// <summary>
        /// 实付金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 申请退款金额
        /// </summary>
        public decimal ApplyRefundAmount { get; set; }

        /// <summary>
        /// 实退金额
        /// </summary>
        public decimal? RefundAmount { get; set; }

        /// <summary>
        /// 申请售后日期
        /// </summary>
        public DateTime ApplyDateTime { get; set; }

        /// <summary>
        /// 退款原因
        /// </summary>
        public RefundReason? Reason { get; set; }

        /// <summary>
        /// 退款人昵称
        /// </summary>
        public string RefundUserNickName { get; set; }

        /// <summary>
        /// 退款人手机号码
        /// </summary>
        public string RefundUserPhoneNumber { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// 售后状态
        /// </summary>
        public AftersalesState State { get; set; }

        /// <summary>
        /// 首次审核结果
        /// </summary>
        public AuditResult FirstAuditResult { get; set; }

        /// <summary>
        /// 第二次审核结果
        /// </summary>
        public AuditResult SecondAuditResult { get; set; }

        /// <summary>
        /// 快递信息
        /// </summary>
        public ExpressInfo ExpressInfo { get; set; }

        /// <summary>
        /// 寄回地址
        /// </summary>
        public string SendBackAddress { get; set; }

        public string SendBackMobile { get; set; }

        public string SendBackUserName { get; set; }

        /// <summary>
        /// 寄回时间
        /// </summary>
        public DateTime? SendBackTime { get; set; }

        /// <summary>
        /// 退款原因描述
        /// </summary>
        public string ReasonDesc { get; set; }

        /// <summary>
        /// 申请凭证（图片/视频）
        /// </summary>
        public IEnumerable<dynamic> Vouchers { get; set; }


        /// <summary>
        /// 退款时间
        /// </summary>
        public DateTime? RefundTime { get; set; }

        /// <summary>
        /// 售后单完成时间
        /// </summary>

        public DateTime? FinishTime
        {
            get
            {
                if (this.State == AftersalesState.HasRefund)
                {
                    return this.RefundTime;
                }
                else
                {
                    if (this.SecondAuditResult != null)
                    {
                        return this.SecondAuditResult.AuditDateTime;
                    }
                    else if (this.FirstAuditResult != null)
                    {
                        if (this.FirstAuditResult.State == AuditState.AduitFail)
                        {
                            return this.FirstAuditResult.AuditDateTime;

                        }
                    }
                }
                return null;
            }
        }


        public OrderInfo OrderInfo { get; set; }


        public OrderRefundSpecialReason SpecialReason { get; set; }

        public string Remark { get; set; }




    }

    public class Address
    {

        public string SendBackAddress { get; set; }

        public string SendBackMobile { get; set; }

        public string SendBackUserName { get; set; }


    }

    public class OrderInfo
    {
        public int State { get; set; }

        public DateTime CreateTime { get; set; }

    }


    /// <summary>
    /// 审核结果
    /// </summary>
    public class AuditResult
    {

        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditState State { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public Guid AuditorId { get; set; }

        public string AuditorName { get; set; }

        /// <summary>
        /// 审核备注
        /// </summary>
        public string Remark { get; set; }

        public DateTime? AuditDateTime { get; set; }

    }
    public class SKU
    {

        public Guid Id { get; set; }
        public Guid GoodsId { get; set; }
        public string GoodsName { get; set; }

        public Guid PropId { get; set; }

        public string PropName { get; set; }

    }
    /// <summary>
    /// 物流
    /// </summary>
    public class ExpressInfo
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Code { get; set; }
    }

    public class OrderRefundDetail
    {



        public Guid CourseID { get; set; }

        public string CourseTitle { get; set; }
        public string CourseSubTitle { get; set; }

        public List<string> Banners { get; set; }

        public List<string> BannerThumbnails { get; set; }
        public string RefundStatus { get; set; }

        public decimal Price { get; set; }

        public int RefundCount { get; set; }

        public decimal RefundAmount { get; set; }

        public bool IsContainFreight { get; set; }

    }


    /// <summary>
    /// 
    ///退款理由选择：
    ///1.不想要了、2.商品信息拍错（属性/颜色等）、3.地址/电话信息填写错误、4.拍多了、5.协商一致退款、6.缺货、7.发货速度不满意、8.其他。
    ///
    ///11颜色/尺寸/参数不符、12商品瑕疵、13质量问题、14少件/漏发、15其他。
    /// </summary>
    public enum RefundReason
    {


        /// <summary>
        /// 不想要了
        /// </summary>
        [Description("不想要了")]
        UnWanted = 1,
        /// <summary>
        /// 商品信息错误
        /// </summary>
        [Description("商品信息拍错（属性/颜色等）")]
        InfomationError = 2,
        /// <summary>
        /// 收货信息错误
        /// </summary>
        [Description("地址/电话信息填写错误")]
        ShippingAddressError = 3,
        /// <summary>
        /// 拍多了
        /// </summary>
        [Description("拍多了")]
        SoMore = 4,
        /// <summary>
        /// 协商一致退款
        /// </summary>
        [Description("协商一致退款")]
        ConsultRefund = 5,
        /// <summary>
        /// 缺货
        /// </summary>
        [Description("缺货")]
        OutOfStock = 6,
        /// <summary>
        /// 发货速度不满意
        /// </summary>
        [Description("发货速度不满意")]
        SendOutSpeedSlow = 7,
        /// <summary>
        /// 其他
        /// </summary>
        [Description("其他")]
        Others = 8,

        /// <summary>
        /// 商品参数不符
        /// </summary>
        [Description("颜色/尺寸/参数不符")]
        GoodsParamsUnRight = 11,
        /// <summary>
        /// 商品瑕疵
        /// </summary>
        [Description("商品瑕疵")]
        GoodsFlaw = 12,
        /// <summary>
        /// 质量问题
        /// </summary>
        [Description("13质量问题")]
        QualityQuestion = 13,
        /// <summary>
        /// 漏发
        /// </summary>
        [Description("少件/漏发")]
        SendOmit = 14,
        /// <summary>
        /// 退货退款的其他
        /// </summary>
        [Description("其他")]
        OtherSecond = 15





    }


    /// <summary>
    /// 售后状态
    /// </summary>
    public enum AftersalesState
    {
        /// <summary>
        /// 等待审核
        /// </summary>
        [Description("等待审核")]
        WaitAudit = 0,
        /// <summary>
        /// 等待用户退货
        /// </summary>
        [Description("等待用户退货")]
        WaitReback = 1,
        /// <summary>
        /// 用户已退货
        /// </summary>
        [Description("用户已退货")]
        HasReback = 2,
        /// <summary>
        /// 审核未通过
        /// </summary>
        [Description("审核未通过")]
        ApplyRefus = 3,
        /// <summary>
        /// 审核已撤销
        /// </summary>
        [Description("审核已撤销")]
        ApplyCancel = 4,
        /// <summary>
        /// 已退款
        /// </summary>
        [Description("已退款")]
        HasRefund = 5,


    }

    public enum AuditState
    {
        /// <summary>
        /// 未审核
        /// </summary>
        [Description("未审核")]
        UnAudit,
        /// <summary>
        /// 审核通过
        /// </summary>
        [Description("审核通过")]
        AduitSuccess,
        /// <summary>
        /// 审核不通过
        /// </summary>
        [Description("审核不通过")]
        AduitFail,

    }

    /// <summary>
    /// 售后类型 1.退款  2.退货退款 3.极速退款  4.后台退款
    /// </summary>
    public enum AftersalesType
    {
        /// <summary>
        /// 退款
        /// </summary>
        [Description("退款")]
        Refund = 1,
        /// <summary>
        /// 退货退款
        /// </summary>
        [Description("退货退款")]
        ReturnRefund = 2,
        /// <summary>
        /// 极速退款
        /// </summary>
        [Description("极速退款")]
        QuickRefund = 3,
        /// <summary>
        /// 后台退款
        /// </summary>
        [Description("后台退款")]
        BackgroundRefund = 4,
    }



}
