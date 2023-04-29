using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    public class MallThemeLsQuery : IRequest<MallThemeLsQryResult>
    {
        /// <summary>第几页</summary>
        public int PageIndex { get; set; }
        /// <summary>页大小</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 主题短id或id. 貌似用于主题列表选中某个主题后到主题详情页,再后退回主题列表页时带此参数定位回该主题
        /// <br/>有此参数可以不用pageIndex
        /// <br/>当此参数和pageIdex都缺失时,默认定位到本期主题
        /// </summary>
        public string? Id { get; set; }
    }

#nullable disable
}
