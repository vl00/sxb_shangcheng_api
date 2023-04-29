using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 评测卡片请求实体
    /// </summary>
    public class EvalLableByUrlQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测详情页Url
        /// </summary>
        public string  EvalDetailUrl { get; set; }

    }   

}
