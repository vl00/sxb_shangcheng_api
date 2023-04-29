using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class MyEvaluationPageQueryHandler : IRequestHandler<MyEvaluationPageQuery, LoadMoreResult<MyEvaluationItemDto>>
    {
        OrgUnitOfWork unitOfWork;        
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;
        IMediator mediator;
        public MyEvaluationPageQueryHandler(IMediator mediator,IOrgUnitOfWork unitOfWork, CSRedisClient redis, IUserInfo me,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
            this.mediator = mediator;
        }

        public async Task<LoadMoreResult<MyEvaluationItemDto>> Handle(MyEvaluationPageQuery req, CancellationToken cancellation)
        {
            var see_pepole = null != req.SeeUserId && Guid.Empty != req.SeeUserId;
                var pg = new LoadMoreResult<MyEvaluationItemDto>();
            var sql = @"
select count(1) from Evaluation evlt where evlt.IsValid=1 and evlt.status=1 and evlt.userid=@UserId

select evlt.Id,evlt.No as Id_s,evlt.title,evlt.stick,evlt.isplaintext,evlt.cover,evlt.userid as AuthorId,evlt.CreateTime,c.content,
evlt.CollectionCount,evlt.CommentCount,evlt.likes as LikeCount,evlt.ViewCount
from Evaluation evlt
LEFT JOIN (select evlt.id,string_agg(item.content,char(10)) WITHIN GROUP(ORDER BY item.type) as content
from Evaluation evlt
join EvaluationItem item on item.evaluationid=evlt.id
where evlt.IsValid=1 and evlt.status=1 and item.IsValid=1 and evlt.userid=@UserId
GROUP BY evlt.id )c on c.id=evlt.id
where evlt.IsValid=1 and evlt.status=1 and evlt.userid=@UserId
order by evlt.CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var dyp = new DynamicParameters(req);
            if (see_pepole)
                dyp.Set("UserId", req.SeeUserId);
            else
            {
                if (!me.IsAuthenticated) throw new CustomResponseException("未登录", ResponseCode.NoLogin.ToInt());
                dyp.Set("UserId", me.UserId);
            }
            var gr = await unitOfWork.QueryMultipleAsync(sql, dyp);
            var total = await gr.ReadFirstAsync<int>();
            pg.TotalPageCount = (int)Math.Ceiling(total / (double)req.PageSize);
            pg.CurrPageIndex = req.PageIndex;
            pg.PageSize = req.PageSize;
            var items = await gr.ReadAsync<MyEvaluationItemDto>();
            foreach (var item in items)
            {
                item.Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(item.Id_s));
                if(!see_pepole)
                item.AuthorName = me.UserName;                
            }
            if (see_pepole)
            {

                // 查用户信息
                var uInfos = await mediator.Send(new UserSimpleInfoQuery
                {
                    UserIds = items.Select(_ => _.AuthorId)
                });
                foreach (var u in uInfos)
                {
                    foreach (var u0 in items.Where(_ => _.AuthorId == u.Id))
                    {
                        u0.AuthorName = u.Nickname;
                     
                    }
                }
            }



            pg.CurrItems = items;
            return pg;
        }

        
    }
}
