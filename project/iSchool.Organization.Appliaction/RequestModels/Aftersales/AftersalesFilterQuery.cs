using iSchool.Organization.Appliaction.ViewModels.Aftersales;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    public class AftersalesFilterQuery : IRequest<AftersalesCollection>
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        /// <summary>
        /// 退货理由
        /// </summary>
        public RefundReason? RefundReason { get; set; }


        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditState? AuditState { get; set; }

        /// <summary>
        /// 售后类型
        /// </summary>
        public AftersalesType? Type { get; set; }

        /// <summary>
        /// 退货人手机号码
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 退货人昵称
        /// </summary>
        public string UserNickName { get; set; }

        /// <summary>
        /// 商品/品牌名称
        /// </summary>
        public string GoodsOrBrandName { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? SDateTime { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EDateTime { get; set; }

        /// <summary>
        /// /关联订单号
        /// </summary>
        public string OrderNumber { get; set; }


        /// <summary>
        /// 订单状态
        /// </summary>
        public int? OrderState { get; set; }

        /// <summary>
        /// 售后状态
        /// </summary>
        public AftersalesState? AftersalesState { get; set; }

    }



}

