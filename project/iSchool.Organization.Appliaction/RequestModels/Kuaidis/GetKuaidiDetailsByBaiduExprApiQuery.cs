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

    public abstract class KuaidiDetailsBy3thApiQuery //: IRequest<KuaidiNuDataDto>
    {
        /// <summary>快递单号.必传</summary>
        public string Nu { get; set; } = default!;
        /// <summary>
        /// 快递公司编号.可选 <br/>
        /// 自动识别不能100%准确.一个单号可对应多个快递公司.
        /// </summary>
        public string? Com { get; set; }
        /// <summary>
        /// 一般情况下请务必不传, 不传默认为true
        /// </summary>
        public bool ReadUseDb { get; set; } = true;
        /// <summary>
        /// 一般情况下请务必不传, 不传默认为true
        /// </summary>
        public bool WriteUseDb { get; set; } = true;
    }

    /// <summary>
    /// 调用百度快递单查询接口,查询快递单号详情
    /// </summary>
    public class GetKuaidiDetailsByBaiduExprApiQuery : KuaidiDetailsBy3thApiQuery, IRequest<KuaidiNuDataDto>
    {        
        /// <summary>
        /// get请求`https://www.baidu.com/s?wd={nu}`后, 得到的html. <br/>
        /// 可选
        /// </summary>
        public string? Html { get; set; }
        /// <summary>
        /// 根据html解析出的url.可选
        /// </summary>
        public string? BaiduApiUrl { get; set; }
        /// <summary>
        /// get请求`https://www.baidu.com/s?wd={nu}`后, http响应cookie中的`BAIDUID`值. <br/>
        /// 可选, 但当`html或baiduApiUrl参数任一不为null时必填`.
        /// </summary>
        public string? BaiduId { get; set; }
    }

#nullable disable
}
