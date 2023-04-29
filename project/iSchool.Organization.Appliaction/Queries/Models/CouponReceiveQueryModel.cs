using iSchool.Infras;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace iSchool.Organization.Appliaction.Queries
{

    public class CouponReceiveQueryModel : SearchBaseQueryModel
    {
        /// <summary>
        /// 优惠券类型
        /// </summary>
        public CouponType? CouponType { get; set; }

        /// <summary>
        /// 领取状态
        /// </summary>
        public CouponReceiveStateExt? Status { get; set; }

        /// <summary>
        /// 领券人昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 领券人手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 领取编码
        /// </summary>
        public string ReceiveNumber { get; set; }

        /// <summary>
        /// 优惠券编码
        /// </summary>
        public string CouponNumber { get; set; }

        /// <summary>
        /// 优惠券标题
        /// </summary>
        public string CouponName { get; set; }

        /// <summary>
        /// 可用范围
        /// </summary>
        public string EnableRange { get; set; }

        /// <summary>
        /// 有效日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 有效日期
        /// </summary>
        public DateTime? EndDate { get; set; }


        public string GetCacheKey()
        {
            var exclundPropertyNames = new string[] { nameof(PageIndex), nameof(PageSize) };
            var type = GetType();
            var properties = type.GetProperties().OrderBy(s => s.Name);

            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                if (!exclundPropertyNames.Contains(property.Name))
                {
                    sb.Append(property.Name);
                    sb.Append(property.GetValue(this));
                }
            }

            return HashAlgmUtil.Encrypt(sb.ToString(), HashAlgmUtil.Md5);
        }
    }
}