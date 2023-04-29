using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class UserSelectEvltVoteCommandHandler : IRequestHandler<UserSelectEvltVoteCommand, IEnumerable<UserSelectEvltVoteResult>>
    {
        OrgUnitOfWork unitOfWork;
        IUserInfo me;
        IMediator mediator;
        CSRedisClient redis;
        BussTknOption bussTkn;

        public UserSelectEvltVoteCommandHandler(IOrgUnitOfWork unitOfWork, IUserInfo me, IMediator mediator, IOptionsSnapshot<BussTknOption> option,
            CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            this.mediator = mediator;
            bussTkn = option.Get(string.Empty);
            this.redis = redis;
        }

        public async Task<IEnumerable<UserSelectEvltVoteResult>> Handle(UserSelectEvltVoteCommand cmd, CancellationToken cancellation)
        {
            if (!TokenHelper.ValidStokenByJwt(bussTkn.Key, bussTkn.Alg, bussTkn.Exp, cmd.Token, cmd.VoteItemId, cmd.VoteId, cmd.EvltId))
                throw new CustomResponseException("非法操作");
            if (!me.IsAuthenticated)
                throw new CustomResponseException("非法操作");
            await default(ValueTask);

            //// 需要判断是否绑定了手机号
            //await mediator.Send(new CheckUserBindMobileCommand { ThrowIfNoBind = true });

            var now = DateTime.Now;

            // 检测投票是否过期
            EvaluationVote vote = null;
            do
            {
                var rdk = CacheKeys.Evlt.FormatWith(cmd.EvltId);
                var rdk1 = "vote";                
                var str_vote = await redis.HGetAsync(rdk, rdk1);
                if (str_vote != null)
                {
                    if (str_vote == "{}" || str_vote == "") vote = null;
                    vote = str_vote.ToObject<EvaluationVote>();
                }
                else
                {
                    var sql = @"select * from EvaluationVote vote where vote.IsValid=1 and vote.id=@VoteId";
                    vote = await unitOfWork.QueryFirstOrDefaultAsync<EvaluationVote>(sql, new { cmd.VoteId });
                    // dot't set to redis
                }
                if (vote == null)
                    throw new CustomResponseException("投票无效");
                if (vote.Endtime == null)
                    break;
                if (vote.Endtime.Value <= now)
                    throw new CustomResponseException("投票已过期");
            }
            while (false);

            // 检测是否我投过
            do
            {
                var rdk = CacheKeys.MyEvltVote.FormatWith(
                    ("userid", me.UserId), 
                    ("evltId", cmd.EvltId));

                var sel = await redis.HGetAsync<int?>(rdk, $"{cmd.VoteId}_{cmd.VoteItemId}");
                if (sel > 0) throw new CustomResponseException("已投票");                
                
                var i = await redis.StartPipe()
                    .IncrBy(CacheKeys.IamVoting.FormatWith(("userid", me.UserId), ("voteId", cmd.VoteId)))
                    .Expire(CacheKeys.IamVoting.FormatWith(("userid", me.UserId), ("voteId", cmd.VoteId)), 3)
                    .EndPipeAsync();

                if (vote.Type != 1 && (long)i[0] > 1L) throw new CustomResponseException("已在投票中...");
            }
            while (false);

            // update
            {
                int i;
                try
                {
                    unitOfWork.BeginTransaction();
                    var sql = $@"
if {"not exists(select 1 from EvaluationVoteSelect where isvalid=1 and voteId=@VoteId and voteItemId=@VoteItemId and userid=@UserId)".If(vote.Type != 1)}
{"not exists(select 1 from EvaluationVoteSelect where isvalid=1 and voteId=@VoteId and userid=@UserId)".If(vote.Type == 1)}
begin
insert EvaluationVoteSelect(id,voteid,voteItemId,userid,CreateTime,IsValid)
    values(newid(),@VoteId,@VoteItemId,@UserId,@now,1);
update EvaluationVoteItems set count=count+1 where IsValid=1 and id=@VoteItemId ;
select 1;
end else begin
select 0
end";
                    i = await unitOfWork.DbConnection.ExecuteScalarAsync<int>(sql, 
                        new DynamicParameters(cmd)
                            .Set(nameof(me.UserId), me.UserId)
                            .Set(nameof(now), now),
                        unitOfWork.DbTransaction); 
                    unitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    try { unitOfWork.Rollback(); } catch { }
                    throw ex;
                }
                if (i < 1)
                {
                    throw new CustomResponseException("已投票");
                }

                await redis.StartPipe()
                    .Del(CacheKeys.EvltVote.FormatWith(cmd.VoteId))
                    .Del(CacheKeys.MyEvltVote.FormatWith(("userid", me.UserId), ("evltId", cmd.EvltId)))
                    .EndPipeAsync();
            }

            var dto = await mediator.Send(new EvltDetailQuery { EvltId = cmd.EvltId, AllowRecordPV = false });
            return dto.Vote == null ? Enumerable.Empty<UserSelectEvltVoteResult>()
                : dto.Vote.Items.Select(_ => new UserSelectEvltVoteResult { Id = _.Id, Count = _.Count ?? 0 });
        }
    }
}
