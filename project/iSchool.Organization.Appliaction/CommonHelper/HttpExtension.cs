using iSchool.Infras;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    public static class FinanceCenterHttpExtension
    {
        /// <summary>
        /// 支付中心签名header
        /// </summary>
        /// <returns></returns>
        public static HttpRequestMessage SetFinanceSignHeader(this HttpRequestMessage req, string paykey, string body, string system)
        {
            var timespan = DateTime.Now.ToString("yyyyMMddHHmmss");
            var nonce = Guid.NewGuid().ToString("N");
            req.SetHttpHeader("sxb.timespan", timespan);
            req.SetHttpHeader("sxb.nonce", nonce);
            req.SetHttpHeader("sxb.key", system);
            var sign = $"{paykey}{timespan}\n{nonce}\n{body}\n";
            req.SetHttpHeader("sxb.sign", HashAlgmUtil.Encrypt(sign, "md5", false));
            return req;
        }

        public static IDictionary<string, string> SetFinanceSignHeader(this IDictionary<string, string> headers, string paykey, string body, string system)
        {
            var timespan = DateTime.Now.ToString("yyyyMMddHHmmss");
            var nonce = Guid.NewGuid().ToString("N");
            headers["sxb.timespan"] = timespan;
            headers["sxb.nonce"] = nonce;
            headers["sxb.key"] = system;
            var sign = $"{paykey}{timespan}\n{nonce}\n{body}\n";
            headers["sxb.sign"] = HashAlgmUtil.Encrypt(sign, "md5", false);
            return headers;
        }

        public static IDictionary<string, string> SetHeader(this IDictionary<string, string> headers, string name, string value)
        {
            headers[name] = value;
            return headers;
        }
    }

    public static class Tencent_Cloud_HttpExtension
    {
        public static string SetTencentCloudMarketAuths(this HttpRequestMessage req, string secretId, string secretKey, string source = null)
        {
            source ??= "market";
            var dt = DateTime.UtcNow.GetDateTimeFormats('r')[0];
            var signStr = "x-date: " + dt + "\n" + "x-source: " + source;
            var sign = HMACSHA1Text(signStr, secretKey);
            var auth = $"hmac id=\"{secretId}\", algorithm=\"hmac-sha1\", headers=\"x-date x-source\", signature=\"{sign}\"";
            req.SetHttpHeader("Authorization", auth);
            req.SetHttpHeader("X-Source", source);
            req.SetHttpHeader("X-Date", dt);
            return dt;
        }

        static string HMACSHA1Text(string encryptText, string encryptKey)
        {
            using var hmacsha1 = new System.Security.Cryptography.HMACSHA1();
            hmacsha1.Key = Encoding.UTF8.GetBytes(encryptKey);
            var dataBuffer = Encoding.UTF8.GetBytes(encryptText);
            var hashBytes = hmacsha1.ComputeHash(dataBuffer);
            return Convert.ToBase64String(hashBytes);
        }
    }
}