using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    
    /// <summary>
    /// 更新评测官方点赞数
    /// </summary>
    public class UpdateEvltShamLikesByIdCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 官方点赞数
        /// </summary>
        public int ShamLikes { get; set; }
    }
}
