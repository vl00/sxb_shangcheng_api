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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class UserScoreOnRwInviteActivityArgsHandler : IRequestHandler<UserScoreOnRwInviteActivityArgs, UserScoreOnRwInviteActivityResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;        
        IMapper _mapper;
        IConfiguration _config;

        public UserScoreOnRwInviteActivityArgsHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<UserScoreOnRwInviteActivityResult> Handle(UserScoreOnRwInviteActivityArgs query, CancellationToken cancellation)
        {
            var result = new UserScoreOnRwInviteActivityResult();
            switch (query.Action)
            {
                case UserScoreOnRwInviteActivityArgs.PreConsumeAction _PreConsumeAction:
                    result.Result = await Handle_ConsumeAction(query, _PreConsumeAction.Score);
                    break;
                case UserScoreOnRwInviteActivityArgs.ConsumeAction _ConsumeAction:
                    result.Result = await Handle_ConsumeAction(query, _ConsumeAction.Score);
                    break;
            }
            return result;
        }

        protected async Task<double> Handle_ConsumeAction(UserScoreOnRwInviteActivityArgs query, double score)
        {
            var unionID = query.UnionID;
            if (string.IsNullOrEmpty(unionID))
            {
                var unionID_dto = await _mediator.Send(new GetUserSxbUnionIDQuery { UserId = query.UserId ?? default });
                if (unionID_dto == null)
                    throw new CustomResponseException("用户没UnionID", Consts.Err.OrderCreate_UserHasNoUnionID);

                unionID = unionID_dto.UnionID;
            }
            unionID = $"\"{unionID}\"";

            var rdk = query.CourseExchangeType switch
            {
                CourseExchangeTypeEnum.Ty1 => CacheKeys.RwInviteActivity_InviteeBuyQualify,
                CourseExchangeTypeEnum.Ty2 => CacheKeys.RwInviteActivity_InviterBonusPoint,
                _ => throw new CustomResponseException("无效的积分配置", Consts.Err.OrderCreate_CourseExchangeIsNotRwInviteActivity),
            };

            //
            //  -3: 积分未初始化
            //  -2: 积分不足
            //  -1: 不限积分
            //  大于等于0: 扣减之后剩余的积分
            //
            var lua = $@"
local score = redis.call('zscore',KEYS[1],ARGV[1])
if (score) or (score == 0) then
    score = score - ARGV[2]
	if (score < 0) then
		return -2
	end
	redis.call('zadd',KEYS[1],score,ARGV[1])
	return score
end
return -3
";
            var r = Convert.ToDouble(await _redis.EvalAsync(lua, rdk, unionID, score));
            return r;
        }

    }
}
