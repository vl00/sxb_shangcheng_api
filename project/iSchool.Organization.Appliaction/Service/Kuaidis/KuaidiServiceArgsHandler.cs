using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Modles;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public partial class KuaidiServiceArgsHandler : IRequestHandler<KuaidiServiceArgs, KuaidiServiceArgsResult>
    {
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;
        NLog.ILogger _log;
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        ILock1Factory _lock1Factory;

        public KuaidiServiceArgsHandler(IHttpClientFactory httpClientFactory, IConfiguration config,
            IOrgUnitOfWork orgUnitOfWork, ILock1Factory lock1Factory, IMediator mediator,
            IServiceProvider services)
        {
            this._httpClientFactory = httpClientFactory;
            this._config = config;
            this._log = services.GetService<NLog.ILogger>();
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._lock1Factory = lock1Factory;
            this._mediator = mediator;
        }

        public async Task<KuaidiServiceArgsResult> Handle(KuaidiServiceArgs req, CancellationToken cancellation)
        {
            var res = new KuaidiServiceArgsResult();
            switch (req.Args)
            {
                case KuaidiServiceArgs.GetCodeQuery _GetCodeQuery:
                    res.Result = await Handle_GetCodeQuery(_GetCodeQuery.Code);
                    break;
                case KuaidiServiceArgs.ParseSrcResultCmd _ParseSrcResultCmd:
                    res.Result = Handle_ParseSrcResult(_ParseSrcResultCmd.SrcResult, _ParseSrcResultCmd.SrcType);
                    break;
                case KuaidiServiceArgs.ReadFromDBQuery _ReadFromDBQuery:
                    res.Result = await Handle_ReadFromDBQuery(_ReadFromDBQuery);
                    break;
                case KuaidiServiceArgs.WriteToDBCmd _WriteToDBCmd:
                    res.Result = await Handle_WriteToDBCmd(_WriteToDBCmd);
                    break;
                case KuaidiServiceArgs.GetCompanyCodesQuery _GetCompanyCodesQuery:
                    res.Result = Handle_GetCompanyCodesQuery();
                    break;
                case KuaidiServiceArgs.CheckNuCmd _CheckNuCmd:
                    res.Result = await Handle_CheckNuCmd(_CheckNuCmd);
                    break;
            }
            return res;
        }

        private static IEnumerable<KuaidiNuDataItemDto> Handle_ParseSrcResult(JToken srcResult, int srcType)
        {
            switch (srcType)
            {
                case (int)KuaidiApiResultSrcTypeEnum.Baidu:
                    {
                        var list = srcResult.SelectToken("data.info.context") as JArray;
                        if (list == null) return null;
                        return list.Select(r => new KuaidiNuDataItemDto
                        {
                            Desc = (string)r["desc"],
                            Time = (string)r["time"] is string st ? DateTimeExchange.ToLocalTimeDateBySeconds(Convert.ToInt64(st)).ToString("yyyy-MM-dd HH:mm:ss") : null,
                        }).AsArray();
                    }
                case (int)KuaidiApiResultSrcTypeEnum.Txc17972:
                    {
                        var list = srcResult["list"] as JArray;
                        if (list == null) return null;
                        if ((string)srcResult["type"] == "SF" && list.Count > 0)
                        {
                            if ((string)list[0]["content"] == "在官网\"运单资料&签收图\",可查看签收人信息") 
                                list.RemoveAt(0);
                        }
                        return list.Select(r => new KuaidiNuDataItemDto
                        {
                            Desc = (string)r["content"],
                            Time = DateTime.TryParse((string)r["time"], out var dt) ? dt.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        }).Where(_ => !_.Time.IsNullOrEmpty()).AsArray();
                    }
                case (int)KuaidiApiResultSrcTypeEnum.TxcKdniao:
                    {
                        var list = srcResult["Traces"] as JArray;
                        if (list == null) return null;
                        return list.Select(r => new KuaidiNuDataItemDto
                        {
                            Desc = (string)r["AcceptStation"],
                            Time = (string)r["AcceptTime"],
                        }).OrderByDescending(_ => _.Time).AsArray();
                    }
                default:
                    return null;
            }
        }


        private async Task<KdCompanyCodeDto> Handle_GetCodeQuery(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            await default(ValueTask);

            var arr = KdcodeDatas_for_transformat().Where(a => a.Any(c => c != null && c == code)).FirstOrDefault();
            if (arr?.Length > 0)
            {
                return new KdCompanyCodeDto
                {
                    Code = arr[1],
                    Com = arr[0],
                    Code100 = arr[2],
                    ComAlias = arr[3..] is string[] a3 && a3.Length > 0 ? a3 : null,
                };
            }

            return null;
        }

        /// <summary>
        /// [com_name, code, code100, alias...]
        /// </summary>
        /// <returns></returns>
        static IEnumerable<string[]> KdcodeDatas_for_transformat()
        {
            #region g Kuaidi-company-code

            

            #endregion g Kuaidi-company-code

            // and more...
        }

        private KeyValuePair<string, string>[] Handle_GetCompanyCodesQuery()
        {
            return KdcodeDatas_for_transformat().Select(arr => KeyValuePair.Create(arr[1], arr[0])).OrderBy(_ => _.Key).ToArray();
        }


        private async Task<(KuaidiNuDataDto, bool)> Handle_ReadFromDBQuery(KuaidiServiceArgs.ReadFromDBQuery query)
        {
            var sql = $"select * from KuaidiNuData where Nu=@Nu {"and Company=@Com".If(!string.IsNullOrEmpty(query.Com))}";
            var nudatas = (await _orgUnitOfWork.QueryAsync<KuaidiNuData>(sql, new { query.Nu, query.Com })).AsList();
            if (nudatas.Count > 1)
            {
                //if (!string.IsNullOrEmpty(query.Com))
                //    throw new CustomResponseException("系统繁忙", Consts.Err.Kuaidi_Has2Nu);

                nudatas.RemoveRange(1, nudatas.Count - 1);
            }
            if (nudatas.Count == 1)
            {
                var result = new KuaidiNuDataDto();
                result.Id = nudatas[0].Id;
                result.Nu = nudatas[0].Nu;
                result.UpTime = nudatas[0].UpTime;
                var srcResult = JToken.Parse(nudatas[0].Data);
                result.SrcResult = srcResult;
                result.SrcType = nudatas[0].SrcType;
                result.Errcode = result.SrcType > 0 ? 0 : -1;
                result.Items = Handle_ParseSrcResult(srcResult, result.SrcType);
                result.IsCompleted = nudatas[0].IsCompleted;
                result.CompanyName = nudatas[0].CompanyName;
                result.CompanyCode = nudatas[0].Company;
                return (result, nudatas[0].IsCompleted);
            }
            return default;
        }


        private async Task<Exception> Handle_WriteToDBCmd(KuaidiServiceArgs.WriteToDBCmd cmd)
        {
            var dbmodel = cmd.Dbmodel;
            var company = await Handle_GetCodeQuery(dbmodel.Company);
            if (company?.Code == null)
            {                
                dbmodel.IsComOk = dbmodel.Company.IsNullOrEmpty() ? (bool?)null : false;
                //throw new CustomResponseException($"接口返回的kdccode='{dbmodel.Company}'不在已知codes中", Consts.Err.Kuaidi_ComNotmatch_When_WriteToDB);
            }
            else
            {
                dbmodel.IsComOk = true;
                dbmodel.Company = company.Code;
                dbmodel.CompanyName ??= company.Com;
            }

            await using var lck = await _lock1Factory.LockAsync("org:lck2:kuaidi_sync2db", 5000);
            if (!lck.IsAvailable) return new CustomResponseException($"频繁写入运单'{dbmodel.Nu}'", Consts.Err.Kuaidi_BusyForWriteToDB);

            try
            {
                _orgUnitOfWork.BeginTransaction();

                var i = 0;
                if (cmd.PrevId != null)
                {
                    var sql = "delete from KuaidiNuData where Id=@PrevId";
                    i = await _orgUnitOfWork.ExecuteAsync(sql, new { cmd.PrevId }, _orgUnitOfWork.DbTransaction);                    
                }
                if (i < 1)
                {                    
                    //var sql = "delete from KuaidiNuData where Nu=@Nu and isnull(Company,'')=@Company";
                    var sql = "delete from KuaidiNuData where IsCompleted=0 and Nu=@Nu ";
                    await _orgUnitOfWork.ExecuteAsync(sql, new
                    {
                        dbmodel.Nu,
                        Company = company?.Code ?? "",
                    }, _orgUnitOfWork.DbTransaction);
                }
                await _orgUnitOfWork.DbConnection.InsertAsync(dbmodel, _orgUnitOfWork.DbTransaction);                
                
                _orgUnitOfWork.CommitChanges();                
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.SafeRollback();
                return ex;
            }

            return null;
        }


        private async Task<KdCompanyCodeDto> Handle_CheckNuCmd(KuaidiServiceArgs.CheckNuCmd cmd)
        {
            if (!Regex.IsMatch(cmd.Com, @"^[A-Z][A-Z0-9_]{0,}$"))
                return null;
            if (!Regex.IsMatch(cmd.Nu, @"^[A-Z]{0,4}[0-9]{5,}$"))
                return null;

            switch (cmd.Com)
            {
                
            }
            // other rules
            {
                
            }

            try
            {
                var kd = await _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery { Nu = cmd.Nu, Com = cmd.Com });
                if (kd.Errcode == 0)
                {
                    var com = kd.CompanyCode;
                    return await Handle_GetCodeQuery(com);
                }
                if (kd.Errcode.In(-1, 201))
                {
                    return null;
                }
            }
            catch 
            {
                //
            }

            return new KdCompanyCodeDto();
        }
    }
}

