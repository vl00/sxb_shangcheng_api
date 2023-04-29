using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
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
    public class SpecialReqQueryHandler : IRequestHandler<SpecialReqQuery, SpecialResEntity>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;

        public SpecialReqQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IUserInfo me,            
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
        }

        public async Task<SpecialResEntity> Handle(SpecialReqQuery req, CancellationToken cancellation)
        {
            var res = new SpecialResEntity();
            await GetA_Special(req, res);
            var pg = await mediator.Send(new SpecialLoadMoreEvaluationsQuery 
            {
                PageIndex = 1,
                Id = res.Id,
                OrderBy = req.OrderBy,
            });
            res.Evaluations = pg.CurrItems;
            res.TotalPageCount = pg.TotalPageCount;
            
            return res;
        }

        async Task GetA_Special(SpecialReqQuery req, SpecialResEntity res)
        {
            var special = await mediator.Send(new GetSpecialInfoQuery { No = req.No });
            if (special == null) throw new CustomResponseException($"无效专题no={req.No}");
            mapper.Map<Special, SpecialResEntity>(special, res);            
        }
    }
}
