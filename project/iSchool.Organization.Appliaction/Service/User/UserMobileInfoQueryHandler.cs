using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class UserMobileInfoQueryHandler : IRequestHandler<UserMobileInfoQuery, (userinfo, userinfo[])[]>
    {        
        IMediator mediator;
        UserUnitOfWork userUnitOfWork;

        public UserMobileInfoQueryHandler(IMediator mediator,
            IUserUnitOfWork userUnitOfWork,
            IServiceProvider services)
        {            
            this.mediator = mediator;
            this.userUnitOfWork = (UserUnitOfWork)userUnitOfWork;
        }

        public async Task<(userinfo, userinfo[])[]> Handle(UserMobileInfoQuery query, CancellationToken cancellation)
        {
            var result = new (userinfo UserInfo, userinfo[] OtherUserInfo)[query.UserIds.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = (new userinfo { Id = query.UserIds[i] }, Array.Empty<userinfo>());
            }

            if (query.UserIds.Length > 0)
            {
                var sql = "select * from dbo.userinfo u where u.channel is null and u.id in @UserIds";
                var us = (await userUnitOfWork.QueryAsync<userinfo>(sql, query)).AsList();
                for (var i = 0; i < result.Length; i++)
                {
                    if (!us.TryGetOne(out var u1, (_) => _.Id == result[i].UserInfo.Id)) continue;
                    result[i] = (u1, Array.Empty<userinfo>());
                }
            }

            var mbs = result.Select(_ => (_.UserInfo.NationCode ?? 86, _.UserInfo.Mobile)).Distinct().ToArray();
            if (mbs.Length > 0)
            {
                var sql = $@"
select u.* from dbo.userinfo u where u.channel is null 
and ({string.Join(" or ", mbs.Select(_ => $"(isnull(u.nationCode,{86})={_.Item1} and u.mobile='{_.Mobile}')"))} )
-- order by (isnull(u.nationCode,{86}),u.mobile
";
                var us = (await userUnitOfWork.QueryAsync<userinfo>(sql))
                    .GroupBy(x => (NationCode: x.NationCode ?? 86, x.Mobile))
                    .ToArray();

                for (var i = 0; i < result.Length; i++)
                {
                    if (!us.TryGetOne(out var u1, (_) => _.Key.NationCode == (result[i].UserInfo.NationCode ?? 86) && _.Key.Mobile == result[i].UserInfo.Mobile)) continue;
                    result[i] = (result[i].UserInfo, u1.Where(_ => _.Id != result[i].UserInfo.Id).ToArray());
                }
            }

            return result;
        }

    }
}
