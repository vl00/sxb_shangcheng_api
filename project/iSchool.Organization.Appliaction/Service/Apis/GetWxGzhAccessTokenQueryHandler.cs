using CSRedis;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.wx;
using iSchool.Organization.Domain;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetWxGzhAccessTokenQueryHandler : IRequestHandler<GetWxGzhAccessTokenQuery, AccessTokenApiData>
    {
        IHttpClientFactory httpClientFactory;
        IConfiguration config;

        public GetWxGzhAccessTokenQueryHandler(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;
        }

        public async Task<AccessTokenApiData> Handle(GetWxGzhAccessTokenQuery query, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(query.GzhAppName)) return null;
            query.AccessTokenApiUrl = !string.IsNullOrEmpty(query.AccessTokenApiUrl) ? query.AccessTokenApiUrl : config["AppSettings:AccessTokenApi"];

            AccessTokenApiResult r_accesstoken = null;

            using var http = httpClientFactory.CreateClient(string.Empty);
            var res_accesstoken = await http.GetAsync(query.AccessTokenApiUrl.FormatWith(query.GzhAppName));
            try
            {
                res_accesstoken.EnsureSuccessStatusCode();
                r_accesstoken = (await res_accesstoken.Content.ReadAsStringAsync()).ToObject<AccessTokenApiResult>();
            }
            catch (Exception ex)
            {
                throw new CustomResponseException("获取AccessToken意外失败\n" + ex.Message);
            }
            if (r_accesstoken.Success) return r_accesstoken.Data;
            else throw new CustomResponseException("获取AccessToken意外失败.");            
        }

        
    }
}
