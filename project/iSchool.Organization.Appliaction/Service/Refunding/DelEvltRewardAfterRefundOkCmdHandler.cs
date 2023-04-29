using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class DelEvltRewardAfterRefundOkCmdHandler : IRequestHandler<DelEvltRewardAfterRefundOkCmd>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public DelEvltRewardAfterRefundOkCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<Unit> Handle(DelEvltRewardAfterRefundOkCmd cmd, CancellationToken cancellation)
        {
            var orderRefund = cmd.OrderRefund;
            if (orderRefund == null)
            {
                orderRefund = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<OrderRefunds>($@"
                    select * from OrderRefunds where IsValid=1 and id=@id
                ", new { id = cmd.Id });
            }
            if (orderRefund == null)
            {
                throw new CustomResponseException($"无效的退款单id={cmd.Id}");
            }

            // 删除机会
            {
                var sql = $@"
select top {orderRefund.Count} r.* 
from EvaluationReward r left join [OrderDetial] d on d.orderid=r.orderid and d.Productid=r.goodsid
where isnull(r.isvalid,1)=1 and r.used=0 and r.orderid=@OrderId and d.id=@OrderDetailId
order by r.createtime asc
";
                var ls = await _orgUnitOfWork.DbConnection.QueryAsync<EvaluationReward>(sql, new { orderRefund.OrderId, orderRefund.OrderDetailId });
                if (ls.Count() < 1) return default;

                sql = $@"update EvaluationReward set IsValid=0,ModifyDateTime=getdate(),Modifier='00000000-0000-0000-0000-000000000001' where Id in @Ids";
                await _orgUnitOfWork.ExecuteAsync(sql, new { Ids = ls.Select(_ => _.Id).ToArray() });
            }

            return default;
        }

    }
}
