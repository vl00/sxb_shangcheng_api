using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    public static class HttpHelper
    {

        public static T Get<T>(string url)
        {

            try
            {
                var request = WebRequest.Create(url);
                if (request != null)
                {
                    var response = request.GetResponse() as HttpWebResponse;
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader myStreamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")))
                        {
                            string retString = myStreamReader.ReadToEnd();
                            var rb = JsonConvert.DeserializeObject<T>(retString);
                            return rb;

                        }
                    }
                }
                return default(T);
            }
            catch (Exception ex)
            {
                return default(T);
            }

        }

        /// <summary>
        /// 发起POST同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="token"></param>
        /// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        /// <param name="headers">填充消息头</param>        
        /// <returns></returns>
        public static string HttpPostWithHttps(string url, string postData = null, string token = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null)
        {
            postData = postData ?? "";
            var client = url.StartsWith("https") ? new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true }) : new HttpClient();
            using (client)
            {
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                if (headers != null)
                {
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                using (HttpContent httpContent = new StringContent(postData, Encoding.UTF8))
                {
                    if (contentType != null)
                        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    HttpResponseMessage response = client.PostAsync(url, httpContent).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
            }
        }


    }
}
