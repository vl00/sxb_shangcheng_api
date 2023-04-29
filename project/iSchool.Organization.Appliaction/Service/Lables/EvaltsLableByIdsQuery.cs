using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 【长Id集合】评测卡片请求实体
    /// </summary>
    public class EvaltsLableByIdsQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测长Id集
        /// </summary>
        public List<Guid>  Ids { get; set; }

    }   

}
