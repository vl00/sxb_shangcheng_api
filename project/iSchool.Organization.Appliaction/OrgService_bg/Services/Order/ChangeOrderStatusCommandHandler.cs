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
    /// 更改订单状态
    /// </summary>
    public class ChangeOrderStatusCommandHandler : IRequestHandler<ChangeOrderStatusCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
              
        public ChangeOrderStatusCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;       
        }

        public async Task<ResponseResult> Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var dy = new DynamicParameters()
                .Set("id", request.Id)     
                .Set("Status", request.Status)
                .Set("courseid", request.CourseId)
                ;
            string order_sql = $@"update[dbo].[Order] set Status=@Status  where id = @id and IsValid=1 ; ";
            string course_sql = "";
            
            if(request.Status== (int)OrderStatus.Delivered)//已发货：课程表的库存-1，课程表的销量+1；
            {
                course_sql = "update [dbo].[Course] set stock-=1,sellcount+=1 where id=@courseid and IsValid=1;";
            }
            else if (request.Status == (int)OrderStatus.Returned)//已退货：课程表的库存+1，课程表的销量-1 ,退单数+1 
            {
                course_sql = "update [dbo].[Course] set stock+=1,sellcount-=1,ChargebackCount+=1 where id=@courseid and IsValid=1;";
            }
            try
            {                     
                _orgUnitOfWork.BeginTransaction();
                var count = _orgUnitOfWork.DbConnection.Execute(order_sql+ course_sql, dy,_orgUnitOfWork.DbTransaction);
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
