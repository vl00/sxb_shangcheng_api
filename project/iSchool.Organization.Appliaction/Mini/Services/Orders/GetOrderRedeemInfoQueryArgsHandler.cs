using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetOrderRedeemInfoQueryArgsHandler : IRequestHandler<GetOrderRedeemInfoQueryArgs, IEnumerable<OrderRedeemInfoDto>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient redis;              

        public GetOrderRedeemInfoQueryArgsHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;            
        }

        public async Task<IEnumerable<OrderRedeemInfoDto>> Handle(GetOrderRedeemInfoQueryArgs query, CancellationToken cancellation)
        {
            if (query.OrderIds?.Length < 1) return Enumerable.Empty<OrderRedeemInfoDto>();

            // RedeemCode重新导入时会把原来的变成IsVaild=0, 所以只查记录就可以了
            //
            var sql = $@"
select e.orderid,e.code as RedeemCode,e.mobile,m.url,m.msg,m.IsRedirect,c.* 
from Exchange e
left join [order] o on e.orderid=o.id and o.type>={OrderType.BuyCourseByWx.ToInt()} and o.IsValid=1
left join MsgTemplate m on m.courseid=o.courseid
left join RedeemCode c on e.code=c.code --and c.IsVaild=1
where e.IsValid=1 and e.status={ExchangeStatus.Converted.ToInt()} --and c.Used=1
and o.id in @OrderIds
";
            var ls = await _orgUnitOfWork.QueryAsync<OrderRedeemInfoDto, RedeemCode, OrderRedeemInfoDto>(sql, 
                param: new { query.OrderIds },
                splitOn: "id",
                map: (dto, r) => 
                {
                    r ??= new RedeemCode { Code = dto.RedeemCode };
                    r.IsVaild = r.Used = true;
                    dto.Redeem0 = r;
                    return dto;
                }
            );
            return ls.AsList();            
        }

    }
}
