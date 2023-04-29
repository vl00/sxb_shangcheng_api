using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class CouponInfosFilterQuery: IRequest<CouponInfos>
    {

        /// <summary>
        /// 券类型
        /// </summary>
        public CouponType? Type { get; set; }

        /// <summary>
        /// 券状态
        /// </summary>
        public CouponState? State { get; set; }
        /// <summary>
        /// 有效时间类型
        /// </summary>
        public CouponInfoVaildDateType? ExpireTimeType { get; set; }

        /// <summary>
        /// 有效天数 ExpireTimeType 为 2 时必传
        /// </summary>
        public int? ExpireDays { get; set; }

        public DateTime? STime { get; set; }
        public DateTime? ETime { get; set; }

        /// <summary>
        /// 券编号
        /// </summary>
        public string Num { get; set; }

        /// <summary>
        /// 券标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 可用范围搜索关键词
        /// </summary>
        public string EnableRangeKeyWord { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 20;

    }
}
