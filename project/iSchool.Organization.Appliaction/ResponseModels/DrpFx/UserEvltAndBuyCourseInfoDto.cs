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
    public class UserEvltAndBuyCourseInfoDto
    {
        public Guid UserId { get; set; }

        /// <summary>购买课程数量</summary>
        public int BuyCourseCount { get; set; }
        /// <summary>消费金额</summary>
        public decimal ConsumedMoneys { get; set; }

        /// <summary>评测数量</summary>
        public int EvltCount { get; set; }
        /// <summary>精选评测数量</summary>
        public int StickEvltCount { get; set; }
    }
}
#nullable disable
