using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Domain.Enum;

namespace iSchool.Organization.Appliaction.OrgService_bg.Order
{
    /// <summary>
    /// 更改约课状态
    /// </summary>
    public class ChangeAppointmentStatusCommandHandler : IRequestHandler<ChangeAppointmentStatusCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
              
        public ChangeAppointmentStatusCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;       
        }

        public async Task<ResponseResult> Handle(ChangeAppointmentStatusCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var dy = new DynamicParameters()
                .Set("id", request.OrdId)     
                .Set("appointmentStatus", request.AppointmentStatus)
                .Set("userid", request.UserId)
                .Set("time",DateTime.Now)
                ;
            string order_sql = $@" update [dbo].[Order] set appointmentStatus=@appointmentStatus,Modifier=@userid 
,[ModifyDateTime]=@time
where id = @id and IsValid=1  ; ";           
            try
            {                     
                _orgUnitOfWork.BeginTransaction();
                var count = _orgUnitOfWork.DbConnection.Execute(order_sql, dy,_orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.CommitChanges();
                return ResponseResult.Success("操作成功");
            }
            catch(Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return ResponseResult.Failed("操作失败");
            }           
        }
    }
}
