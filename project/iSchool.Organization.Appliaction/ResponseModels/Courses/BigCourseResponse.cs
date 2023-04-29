using iSchool.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 体验课关联的大课返回实体类
    /// </summary>
    public class BigCourseResponse
    {               
        /// <summary>大课Id</summary>
        public Guid Id { get; set; }     

        /// <summary>
        /// 体验课关联大课名称
        /// </summary>
        public string Title { get; set; }             

        /// <summary>
        /// 课程金额
        /// </summary>
        public decimal Price { get; set; }

        #region 分销信息

        /// <summary>
        /// 平级佣金类型(1：%；2：元)
        /// </summary>
        public int PJCashbackType { get; set; }

        /// <summary>
        /// 平级佣金数值
        /// </summary>
        public decimal? PJCashbackValue { get; set; }

        /// <summary>
        /// 平级佣金金额
        /// (平级佣金类型=1，平级佣金金额 = 课程金额 * 平级佣金数值 * 0.01；平级佣金类型=2，平级佣金金额 = 平级佣金数值;)
        /// </summary>
        public decimal? PJCashbackMoney { get; set; }

        /// <summary>
        /// 返现类型(1：%；2：元)
        /// </summary>
        public int CashbackType { get; set; }

        /// <summary>
        /// 返现数值
        /// </summary>
        public decimal? CashbackValue { get; set; }

        /// <summary>
        /// 自购返现金额
        /// (返现类型=1，自购返现金额 = 课程金额 * 返现数值 * 0.01；返现类型=2，自购返现金额 = 返现数值;)
        /// </summary>
        public decimal? CashbackMoney { get; set; }
        
        /// <summary>
        /// 工资系数计算（1：是；0：否）
        /// </summary>
        public int IsBonusRate { get; set; }

        /// <summary>
        /// 上线独享数值
        /// </summary>
        public decimal? HeadFxUserExclusiveValue { get; set; }

        /// <summary>
        /// 上线独享类型(1：%；2：元)
        /// </summary>
        public int HeadFxUserExclusiveType { get; set; }

        /// <summary>
        /// 上线独享收益
        /// (上线独享类型=1，上线独享收益 = 课程金额 * 上线独享数值 * 0.01；上线独享类型=2，上线独享收益=上线独享数值;)
        /// </summary>
        public decimal? HeadFxUserExclusiveMoney { get; set; }


        #endregion



    }



}
