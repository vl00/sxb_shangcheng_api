using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 组建顾问团队信息dto
    /// </summary>
    public class HeaderFxTeamSetupInfoDto
    {
        /// <summary>消费金额</summary>
        public decimal ConsumedMoneys { get; set; }
        /// <summary>评测数量</summary>
        public int EvltCount { get; set; }
        /// <summary>精选评测数量</summary>
        public int StickEvltCount { get; set; }

        /// <summary>条件1-消费金额</summary>
        public decimal Condition1ConsumedMoneys { get; set; } = 99m;
        /// <summary>条件2-评测数量</summary>
        public int Condition2EvltCount { get; set; } = 10;
        /// <summary>条件2-精选评测数量</summary>
        public int Condition2StickEvltCount { get; set; } = 5;

        /// <summary>是否条件1达成</summary>
        public bool IsCondition1Ok => ConsumedMoneys >= Condition1ConsumedMoneys;
        /// <summary>是否条件2达成</summary>
        public bool IsCondition2Ok
        {
            get 
            {
                if (Condition2EvltCount == -2 || Condition2StickEvltCount == -2) return false;
                return EvltCount >= Condition2EvltCount && StickEvltCount >= Condition2StickEvltCount;
            }
        }

        /// <summary>顾问案例s</summary>
        public HeaderFxCaseItemDto[] Cases { get; set; } = default!;
        /// <summary>综合概述</summary>
        public string? AllDesc { get; set; }

        /// <summary>已确认收货的消费金额</summary>
        public decimal ShippedOkMoneys { get; set; } = 0m;
    }

    /// <summary>
    /// 顾问案例
    /// </summary>
    public class HeaderFxCaseItemDto
    {
        /// <summary>用户头像</summary>
        public string? UserImg { get; set; }
        /// <summary>用户名</summary>
        public string UserName { get; set; } = default!;
        /// <summary>简单描述</summary>
        public string? Phrase { get; set; }
        /// <summary>详细描述</summary>
        public string? Desc { get; set; }
    }
}
#nullable disable
