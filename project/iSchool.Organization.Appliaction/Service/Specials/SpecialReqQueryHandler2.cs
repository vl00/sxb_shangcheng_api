using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class SpecialReqQueryHandler2 : IRequestHandler<SpecialReqQuery2, SpecialResEntity2>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;

        public SpecialReqQueryHandler2(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IUserInfo me,            
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
        }

        public async Task<SpecialResEntity2> Handle(SpecialReqQuery2 req, CancellationToken cancellation)
        {
            var res = new SpecialResEntity2();
            await GetA_Special(req, res);
            req.SpecialType = res.SpecialType;
            #region get smallspecials by  bigspecial
            if (!Enum.IsDefined(typeof(SpecialTypeEnum), (int)req.SpecialType))
            {
                throw new CustomResponseException($"无效专题类型type={req.SpecialType}");
            }

            //get smallspecials           
            long? smallShortId = null;
            Guid smallId = default;
            if (req.SpecialType == SpecialTypeEnum.BigSpecial.ToInt())
            {
                res.SmallSpecialItems = await mediator.Send(new GetSpecialsQuery() { No = req.No, SpecialId = res.Id });

                //set  SmallId
                if (req.SmallShortId != null)
                    smallId = res.SmallSpecialItems.FirstOrDefault(_ => _.Id_s == UrlShortIdUtil.Long2Base32((long)req.SmallShortId)).Id;
            }                
            #endregion

            var pg = await mediator.Send(new SpecialLoadMoreEvaluationsQuery
            {
                PageIndex = req.PageIndex,
                Id = res.Id,
                OrderBy = req.OrderBy,
                SpecialType = req.SpecialType,
                SmallId=smallId
                
            });
            res.PageInfo = pg;
            return res;
        }

        async Task GetA_Special(SpecialReqQuery2 req, SpecialResEntity2 res)
        {
            var special = await mediator.Send(new GetSpecialInfoQuery { No = req.No });
            if (special == null) throw new CustomResponseException($"无效专题no={req.No}");
            mapper.Map<Special, SpecialResEntity2>(special, res);            
        }
    }
}
