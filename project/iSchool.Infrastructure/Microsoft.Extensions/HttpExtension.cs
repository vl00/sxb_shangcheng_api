using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    public static class HttpExtension
    {
        public static void Set(this HttpHeaders headers, string name, params string[] values)
        {
            headers.Remove(name);
            headers.TryAddWithoutValidation(name, values);
        }

        public static HttpRequestMessage SetHttpHeader(this HttpRequestMessage req, string name, params string[] values)
        {
            Set(req.Headers, name, values);
            return req;
        }

        public static HttpRequestMessage SetContent(this HttpRequestMessage req, HttpContent content)
        {
            req.Content = content;
            return req;
        }

        /// <summary>  
        /// </summary>
        /// <returns>not null</returns>
        public static CookieCollection GetResCookies(this HttpResponseMessage res, CookieContainer cookieContainer = null)
        {
            cookieContainer ??= new CookieContainer();
            ProcessReceivedCookies(res, cookieContainer);
            return cookieContainer.GetCookies(res.RequestMessage.RequestUri);
        }

        static void ProcessReceivedCookies(HttpResponseMessage response, CookieContainer cookieContainer)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                // The header values are always a string[]
                var valuesArray = (string[])values;
                Debug.Assert(valuesArray.Length > 0, "No values for header??");
                Debug.Assert(response.RequestMessage != null && response.RequestMessage.RequestUri != null);

                Uri requestUri = response.RequestMessage.RequestUri;
                for (int i = 0; i < valuesArray.Length; i++)
                {
                    cookieContainer.SetCookies(requestUri, valuesArray[i]);
                }
            }
        }
    }
}