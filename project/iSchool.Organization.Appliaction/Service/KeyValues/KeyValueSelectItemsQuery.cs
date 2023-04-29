using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.KeyValues
{
    /// <summary>
    /// 类别
    /// </summary>
    public class KeyValueSelectItemsQuery:IRequest<ResponseResult>
    {
        /// <summary>
        /// 类别
        /// </summary>
        public int Type { get; set; }
    }
}
