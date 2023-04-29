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
    /// 查询快递单号详情v2 by 腾讯云-17972-全国物流快递查询
    /// </summary>
    public class GetKuaidiDetailsByTxc17972ApiQuery : KuaidiDetailsBy3thApiQuery, IRequest<KuaidiNuDataDto>
    {
        /// <summary>
        /// 第三方接口要求的参数,可选. <br/>
        /// 例如: 当快递为SF时,需要收件人或寄件人手机号后四位
        /// </summary>
        public string? Customer { get; set; }
    }

#nullable disable
}
