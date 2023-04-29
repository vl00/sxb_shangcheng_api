using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Domain;
using MediatR;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{
    /// <summary>
    /// 更新订单信息--机构反馈
    /// </summary>
    public class UpdateOrderInfoCommandHandler : IRequestHandler<UpdateOrderInfoCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public UpdateOrderInfoCommandHandler(IOrgUnitOfWork orgUnitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
            _redisClient = redisClient;
        }

        public Task<bool> Handle(UpdateOrderInfoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string sql = $@"
update dbo.[Order] set SystemRemark='' where id=@orderid and SystemRemark is null;

update [dbo].[Order] set 
SystemRemark+=@SystemRemark
,Modifier=@Modifier
,ModifyDateTime=GETDATE()
where IsValid=1 and id=@orderid
";
                var systemRemark = string.IsNullOrEmpty(request.SystemRemark) ? "" : $"||{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {request.SystemRemark}";
                _orgUnitOfWork.DbConnection.Execute(sql, new DynamicParameters()
.Set("SystemRemark", systemRemark)
.Set("Modifier", request.UserId)
.Set("orderid", request.OrderId)
);
            }
            catch (Exception ex)
            {                
                throw ex;
            }

            return Task.FromResult(true);
        }
    }
}
