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
    public class PcEvltCommentsQueryHandler : IRequestHandler<PcEvltCommentsQuery, PagedList<PcEvaluationCommentDto>>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IConfiguration config;
        IMapper mapper;

        public PcEvltCommentsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config, IUserInfo me, 
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.config = config;
            this.mapper = mapper;
        }

        public async Task<PagedList<PcEvaluationCommentDto>> Handle(PcEvltCommentsQuery query, CancellationToken cancellation)
        {
            var result = new PagedList<PcEvaluationCommentDto>();
            result.CurrentPageIndex = query.PageIndex;
            result.PageSize = query.PageSize;
            await default(ValueTask);

            var pg0 = await mediator.Send(new EvltCommentsQuery
            {
                EvltId = query.EvltId,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
                Naf = query.Naf,
                AllowFindChilds = false,
            });
            result.TotalItemCount = pg0.TotalItemCount;
            result.CurrentPageItems = pg0.CurrentPageItems.Select(_ => mapper.Map<PcEvaluationCommentDto>(_)).AsArray();
            // get commentItem.SubComments
            await GetChildren(query.EvltId, result.CurrentPageItems);

            return result;
        }

        async Task GetChildren(Guid evltId, IEnumerable<PcEvaluationCommentDto> lsU)
        {
            if (lsU?.Any() != true) return;

            var sql = $@"select * from(
select row_number()over(partition by fromid,userid order by CreateTime desc)as _ii,Id,UserId,Username,Comment,CreateTime,fromid
from EvaluationComment where IsValid=1 
and({string.Join(" or ", lsU.Select(x => $"(fromid='{x.Id}' and userid='{x.AuthorId}')"))}) 
)T where _ii=1
";
            var subModels = await unitOfWork.QueryAsync<PcSubCommentDto, Guid, (PcSubCommentDto, Guid)>(
                sql, splitOn: "fromid", map: (_0, _1) => (_0, _1));
            var ls_subs = new List<PcSubCommentDto>(20);
            foreach (var u in lsU)
            {
                u.SubComments = null;
                if (!subModels.TryGetOne(out var subModel, (_) => _.Item2 == u.Id && _.Item1.UserId == u.AuthorId)) continue;
                subModel.Item1.IsAuthor = true;
                subModel.Item1.IsMy = subModel.Item1.UserId == me.UserId;
                u.SubComments = new[] { subModel.Item1 };
                ls_subs.Add(subModel.Item1);
            }

            // user info
            {
                var rr = await mediator.Send(new UserSimpleInfoQuery { UserIds = ls_subs.Select(_ => _.UserId).Distinct() });
                foreach (var item in ls_subs)
                {
                    var r = rr.FirstOrDefault(_ => _.Id == item.UserId);
                    if (r == null) continue;
                    item.Username = r.Nickname;
                    item.UserImg = r.HeadImgUrl ?? config["AppSettings:UserDefaultHeadImg"];
                }
            }

            // like count + IsLikeByMe
            {
                var lks = await mediator.Send(new EvltCommentLikesQuery { Ids = ls_subs.Select(_ => (evltId, _.Id)).ToArray() });
                foreach (var item in ls_subs)
                {
                    if (!lks.Items.TryGetValue((evltId, item.Id), out var v)) continue;
                    item.Likes = v.Likecount;
                    item.IsLikeByMe = v.IsLikeByMe;
                }
            }

        }
    }
}
