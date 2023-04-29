using CSRedis;
using iSchool.Domain.Modles;
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
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class SensitiveKeywordCmdHandler : IRequestHandler<SensitiveKeywordCmd, SensitiveKeywordCmdResult>
    {
        IHttpClientFactory httpClientFactory;
        IConfiguration config;
        IUserInfo me;
        NLog.ILogger log;

        public SensitiveKeywordCmdHandler(IHttpClientFactory httpClientFactory, IConfiguration config, 
            IUserInfo me,
            IServiceProvider services)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;
            this.me = me;
            this.log = services.GetService<NLog.ILogger>();
        }

        public async Task<SensitiveKeywordCmdResult> Handle(SensitiveKeywordCmd q, CancellationToken cancellation)
        {
            const string strjon = "\r\n|\r\n";

            var txt = q.Txt.IsNullOrEmpty() && q.Txts?.Length < 1 ? null :
                !q.Txt.IsNullOrEmpty() && q.Txts?.Length > 0 ? throw new CustomResponseException("参数错误", 201) :
                !q.Txt.IsNullOrEmpty() ? q.Txt : string.Join(strjon, q.Txts);

            if (txt.IsNullOrEmpty())
            {
                return new SensitiveKeywordCmdResult { Pass = true };
            }            

            var msg = new NLog.LogEventInfo();
            msg.Properties["UserId"] = me.UserId;
            msg.Properties["Level"] = "错误";
            msg.Properties["Content"] = "检查敏感词错误";

            using var http = httpClientFactory.CreateClient(string.Empty);
            var r = await new HttpApiInvocation(log, msg).SetApiDesc("检查敏感词")
                .SetMethod(HttpMethod.Post).SetUrl($"{config["AppSettings:api9999Url"]}/text/greentextcheck")
                .SetHeader("X-Requested-With", "XMLHttpRequest")
                .SetBodyByJson(new { keywords = txt })
                .InvokeByAsync(http);

            if (!r.Succeed)
            {
                return new SensitiveKeywordCmdResult { Pass = true };
            }

            var jtk = r.Data;
            var result = jtk.ToObject<SensitiveKeywordCmdResult>();
            result.SrcData = jtk;
            if (!result.Pass)
            {                
                var reason = (string)jtk.SelectToken("result[0].results[0].label");
                var src = (string)jtk.SelectToken("result[0].content");
                var filteredCtn = (string)jtk.SelectToken("result[0].filteredContent");

                msg.Properties["Time"] = DateTime.Now.ToMillisecondString();                      
                msg.Properties["Error"] = $"内容包含敏感词.reason='{reason}',src='{src}'.";
                msg.Properties["ErrorCode"] = ResponseCode.GarbageContent.ToInt();
                log?.Error(msg);

                result.NotpassMessage = "内容包含敏感词";

                if (!string.IsNullOrEmpty(filteredCtn))
                {
                    if (!q.Txt.IsNullOrEmpty()) result.FilteredTxt = filteredCtn;
                    else
                    {                        
                        var filteredCtns = filteredCtn.Split(strjon);
                        var txts = new string[q.Txts.Length];
                        Debug.Assert(q.Txts.Length <= filteredCtns.Length);
                        for (int i = 0, j = 0; i < q.Txts.Length; i++)
                        {
                            var qtxt = q.Txts[i];
                            var c = FindCount(qtxt, strjon) + 1;
                            txts[i] = string.Join(strjon, filteredCtns[j..(j + c)]);
                            j += c;
                        }
                        result.FilteredTxts = txts;
                    }
                }
            }
            return result;
        }

        static int FindCount(string str, in ReadOnlySpan<char> s)
        {
            var src = str.AsSpan();
            var cc = 0;
            while (src.Length > 0 && src.IndexOf(s) is int i && i > -1)
            {
                cc++;
                src = src[(i + s.Length)..];
            }
            return cc;
        }
    }
}

