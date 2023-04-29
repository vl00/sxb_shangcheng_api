using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// RwInviteActivity 用户积分操作
    /// </summary>
    public class UserScoreOnRwInviteActivityArgs : IRequest<UserScoreOnRwInviteActivityResult>
    {
        /// <summary>用户id</summary>        
        public Guid? UserId { get; set; }
        public string? UnionID { get; set; }

        public CourseExchangeTypeEnum CourseExchangeType { get; set; }

        public UserScoreOnRwInviteActivityArgs SetCourseExchangeType(CourseExchangeTypeEnum courseExchangeType)
        {
            this.CourseExchangeType = courseExchangeType;
            return this;
        }

        public object Action { get; set; } = default!;


        /// <summary>
        /// 预消费(扣减)积分
        /// </summary>
        /// <returns>
        /// 被扣后的积分 <br/>        
        /// -2 = 积分不够
        /// </returns>
        public UserScoreOnRwInviteActivityArgs PreConsume(double score)
        {
            this.Action = new PreConsumeAction { Score = score };
            return this;
        }
        public class PreConsumeAction : IRequest<double>
        {
            public double Score { get; set; }
        }

        /// <summary>
        /// 消费(扣减)积分
        /// </summary>
        /// <returns>
        /// 被扣后的积分 <br/>        
        /// -2 = 积分不够
        /// </returns>
        public UserScoreOnRwInviteActivityArgs Consume(double score)
        {
            this.Action = new ConsumeAction { Score = score };
            return this;
        }
        public class ConsumeAction : IRequest<double>
        {
            public double Score { get; set; }
        }
    }


#nullable disable
}
