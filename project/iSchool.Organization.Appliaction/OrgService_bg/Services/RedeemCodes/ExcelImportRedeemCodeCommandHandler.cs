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

namespace iSchool.Organization.Appliaction.OrgService_bg.RedeemCodes
{
    /// <summary>
    /// 导入兑换码
    /// </summary>
    public class ExcelImportRedeemCodeCommandHandler : IRequestHandler<ExcelImportRedeemCodeCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public ExcelImportRedeemCodeCommandHandler(IOrgUnitOfWork orgUnitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
            _redisClient = redisClient;
        }

        public Task<bool> Handle(ExcelImportRedeemCodeCommand request, CancellationToken cancellationToken)
        {
            using (var package = new ExcelPackage())
            {
                package.Load(request.Excel);

                var worksheet = package.Workbook.Worksheets.First(); //sheet1

                var ls = new List<RedeemCode>();
                for (var i = 2; i <= worksheet.Dimension.Rows; i++)
                {
                    var code= worksheet.Cells[$"A{i}"].Value?.ToString();
                    if (!string.IsNullOrEmpty(code))
                    {
                        var redeemCode = new RedeemCode();
                        redeemCode.Id = Guid.NewGuid();
                        redeemCode.Code = code;
                        redeemCode.Courseid = request.CourseId;
                        redeemCode.Createor = request.UserId;
                        redeemCode.CreatTime = DateTime.Now;
                        redeemCode.Used = false;
                        redeemCode.IsVaild = true;
                        ls.Add(redeemCode);
                    }                    
                }

                try
                {
                    _orgUnitOfWork.BeginTransaction();
                    if (ls.Any() == true)
                    {
                        _orgUnitOfWork.DbConnection.Execute($@" update dbo.RedeemCode set IsVaild=0 where Courseid='{request.CourseId}'; ", null, _orgUnitOfWork.DbTransaction);
                        foreach (var redeemCode in ls)
                        {
                            _orgUnitOfWork.DbConnection.Execute($@"
Insert into dbo.RedeemCode (Id, Courseid, GoodId, Code, [ExpireDate], CreatTime, Createor, IsVaild, Used)
values(NEWID(), @Courseid, @GoodId, @Code, @ExpireDate, @CreatTime, @Createor, @IsVaild, @Used)
", new DynamicParameters()
.Set("Courseid", redeemCode.Courseid)
.Set("GoodId", redeemCode.GoodId)
.Set("Code", redeemCode.Code)
.Set("ExpireDate", redeemCode.ExpireDate)
.Set("CreatTime", redeemCode.CreatTime)
.Set("Createor", redeemCode.Createor)
.Set("IsVaild", redeemCode.IsVaild)
.Set("Used", redeemCode.Used)
, _orgUnitOfWork.DbTransaction);
                            
                        }
                    }
                    _orgUnitOfWork.CommitChanges();

                    //导入兑换码成功，则清除所有订单绑定兑换码的缓存
                    _redisClient.BatchDelAsync(CacheKeys.notUsedSingleCode.FormatWith(request.CourseId,"*"),10);
                    _redisClient.BatchDelAsync(CacheKeys.CodeIsLock.FormatWith(request.CourseId, "*"), 10);
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
