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
    public class OrderDetailRefundCountsQryArgsHandler : IRequestHandler<OrderDetailRefundCountsQryArgs, (int OkCount, int RefundingCount)>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public OrderDetailRefundCountsQryArgsHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<(int OkCount, int RefundingCount)> Handle(OrderDetailRefundCountsQryArgs query, CancellationToken cancellation)
        {
            var sql = $"select r.id,r.OrderId,r.OrderDetailId,r.type,r.status,r.count from OrderRefunds r where r.IsValid=1 and r.OrderDetailId=@OrderDetailId";
            var fs = await _orgUnitOfWork.DbConnection.QueryAsync<OrderRefunds>(sql, new { query.OrderDetailId }); // use write conn

            var okCount = fs.Where(p => ((RefundTypeEnum)p.Type).In(RefundTypeEnum.FastRefund, RefundTypeEnum.BgRefund)
                    || ((RefundStatusEnum)p.Status).In(RefundStatusEnum.RefundSuccess, RefundStatusEnum.ReturnSuccess)
                ).Sum(_ => _.Count);

            var rdingCount = fs.Sum(_ => _.Count) - okCount - fs.Where(p => ((RefundStatusEnum)p.Status).In(
                    RefundStatusEnum.RefundAuditFailed, RefundStatusEnum.ReturnAuditFailed, RefundStatusEnum.InspectionFailed,
                    RefundStatusEnum.Cancel, RefundStatusEnum.CancelByExpired
                )).Sum(_ => _.Count);

            return (okCount, rdingCount);
        }

    }
}
