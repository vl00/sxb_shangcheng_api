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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class GetUserOpenIdQryArgsHandler : IRequestHandler<GetUserOpenIdQryArgs, (string, string)>
    {
        UserUnitOfWork _userUnitOfWork;
        Openid_WXOrgUnitOfWork _openid_WXUnitOfWork;
        IMediator _mediator;
        CSRedisClient redis;        
        IMapper _mapper;
        IConfiguration _config;

        public GetUserOpenIdQryArgsHandler(IUserUnitOfWork userUnitOfWork,IOpenid_WXUnitOfWork openid_WXUnitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config,
            IMapper mapper)
        {
            this._openid_WXUnitOfWork = (Openid_WXOrgUnitOfWork)openid_WXUnitOfWork;
            this._mediator = mediator;
            this.redis = redis;            
            this._mapper = mapper;
            this._config = config;
            this._userUnitOfWork = (UserUnitOfWork)userUnitOfWork;
        }

        public async Task<(string,string)> Handle(GetUserOpenIdQryArgs query, CancellationToken cancellation)
        {
            var sqlUser = @"select Mobile from [dbo].[userInfo] where Id=@UserId";
            var mobolie = await _userUnitOfWork.QueryFirstOrDefaultAsync<string>(sqlUser, new { query.UserId });

            var sql = @"select openID from [dbo].[openid_weixin] where valid=1 and userID=@UserId"; 
            var openid = await _openid_WXUnitOfWork.QueryFirstOrDefaultAsync<string>(sql, new { query.UserId });
            return (mobolie, openid);
        }

    }
}
