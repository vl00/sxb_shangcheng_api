using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 【短Id集合】评测卡片请求实体
    /// </summary>
    public class EvaltsLableById_ssQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测短Id集
        /// </summary>
        public List<string>  Id_ss { get; set; }

    }   

}
