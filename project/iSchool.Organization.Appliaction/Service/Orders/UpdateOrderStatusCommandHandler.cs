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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;                
        IConfiguration _config;

        public UpdateOrderStatusCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;            
            this._config = config;
        }

        public async Task<bool> Handle(UpdateOrderStatusCommand cmd, CancellationToken cancellation)
        {
            var status0UnPaid_Timeout = cmd.Status0 == (int)OrderStatusV2.Unpaid ? cmd.Status0UnPaid_TimeoutMin * 60 : null;
            var paymenttime = cmd.NewStatus == (int)OrderStatusV2.Paid ? cmd.NewStatusOk_Paymenttime : null;

            try
            {
                _orgUnitOfWork.BeginTransaction();

                string sql;
                if (cmd.AdvanceOrderId != null)
                {
                    sql = $@"
update [OrderDetial] set [status]=@NewStatus
    where 1=1 {"and [status]=@Status0".If(cmd.Status0 != null)} and [orderid] in (select id from [order] where [AdvanceOrderId]=@AdvanceOrderId)
---
update [order] set [status]=@NewStatus, {"[Paymenttime]=@Paymenttime,".If(paymenttime != null)}
[ModifyDateTime]=@now,[Modifier]='11111111-1111-1111-1111-111111111111'
where 1=1 {"and [status]=@Status0".If(cmd.Status0 != null)} and [AdvanceOrderId]=@AdvanceOrderId
{"and datediff(second,[CreateTime],getdate())<@Status0UnPaid_Timeout".If(status0UnPaid_Timeout != null)}
";
                }
                else
                {
                    sql = $@"
update [OrderDetial] set [status]=@NewStatus
    where [orderid]=@orderid {"and [status]=@Status0".If(cmd.Status0 != null)}
---
update [order] set [status]=@NewStatus, {"[Paymenttime]=@Paymenttime,".If(paymenttime != null)}
[ModifyDateTime]=@now,[Modifier]='11111111-1111-1111-1111-111111111111'
where [id]=@orderid {"and [status]=@Status0".If(cmd.Status0 != null)} 
{"and datediff(second,[CreateTime],getdate())<@Status0UnPaid_Timeout".If(status0UnPaid_Timeout != null)}
";
                }
                var i = await _orgUnitOfWork.DbConnection.ExecuteAsync(sql, new
                {
                    cmd.OrderId,
                    cmd.AdvanceOrderId,
                    cmd.Status0,
                    status0UnPaid_Timeout,
                    cmd.NewStatus,
                    paymenttime,
                    now = paymenttime == null ? DateTime.Now 
                        : paymenttime < DateTime.Now ? DateTime.Now
                        : paymenttime.Value.AddSeconds(1),
                }, _orgUnitOfWork.DbTransaction);

                if (i > 0) _orgUnitOfWork.CommitChanges();
                else _orgUnitOfWork.SafeRollback();

                return i > 0;
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.SafeRollback();
                throw ex;
            }
        }

    }
}
