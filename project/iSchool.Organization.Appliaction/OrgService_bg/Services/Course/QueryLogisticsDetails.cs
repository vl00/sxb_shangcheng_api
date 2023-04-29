using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 物流详情
    /// </summary>
    public class QueryLogisticsDetails: IRequest<ResponseResult>
    {       
        /// <summary>
        /// 物流单号
        /// </summary>
        public string LogisticeCode { get; set; }

        /// <summary>
        /// 物流公司编号
        /// </summary>
        public string LogisticeName { get; set; }

        /// <summary>
        /// 物流详情api
        /// </summary>
        public string LogisticeApi { get; set; }

    }
}
