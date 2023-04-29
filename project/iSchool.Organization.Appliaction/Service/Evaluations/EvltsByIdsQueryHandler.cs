using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class EvltsByIdsQueryHandler : IRequestHandler<EvltsByIdsQuery, ResponseResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;  
        IMapper mapper;

        public EvltsByIdsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator,IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;  
            this.mapper = mapper;
     

        }

        public async Task<ResponseResult> Handle(EvltsByIdsQuery req, CancellationToken cancellation)
        {
            if(req.EvltIds.Any()==false)
                return ResponseResult.Failed("评测Id集不允许为空集！");
            var sql = $@"
select evlt.* from Evaluation evlt
where evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} 
and evlt.id in ('{string.Join("','",req.EvltIds)}')
";
            var qs =  unitOfWork.Query<Evaluation>(sql, null).ToList();
            if(qs?.Any()==false)
                return ResponseResult.Success("暂无数据！");

            List<EvltQueryResult> items = new List<EvltQueryResult>();
             items = qs.Select(evlt => mapper.Map<EvltQueryResult>(evlt)).ToList();
           
            // 查用户信息
            {
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
            }

            return ResponseResult.Success(items);
        }
    }
}
