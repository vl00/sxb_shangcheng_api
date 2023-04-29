using iSchool.Organization.Appliaction.Service.PointsMall.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.PointsMall
{
    public class PointsMallService : IPointsMallService
    {
        HttpClient _client;
        ILogger<PointsMallService> _logger;
        public PointsMallService(HttpClient client, IOptions<PointsMallOptions> options, ILogger<PointsMallService> logger)
        {
            if (options.Value == null) throw new ArgumentNullException("pointsmallservice need config.");
            client.BaseAddress = new Uri(options.Value.BaseUrl);
            if (!client.DefaultRequestHeaders.TryAddWithoutValidation("sxb-innerToken", options.Value.InnerToken))
                throw new ArgumentNullException("pointsmallservice innerToken config error.");
            _client = client;
            _logger = logger;
        }

        public async Task<bool> AddAccountPoints(Guid userId, long points, string originId, string remark, int originType)
        {
            string url = _client.BaseAddress  + "/api/AccountPoints/AddAccountPoints";
            var obj = new { userId, points, originId, remark, originType };
            StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(responseContent);
            var succeed = jobj["succeed"].Value<bool>();
            if (succeed)
            {
                return true;
            }
            else
            {
                string msg = jobj["msg"].Value<string>();
                _logger.LogInformation("加/减积分失败。msg = {msg}", msg);
                return false;
            }
        }

        public async Task<bool> DeductFreezePoints(Guid freezeId, Guid userId,int originType)
        {
            string url = _client.BaseAddress + "/api/AccountPoints/DeductFreezePoints";
            var obj = new { freezeId, userId , originType };
            StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(responseContent);
            var succeed = jobj["succeed"].Value<bool>();
            if (succeed)
            {
                return true;
            }
            else
            {
                string msg = jobj["msg"].Value<string>();
                _logger.LogInformation("扣除冻结积分失败。msg = {msg}", msg);
                return false;
            }
        }

        public async Task<bool> DeFreezePoints(Guid freezeId, Guid userId)
        {
            string url = _client.BaseAddress + "/api/AccountPoints/DeFreezePoints";
            var obj = new { freezeId, userId };
            StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(responseContent);
            var succeed =  jobj["succeed"].Value<bool>();
            if (succeed)
            {
                return true;
            }
            else {
                string msg = jobj["msg"].Value<string>();
                _logger.LogInformation("解冻积分失败。msg = {msg}", msg);
                return false;
            }
        }

        public async Task<Guid> FreezePoints(FreezePointsRequest request)
        {
            string url = _client.BaseAddress + "/api/AccountPoints/FreezePoints";
            StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(responseContent);
            bool succeed =  jobj["succeed"].Value<bool>();
            if (succeed)
            {
                string freezeId = jobj["data"]["freezeId"].Value<string>();
                return Guid.Parse(freezeId);
            }
            else
            {
                string msg = jobj["msg"].Value<string>();
                throw new Exception(msg);
            }
        }

        public async Task<Guid> AddFreezePoints(FreezePointsRequest request)
        {
            string url = _client.BaseAddress + "/api/AccountPoints/AddFreezePoints";
            StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(responseContent);
            bool succeed = jobj["succeed"].Value<bool>();
            if (succeed)
            {
                string freezeId = jobj["data"]["freezeId"].Value<string>();
                return Guid.Parse(freezeId);
            }
            else
            {
                string msg = jobj["msg"].Value<string>();
                throw new Exception(msg);
            }
        }
    }
}
