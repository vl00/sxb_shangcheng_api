using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    public class EvaluationDetailsQuery : IRequest<EvaluationDto>
    { 
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 用户中心url
        /// </summary>
        public string UserCenterBaseUrl { get; set; }
    }
}
