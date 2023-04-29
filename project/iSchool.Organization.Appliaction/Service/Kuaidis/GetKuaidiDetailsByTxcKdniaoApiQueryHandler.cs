using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.BgServices;
using iSchool.Domain.Modles;
using iSchool.Infras;
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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetKuaidiDetailsByTxcKdniaoApiQueryHandler : IRequestHandler<GetKuaidiDetailsByTxcKdniaoApiQuery, KuaidiNuDataDto>
    {
        IUserInfo _me;
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;
        IMediator _mediator;
        NLog.ILogger _log;
        CSRedisClient _redis;
        OrgUnitOfWork _orgUnitOfWork;
        RabbitMQConnectionForPublish _rabbit;
        ILock1Factory _lock1Factory;

        public GetKuaidiDetailsByTxcKdniaoApiQueryHandler(IHttpClientFactory httpClientFactory, IConfiguration config, IUserInfo me,
            IMediator mediator, CSRedisClient redis, RabbitMQConnectionForPublish rabbit, ILock1Factory lock1Factory,
            IOrgUnitOfWork orgUnitOfWork,
            IServiceProvider services)
        {
            this._me = me;
            this._httpClientFactory = httpClientFactory;
            this._config = config;
            this._mediator = mediator;
            this._log = services.GetService<NLog.ILogger>();
            this._redis = redis;
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._rabbit = rabbit;
            this._lock1Factory = lock1Factory;
        }

        public async Task<KuaidiNuDataDto> Handle(GetKuaidiDetailsByTxcKdniaoApiQuery query, CancellationToken cancellation)
        {
            if (query.Nu.IsNullOrEmpty())
            {
                throw new CustomResponseException("快递单号不能为空");
            }
            query.Customer = string.IsNullOrEmpty(query.Customer) ? null : query.Customer;

            var com = (await _mediator.Send(KuaidiServiceArgs.GetCode(query.Com))).GetResult<KdCompanyCodeDto>();
            if (com?.Code == null && !query.Com.IsNullOrEmpty())
            {
                throw new CustomResponseException($"暂无该快递公司'{query.Com}'的轨迹", Consts.Err.Kuaidi_ComArgsNotmatch);
            }

            // nu
            var nu = query.Nu.StartsWith("SF") || com?.Code == "SF" ? $"{query.Nu}:{(query.Customer ?? DateTime.Now.ToString("sfff"))}" : query.Nu;
            if (nu.EndsWith(':'))
            {
                throw new CustomResponseException("需要Customer参数", Consts.Err.Kuaidi_Txc17972_MissCustomerArgs);
            }

            var result = new KuaidiNuDataDto { SrcType = KuaidiApiResultSrcTypeEnum.TxcKdniao.ToInt() };
            // find in db
            if (query.ReadUseDb)
            {
                var r1 = (await _mediator.Send(KuaidiServiceArgs.ReadFromDB(query.Nu, com?.Code))).GetResult<(KuaidiNuDataDto, bool IsCompleted)>();
                if (r1.Item1 != null) result = r1.Item1;
                if (r1.IsCompleted) return result;

                // api接口有次数限制,这里要避免多次调用
                if (result?.Id != null && (DateTime.Now - result.UpTime) < TimeSpan.FromMinutes(30))
                {
                    // 受限时,普通运单尝试invoke Txc17972 api
                    var r2bd = await _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery { Nu = nu, Com = com?.Code, Customer = query.Customer, ReadUseDb = false });
                    if (r2bd.UpTime > result.UpTime) result = r2bd;
                    return result;
                }
            }

            // api接口有次数限制,这里要避免多次调用
            await using var _counter_ = new DisposableSlim<object>(null, _ => CounterTakeAsync(_redis, CacheKeys.Kuaidi_TxcKdniao_LimitedKey, -1L));
            {
                var incred = await CounterTakeAsync(_redis, CacheKeys.Kuaidi_TxcKdniao_LimitedKey, expSec: 60 * 60 * 6);
                if (incred > 50L)
                {
                    // 受限时,普通运单尝试invoke百度api
                    if (!nu.Contains(':') && com?.Code == null)
                    {
                        var r2bd = await _mediator.Send(new GetKuaidiDetailsByBaiduExprApiQuery { Nu = nu, ReadUseDb = false });
                        if (r2bd.UpTime > (result.UpTime ?? default)) result = r2bd;
                    }

                    if (result.Id == null) throw new CustomResponseException("系统繁忙", Consts.Err.Kuaidi_Api3th_Limited);
                    else return result;
                }
            }

            using var http = _httpClientFactory.CreateClient(string.Empty);

            //LB_Invoke_TxcKdniaoApi:
            Debugger.Break();
            //
            var requestType = _config["AppSettings:KuaidiApi_Txc20590kdniao:requestType"];
            var ebusinessID = _config["AppSettings:KuaidiApi_Txc20590kdniao:ebusinessID"];
            var apiKey = _config["AppSettings:KuaidiApi_Txc20590kdniao:apiKey"];
            //
            var reqJson = $"{{\"CustomerName\":\"{(query.Customer ?? "")}\",\"OrderCode\":\"\",\"ShipperCode\":\"{com?.Code}\",\"LogisticCode\":\"{query.Nu}\"}}";
            var requestData = HttpUtility.UrlEncode(reqJson, Encoding.UTF8);
            var dataSign = Convert.ToBase64String(Encoding.UTF8.GetBytes(HashAlgmUtil.Encrypt(reqJson + (apiKey ?? ""), "md5", true)));
            dataSign = HttpUtility.UrlEncode(dataSign, Encoding.UTF8);
            //
            var rr = await new HttpApiInvocation(_log)
                .SetApiDesc("腾讯云-20590-快递鸟")                
                .SetMethod(HttpMethod.Post)
                .OnBeforeRequest(req => 
                {
                    req.SetTencentCloudMarketAuths(
                        _config["AppSettings:KuaidiApi_Txc20590kdniao:secretid"], 
                        _config["AppSettings:KuaidiApi_Txc20590kdniao:secretkey"],
                        _config["AppSettings:KuaidiApi_Txc20590kdniao:source"]);
                })
                .SetUrl($"{_config["AppSettings:KuaidiApi_Txc20590kdniao:url"]}?DataType=2&DataSign={dataSign}&EBusinessID={ebusinessID}&RequestData={requestData}&RequestType={requestType}")
                .SetBodyByFormStr($"CustomerName={HttpUtility.UrlEncode(query.Customer, Encoding.UTF8)}&OrderCode=&ShipperCode={com?.Code}&LogisticCode={query.Nu}")
                .SetResBodyParser(jsonStr =>
                {
                    var jtk = JToken.Parse(jsonStr);
                    if ((bool?)(jtk["Success"] ?? jtk["success"]) == true) return ResponseResult<JToken>.Success(jtk);
                    var r = ResponseResult<JToken>.Failed((string)(jtk["Reason"] ?? jtk["reason"]));
                    r.status = (ResponseCode)(int.TryParse((string)(jtk["StateEx"] ?? jtk["State"] ?? jtk["state"]), out var _cde) ? _cde : Consts.Err.Kuaidi_OtherError);
                    r.Data = jtk;
                    return r;
                })
                .InvokeByAsync<JToken>(http);

            if (!rr.Succeed)
            {
                switch ((int)rr.status)
                {
                    default:
                    case 429: // secretid和secretkey到达调用上限
                        {
                            // 普通运单尝试invoke百度api
                            if (!nu.Contains(':') && com?.Code == null)
                            {
                                var r2bd = await _mediator.Send(new GetKuaidiDetailsByBaiduExprApiQuery { Nu = nu, ReadUseDb = false });
                                if (r2bd.UpTime > (result.UpTime ?? default)) return result = r2bd;
                            }
                        }
                        break;
                }
            }

            if (rr.Succeed)
            {
                var dbmodel = new KuaidiNuData { Id = Guid.NewGuid() };
                dbmodel.Nu = query.Nu;
                dbmodel.UpTime = DateTime.Now;
                dbmodel.SrcType = (byte)KuaidiApiResultSrcTypeEnum.TxcKdniao;
                dbmodel.Data = rr.Data.ToString();
                dbmodel.IsCompleted = ((int?)rr.Data["State"] == 3);
                dbmodel.Company = com?.Code ?? ((string)rr.Data["ShipperCode"]);
                dbmodel.CompanyName = com?.Com ?? (dbmodel.Company.IsNullOrEmpty() ? null 
                    : (await _mediator.Send(KuaidiServiceArgs.GetCode(dbmodel.Company))).GetResult<KdCompanyCodeDto>()?.Com);
                dbmodel.Customer = query.Customer;

                var items = (await _mediator.Send(KuaidiServiceArgs.ParseSrcResult(rr.Data, dbmodel.SrcType)))
                    .GetResult<IEnumerable<KuaidiNuDataItemDto>>()?.AsArray();

                dbmodel.LastJStr = items?.FirstOrDefault()?.ToJsonString(camelCase: true);

                // sync to db
                if (query.WriteUseDb)
                {
                    // will resolve 'dbmodel.Company' 
                    var ex = (await _mediator.Send(KuaidiServiceArgs.WriteToDB(dbmodel, prevId: result.Id))).GetResult<Exception>();
                    if (ex != null)
                    {
                        _log.Error(GetLogMsg(dbmodel, ex, ex is CustomResponseException crex ? crex.ErrorCode : Consts.Err.Kuaidi_ErrOnSyncToDb));
                    }
                }

                result.SrcType = dbmodel.SrcType;
                result.SrcResult = rr.Data;
                result.Nu = query.Nu;
                result.Id = dbmodel.Id;
                result.UpTime = dbmodel.UpTime;
                result.Errcode = 0;
                result.Items = items;
                result.IsCompleted = dbmodel.IsCompleted;
                result.CompanyCode = dbmodel.Company;
                result.CompanyName = dbmodel.CompanyName;
            }
            else if (result.Id == null)
            {
                // 接口不成功但db有数据时返回db数据,否则返回接口不成功数据

                result.SrcType = (byte)KuaidiApiResultSrcTypeEnum.TxcKdniao;
                var srcResult = rr.Data;
                result.SrcResult = srcResult;
                result.Nu = query.Nu;
                result.Id = null;
                result.Errcode = (int)rr.status switch { 0 => -1, _ => (int)rr.status };
                result.Errmsg = (string)(srcResult?["Reason"] ?? srcResult?["reason"]);
            }

            if (result.Errcode != 0) result.Items = null;
            else result.Items = result.Items?.AsArray();
            return result;
        }

        static async Task<long> CounterTakeAsync(CSRedisClient redis, string key, long c = 1L, int? expSec = null)
        {
            var incred = await redis.IncrByAsync(key, c);
            if (incred == 1L) _ = redis.ExpireAsync(key, expSec ?? 60);
            return incred;
        }

        NLog.LogEventInfo GetLogMsg(object paramsObj, Exception ex, int errcode)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
            msg.Properties["Caption"] = "快递查询-腾讯云-20590-快递鸟";
            msg.Properties["UserId"] = _me.UserId;
            msg.Properties["Level"] = "Error";
            if (paramsObj is string str) msg.Properties["Params"] = str;
            else if (paramsObj != null) msg.Properties["Params"] = (paramsObj).ToJsonString(camelCase: true);
            msg.Properties["Error"] = $"{ex?.Message}";
            msg.Properties["StackTrace"] = ex?.StackTrace;
            msg.Properties["ErrorCode"] = errcode;
            return msg;
        }
    }
}

