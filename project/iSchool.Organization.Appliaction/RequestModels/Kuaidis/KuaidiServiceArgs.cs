using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 
    /// </summary>
    public class KuaidiServiceArgs : IRequest<KuaidiServiceArgsResult>
    {
        [IgnoreDataMember]
        public object Args { get; set; } = default!;

        public KuaidiServiceArgs() { }
        public KuaidiServiceArgs(object args) => this.Args = args;

        /// <summary>查询快递公司编码</summary>
        public static KuaidiServiceArgs GetCode(string code)
        {
            return new KuaidiServiceArgs { Args = new GetCodeQuery { Code = code } };
        }        
        public class GetCodeQuery : IRequest<KdCompanyCodeDto?>
        {
            public string Code { get; set; } = default!;
        }

        /// <summary>格式化第三方接口结果</summary>
        public static KuaidiServiceArgs ParseSrcResult(JToken srcResult, int srcType)
        {
            return new KuaidiServiceArgs { Args = new ParseSrcResultCmd { SrcResult = srcResult, SrcType = srcType } };
        }        
        public class ParseSrcResultCmd : IRequest<IEnumerable<KuaidiNuDataItemDto>>
        {
            public JToken SrcResult { get; set; } = default!;
            public int SrcType { get; set; }
        }

        /// <summary>
        /// find in db.
        /// </summary>
        public static KuaidiServiceArgs ReadFromDB(string nu, string? com)
        {
            return new KuaidiServiceArgs { Args = new ReadFromDBQuery { Nu = nu, Com = com } };
        }
        public class ReadFromDBQuery : IRequest<(KuaidiNuDataDto, bool)>
        {
            public string Nu { get; set; } = default!;
            public string? Com { get; set; }
        }

        /// <summary>
        /// sync to db. <br/>
        /// will resolve 'dbmodel.Company' 
        /// </summary>
        public static KuaidiServiceArgs WriteToDB(KuaidiNuData dbmodel, Guid? prevId)
        {
            return new KuaidiServiceArgs { Args = new WriteToDBCmd { Dbmodel = dbmodel, PrevId = prevId } };
        }
        public class WriteToDBCmd : IRequest<Exception>
        {
            public KuaidiNuData Dbmodel { get; set; } = default!;
            public Guid? PrevId { get; set; }
        }

        /// <summary>快递公司编码s</summary>
        public static KuaidiServiceArgs GetCompanyCodes()
        {
            return new KuaidiServiceArgs { Args = new GetCompanyCodesQuery() };
        }
        public class GetCompanyCodesQuery : IRequest<KeyValuePair<string, string>[]>
        {
        }

        /// <summary>简易检测单号是否正确.结果不是null就没错</summary>
        public static KuaidiServiceArgs CheckNu(string nu, string? com = null)
        {
            return new KuaidiServiceArgs { Args = new CheckNuCmd { Nu = nu, Com = com } };
        }
        public class CheckNuCmd : IRequest<KdCompanyCodeDto?>
        {
            public string Nu { get; set; } = default!;
            public string? Com { get; set; }
        }
    }

#nullable disable
}
