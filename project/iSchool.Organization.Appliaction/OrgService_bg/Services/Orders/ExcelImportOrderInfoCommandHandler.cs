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
    /// 批量导入更新订单信息
    /// </summary>
    public class ExcelImportOrderInfoCommandHandler : IRequestHandler<ExcelImportOrderInfoCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public ExcelImportOrderInfoCommandHandler(IOrgUnitOfWork orgUnitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
            _redisClient = redisClient;
        }

        public Task<bool> Handle(ExcelImportOrderInfoCommand request, CancellationToken cancellationToken)
        {

            using (var package = new ExcelPackage())
            {
                package.Load(request.Excel);

                var worksheet = package.Workbook.Worksheets.First(); //sheet1

            var ls = new List<Domain.Order>();
            for (var i = 2; i <= worksheet.Dimension.Rows; i++)
            {
                var code = worksheet.Cells[$"A{i}"].Value?.ToString();
                    int? AppointmentStatus = null;
                    var astatus = worksheet.Cells[$"B{i}"].Value?.ToString();
                    if (astatus!=null)
                    {
                        AppointmentStatus = Convert.ToInt32(astatus);
                    }
                   
                    var systemRemark = worksheet.Cells[$"C{i}"].Value?.ToString();
                    var order = new Domain.Order();
                    order.Code = code;
                    order.AppointmentStatus = AppointmentStatus;
                    order.SystemRemark = string.IsNullOrEmpty(systemRemark) ? "" : $"||{DateTime.Now.ToString("yyyy-MM-dd HH:mm")} {systemRemark}";
                    order.Modifier = request.UserId;
                    order.ModifyDateTime = DateTime.Now;
                    ls.Add(order);
                   
                }

            try
            {
                _orgUnitOfWork.BeginTransaction();
                if (ls.Any() == true)
                {
                  
                    foreach (var order in ls)
                    {
                            string sql = $@"
update dbo.[Order] set SystemRemark='' where code=@code and SystemRemark is null;
update [dbo].[Order] set 
AppointmentStatus=@AppointmentStatus
,SystemRemark+=@SystemRemark
,Modifier=@Modifier
,ModifyDateTime=GETDATE()
where IsValid=1 and code=@code
";
                        _orgUnitOfWork.DbConnection.Execute(sql, new DynamicParameters()
.Set("SystemRemark", order.SystemRemark)
.Set("Modifier", order.Modifier)
.Set("code", order.Code)
.Set("AppointmentStatus",order.AppointmentStatus)
, _orgUnitOfWork.DbTransaction);                      
                    }
                }
                _orgUnitOfWork.CommitChanges();               
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                throw ex;
            }

            return Task.FromResult(true);
        }
    }
    }
}
