using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
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
    public class MyCollectEvaluationQueryHandler : IRequestHandler<MyCollectEvaluationQuery, EvaluationLoadMoreQueryResult>
    {
        IMediator mediator;
        OrgUnitOfWork unitOfWork;        
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;        

        public MyCollectEvaluationQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redis, IUserInfo me,
            IMapper mapper, IMediator mediator)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
            this.mediator = mediator;
        }

        public async Task<EvaluationLoadMoreQueryResult> Handle(MyCollectEvaluationQuery req, CancellationToken cancellation)
        {
            //暂未放入redis
            var pg = new EvaluationLoadMoreQueryResult();
            pg.CurrPageIndex = req.PageIndex;
            IEnumerable<EvaluationItemDto> items = null;
         
            var sql = @"
select count(1) from Collection  c join Evaluation e on c.dataID=e.id  where c.userid=@userid and e.IsValid=1 and c.IsValid=1  and e.status=1 and c.dataType=2

select e.userid as AuthorId,e.no as id_s,e.* from Collection c join Evaluation e on c.dataID=e.id
where c.userid=@userid  and c.IsValid=1 and e.IsValid=1 and e.status=1 and c.dataType=2
order by c.CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var dyp = new DynamicParameters(req)
                .Set("userid", me.UserId);
            var gr = await unitOfWork.QueryMultipleAsync(sql, dyp);
            var total = await gr.ReadFirstAsync<int>();
            pg.TotalPageCount = (int)Math.Ceiling(total / (double)req.PageSize);
            pg.CurrPageIndex = req.PageIndex;
            pg.PageSize = req.PageSize;
            items = await gr.ReadAsync<EvaluationItemDto>();
            foreach (var item in items)
            {
                item.Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(item.Id_s));
             
            }
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
                    u0.AuthorHeadImg = u.HeadImgUrl;
                }
            }
            foreach (var u in uInfos)
            {
                foreach (var u0 in items.Where(_ => _.AuthorId == u.Id))
                {
                    u0.AuthorName = u.Nickname;
                    u0.AuthorHeadImg = u.HeadImgUrl;
                }
            }
            pg.CurrItems = items;
            return pg;

        }
     


    }
}
