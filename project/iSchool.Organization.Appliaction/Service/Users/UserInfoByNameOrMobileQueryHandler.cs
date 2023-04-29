using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using iSchool.Infrastructure;

namespace iSchool.Organization.Appliaction.Services
{
    public class UserInfoByNameOrMobileQueryHandler : IRequestHandler<UserInfoByNameOrMobileQuery, List<UserInfoByNameOrMobileResponse>>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public UserInfoByNameOrMobileQueryHandler(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<List<UserInfoByNameOrMobileResponse>> Handle(UserInfoByNameOrMobileQuery request, CancellationToken cancellationToken)
        {
            var url = _config["UserCenter:GetBatchUserInfosBaseUrl"];

            var list = new List<UserInfoByNameOrMobileResponse>();

            using (var httpClient = _httpClientFactory.CreateClient(string.Empty))
            {
                if (!string.IsNullOrEmpty(request.Mobile))
                {
                    var req = await httpClient.GetAsync($"{url}/User/Phone/{request.Mobile}");
                    req.EnsureSuccessStatusCode();
                    var userInfo1 = (await req.Content.ReadAsStringAsync()).ToObject<UserInfoQuery>(true);
                    if (userInfo1.succeed)
                    {
                        list.AddRange(userInfo1.data);
                    }
                }
                if (!string.IsNullOrEmpty(request.Name))
                {
                    var req2 = await httpClient.GetAsync($"{url}/User/Name/{request.Name}");
                    req2.EnsureSuccessStatusCode();
                    var userInfo2 = (await req2.Content.ReadAsStringAsync()).ToObject<UserInfoQuery>(true);
                    if (userInfo2.succeed)
                    {
                        list.AddRange(userInfo2.data);
                    }
                }
            }
            return list.Distinct().ToList();
        }

    }


    public class UserInfoQuery
    {
        public bool succeed { get; set; }
        public string msgTime { get; set; }
        public int status { get; set; }
        public string msg { get; set; }
        public List<UserInfoByNameOrMobileResponse> data { get; set; }
    }
}
