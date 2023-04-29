using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Lables
{

    /// <summary>
    /// 【短ID集合】机构卡片列表
    /// </summary>
    public class OrgsLablesById_ssQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 机构短id集
        /// </summary>
        public List<string> Id_ss { get; set; }

       
    }


    

}
