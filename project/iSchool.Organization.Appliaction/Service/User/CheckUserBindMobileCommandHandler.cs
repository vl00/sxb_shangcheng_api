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
    public class CheckUserBindMobileCommandHandler : IRequestHandler<CheckUserBindMobileCommand, bool>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        UserUnitOfWork userUnitOfWork;

        public CheckUserBindMobileCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IServiceProvider services,
            IUserInfo me, IUserUnitOfWork userUnitOfWork)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.userUnitOfWork = (UserUnitOfWork)userUnitOfWork;
        }

        public async Task<bool> Handle(CheckUserBindMobileCommand cmd, CancellationToken cancellation)
        {
            if (!me.IsAuthenticated) throw new CustomResponseException("未登录", ResponseCode.NoLogin.ToInt());

            // 貌似cookie里有非完整的用户手机号
            // so..
            if (!string.IsNullOrEmpty(me.Mobile)) return true;

            // 查虎叔叔他库
            //
            var sql = "select top 1 1 from [userinfo] where id=@UserId and mobile is not null";
            var i = await userUnitOfWork.DbConnection.ExecuteScalarAsync<int?>(sql, new { me.UserId });
            if (i == 1) return true;

            if (cmd.ThrowIfNoBind) throw new CustomResponseException(EnumUtil.GetDesc(ResponseCode.NotBindMobile), ResponseCode.NotBindMobile.ToInt());
            return false;
        }

    }
}
