using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    /// <summary>
    /// 用户列表
    /// </summary>
    public class UserInfosByAPICommandHandler : IRequestHandler<UserInfosByAPICommand, List<userinfo>>
    {
        OrgUnitOfWork _unitOfWork;
        IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public UserInfosByAPICommandHandler(IOrgUnitOfWork unitOfWork
            , IHttpClientFactory httpClientFactory
            , IConfiguration config)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<List<userinfo>> Handle(UserInfosByAPICommand query, CancellationToken cancellation)
        {
            
            using var httpClient = _httpClientFactory.CreateClient(string.Empty);
            var userIds = query.UserIds;
            var url = $"{_config["UserCenter:GetBatchUserInfosBaseUrl"]}/User/GetUsers";
            var req = await httpClient.PostAsync(url, new StringContent(userIds.ToJsonString(), Encoding.UTF8, "application/json"));
            req.EnsureSuccessStatusCode();
            var userInfos = (await req.Content.ReadAsStringAsync()).ToObject<UserInfoFromAPI>(true);
            return userInfos.data;
        }

    }

    public class UserInfoFromAPI
    {
        public bool succeed { get; set; }
        public string msgTime { get; set; }
        public int status { get; set; }
        public string msg { get; set; }

        public List<userinfo> data { get; set; }
    }
}
