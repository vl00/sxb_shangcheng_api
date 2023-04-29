using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Domain;
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
    /// get union_id
    /// </summary>
    public class GetUserSxbUnionIDQueryHandler : IRequestHandler<GetUserSxbUnionIDQuery, UserSxbUnionIDDto>
    {
        UserUnitOfWork _userUnitOfWork;
        CSRedisClient _redis;
        private readonly IConfiguration _config;

        public GetUserSxbUnionIDQueryHandler(IUserUnitOfWork unitOfWork
            , CSRedisClient redis
            , IConfiguration config)
        {
            this._userUnitOfWork = (UserUnitOfWork)unitOfWork;
            _redis = redis;
            _config = config;
        }

        public async Task<UserSxbUnionIDDto> Handle(GetUserSxbUnionIDQuery query, CancellationToken cancellation)
        {
            var sql = @"select unionID,userID,nickname from [dbo].[unionid_weixin] where valid=1 and userID=@UserId";
            var dto = await _userUnitOfWork.QueryFirstOrDefaultAsync<UserSxbUnionIDDto>(sql, new { query.UserId });
            return dto;
        }
    }
}
