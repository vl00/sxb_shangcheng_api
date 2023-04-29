using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 根据快递单号批量查询快递单最新desc
    /// </summary>
    public class GetLastKdnusDescQueryArgs : IRequest<IEnumerable<(string Nu, string Comcode, string Desc, DateTime Time)>>
    {
        /// <summary>快递单号+快递公司编码s</summary>
        public (string Nu, string Comcode)[] Nus { get; set; } = default!;

        public bool ReqApi { get; set; } = false;
    }

#nullable disable
}
