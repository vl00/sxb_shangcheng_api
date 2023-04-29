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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class GetHeaderFxTeamSetupQueryHandler : IRequestHandler<GetHeaderFxTeamSetupQuery, HeaderFxTeamSetupInfoDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;        
        IMapper _mapper;
        IConfiguration _config;

        public GetHeaderFxTeamSetupQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;            
            this._redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<HeaderFxTeamSetupInfoDto> Handle(GetHeaderFxTeamSetupQuery query, CancellationToken cancellation)
        {
            var result = new HeaderFxTeamSetupInfoDto { };
            await default(ValueTask);

            result.Condition1ConsumedMoneys = Convert.ToInt32(_config["AppSettings:drpfx_upgrade2headfx:condition1:consumedMoneys"]);
            result.Condition2EvltCount = Convert.ToInt32(_config["AppSettings:drpfx_upgrade2headfx:condition2:evltCount"]);
            result.Condition2StickEvltCount = Convert.ToInt32(_config["AppSettings:drpfx_upgrade2headfx:condition2:stickEvltCount"]);

            var dto = (await _mediator.Send(new SwaggerSampleDataQuery("HeaderFxTeamSetupInfoDto"))).GetData<HeaderFxTeamSetupInfoDto>();
            result.AllDesc = dto.AllDesc;
            result.Cases = dto.Cases;

            // EvltCount + StickEvltCount
            {
//                var sql = $@"
//select count(1),sum(case when stick=1 then 1 else 0 end) 
//from Evaluation where IsValid=1 and [status]={EvaluationStatusEnum.Ok.ToInt()}
//and userid=@UserId
//";
//                var x = await _orgUnitOfWork.QueryFirstOrDefaultAsync<(int, int)>(sql, new { query.UserId });
//                result.EvltCount = x.Item1;
//                result.StickEvltCount = x.Item2;
            }

            // ConsumedMoneys
            {
                /** old codes
select sum(payment) --,sum(totalpayment),sum(freight)
from [order] where IsValid=1 and type>={OrderType.BuyCourseByWx.ToInt()}
and (status in({OrderStatusV2.Completed.ToInt()},{OrderStatusV2.Paid.ToInt()}) or (status>300 and status<400))
and userid=@UserId
                 */
                var sql = $@"
select sum(d.price*(d.number-isnull(d.RefundCount,0)-isnull(d.ReturnCount,0))),
sum(case when d.status=@stt333 then d.price*(d.number-isnull(d.RefundCount,0)-isnull(d.ReturnCount,0)) else 0 end)
from [order] o join [OrderDetial] d on o.id=d.orderid
where o.IsValid=1 and o.type>=2 and o.userid=@UserId
and (d.status={OrderStatusV2.Paid.ToInt()} or d.status>300)
";
                var (consumedMoneys, ok333Moneys) = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<(decimal, decimal)>(sql, new
                {
                    query.UserId,
                    stt333 = OrderStatusV2.Shipped.ToInt(),
                });
                result.ConsumedMoneys = consumedMoneys;
                result.ShippedOkMoneys = ok333Moneys;
            }

            return result;
        }

    }
}
