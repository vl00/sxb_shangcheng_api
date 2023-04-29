using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Supplier;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services.Supplier
{
    public class AddOrEditSupplierCommandHandler : IRequestHandler<AddOrEditSupplierCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public AddOrEditSupplierCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public Task<ResponseResult> Handle(AddOrEditSupplierCommand request, CancellationToken cancellationToken)
        {
            string sql = "";

            

            if (request.IsAdd)
            {
                string querySql = "select top 1 1 from [dbo].[Supplier] where CompanyName = @CompanyName;";

                bool exsitCompanyName = _orgUnitOfWork.DbConnection.Query<int>(querySql, new { request.CompanyName }).FirstOrDefault() > 0;

                if (exsitCompanyName)
                {
                    return Task.FromResult(ResponseResult.Failed("供应商对公账户名称已存在，请检查。"));
                }

                request.Id = Guid.NewGuid();
                sql = $@" INSERT INTO [dbo].[Supplier]([Id], [Name], [BankCardNo], [BankAddress], [IsPrivate], [CompanyName], [Freight], [CreateTime], [Creator], [ModifyDateTime], [Modifier], [IsValid],[BillingType])
                VALUES (@id, @name, @BankCardNo,@BankAddress, @IsPrivate, @CompanyName, 0.00, getdate(), @UserId, getdate(), @UserId, '1',@BillingType) ;";
            }
            else
                sql = $@"UPDATE [dbo].[Supplier] SET [Name] = @name, [BankCardNo] =@BankCardNo, [BankAddress] = @BankAddress, [IsPrivate] =@IsPrivate, [CompanyName] = @CompanyName,[BillingType]=@BillingType, [Freight] = 0.00, [ModifyDateTime] = getdate(), [Modifier] =@UserId 
                WHERE [Id] =@id;";

            var sqldelete = $@" delete from  [dbo].[SupplierBrand]  WHERE SupplierId =@id;";
            string insertBindSql = @"INSERT INTO [dbo].[SupplierBrand]([id],[SupplierId], [OrgId],CreateTime,Creator,ModifyDateTime,Modifier,IsValid) VALUES (NewID(),@SupplierId, @OrgId, getdate(), @Creator, getdate(), @Modifier,'1');";

            string deleteReturnAddrSql = @"UPDATE [dbo].[SupplierAddress] SET [ModifyDateTime] = getdate(), [Modifier] = @Modifier , [IsVaild] = 0 WHERE [SupplierId] = @SupplierId and [Id] not in @Ids and [IsVaild] = 1";
            string inserReturnAddrSql = @"INSERT INTO [dbo].[SupplierAddress]([id],[SupplierId], [ReturnAddress], [Sort], [IsVaild], [CreateTime], [Creator]) VALUES (NewID(),@SupplierId, @ReturnAddress,@Sort,'1', getdate(), @Creator);";
            string updateReturnAddrSql = @"UPDATE [dbo].[SupplierAddress] SET ReturnAddress = @ReturnAddress,Sort=@Sort,IsVaild = @IsVaild,ModifyDateTime = getdate(),Modifier = @Modifier Where Id = @Id;";

            var orgbind = new List<SupplierBrand>();
            foreach (var item in request.OrgIds)
            {
                orgbind.Add(new SupplierBrand { SupplierId = request.Id,OrgId = item,Creator = request.UserId,Modifier = request.UserId });
            }

            var returnAddrs = new List<SupplierAddress>();    
            for(int i = 0;i< request.ReturnAddress.Count(); i++)
            {
                var address = JsonConvert.SerializeObject(new
                {
                    request.ReturnAddress[i].Receiver,
                    request.ReturnAddress[i].Addr,
                    request.ReturnAddress[i].Phone
                });
                returnAddrs.Add(new SupplierAddress { Id = request.ReturnAddress[i].Id, SupplierId = request.Id, ReturnAddress = address, IsVaild = true, Sort = i, Creator = request.UserId, Modifier = request.UserId });
            }

            var dy = new DynamicParameters()
                .Set("id", request.Id)
                .Set("name", request.Name)
                .Set("BankAddress", request.BankAddress)
                .Set("BankCardNo", request.BankCardNo)
                .Set("CompanyName", request.CompanyName)
                .Set("IsPrivate", request.IsPrivate)
                .Set("BillingType", request.BillingType)
                .Set("UserId", request.UserId);
            try
            {
                _orgUnitOfWork.BeginTransaction();

                var count = _orgUnitOfWork.DbConnection.Execute(sql, dy, _orgUnitOfWork.DbTransaction);


                _orgUnitOfWork.DbConnection.Execute(deleteReturnAddrSql, new { SupplierId = request.Id, Ids = returnAddrs.Where(q => q.Id != null).Select(q => q.Id),Modifier = request.UserId }, _orgUnitOfWork.DbTransaction);


                _orgUnitOfWork.DbConnection.Execute(updateReturnAddrSql, returnAddrs.Where(q => q.Id != null), _orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.DbConnection.Execute(inserReturnAddrSql, returnAddrs.Where(q => q.Id == null), _orgUnitOfWork.DbTransaction);


                _orgUnitOfWork.DbConnection.Execute(sqldelete, dy, _orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.DbConnection.Execute(insertBindSql, orgbind, _orgUnitOfWork.DbTransaction);

                _orgUnitOfWork.CommitChanges();
                if (count == 1)
                {

                    return Task.FromResult(ResponseResult.Success("保存成功"));
                }
                else
                {
                    return Task.FromResult(ResponseResult.Failed("保存失败"));
                }
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return Task.FromResult(ResponseResult.Failed("保存失败"));
            }
        }
    }
}
