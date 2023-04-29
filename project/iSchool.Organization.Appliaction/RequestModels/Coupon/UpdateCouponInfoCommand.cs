using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class UpdateCouponInfoCommand : IRequest<bool>, IValidatableObject
    {
        [Required]
        public Guid Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }

        [Range(1, 999999)]
        public CouponType CouponType { get; set; }


        public decimal? FeeOver { get; set; }
        public decimal? Fee { get; set; }

        public decimal? Discount { get; set; }

        public decimal? PriceOfTest { get; set; }
        public int Stock { get; set; }
        public int MaxTake { get; set; }
        public bool IsHide { get; set; }

        [Range(1, 999999)]
        public CouponInfoVaildDateType ExpireTimeType { get; set; }

        public int? ExpireDays { get; set; }

        public DateTime? STime { get; set; }

        public DateTime? ETime { get; set; }


        /// <summary>
        /// 可用范围
        /// </summary>
        public IEnumerable<dynamic> EnableRanges { get; set; }

        /// <summary>
        /// 规则说明
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string RuleDesc { get; set; }

        public string Link { get; set; }

        public Guid Updator { get; set; }

        public string ICon { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ExpireTimeType == CouponInfoVaildDateType.SpecialDate)
            {
                if (STime == null)
                {
                    yield return new ValidationResult("有效日期为指定日期模式时，STime是必须的。", new[] { nameof(STime) });
                }
                if (ETime == null)
                {
                    yield return new ValidationResult("有效日期为指定日期模式时，ETime是必须的。");
                }
            }
            else if (ExpireTimeType == CouponInfoVaildDateType.SpecialDays)
            {
                if (ExpireDays.GetValueOrDefault() <= 0)
                {
                    yield return new ValidationResult("有效日期为指定天数模式时，ExpireDays是必须且要大于0的。");
                }
            }


        }
    }
}
