using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Infrastructure.Repositories.Organization
{
    public class CouponReceiveRepository : ICouponReceiveRepository
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        public IUnitOfWork UnitOfWork => _orgUnitOfWork;
        public CouponReceiveRepository(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }



        public CouponReceive Add(CouponReceive couponReceive)
        {
            _orgUnitOfWork.DbConnection.Insert(couponReceive, _orgUnitOfWork.DbTransaction);
            int number = _orgUnitOfWork.DbConnection.ExecuteScalar<int>("SELECT Number FROM CouponReceive WHERE id = @id", new { id = couponReceive.Id }, _orgUnitOfWork.DbTransaction);
            if (number == 0){
                throw new Exception("插入 couponReceive 失败。");
            }
            couponReceive.Number = number;
            return couponReceive;
        }

        public async Task<CouponReceive> FindAsync(Guid id)
        {
            string sql = @"SELECT [Id],[Number],[CouponId],[UserId] ,[GetTime] ,[VaildStartTime] ,[VaildEndTime]
,[UsedTime] ,[Status] ,[OrderId] ,[OriginType] ,[ReadTime] ,[Remark]  FROM [Organization].[dbo].[CouponReceive]
WHERE IsDel = 0 AND Id = @id";
            
            var res = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync(sql,new { id }, _orgUnitOfWork.DbTransaction);
            return new CouponReceive(res.Id, res.CouponId, res.UserId, res.GetTime, res.VaildStartTime, res.VaildEndTime
                ,(CouponReceiveState)res.Status,res.UsedTime,res.OrderId,(CouponReceiveOriginType)res.OriginType,res.ReadTime,res.Remark,res.Number);
        
        }

        public bool Update(CouponReceive couponReceive)
        {
            return _orgUnitOfWork.DbConnection.Update(couponReceive, _orgUnitOfWork.DbTransaction);

        }

        public async Task<bool> UpdateAsync(CouponReceive couponReceive, params string[] fields)
        {
            if (fields == null || !fields.Any())
            {
                throw new ArgumentNullException("fields not hanve any element.");
            }
            string sql = @"UPDATE [dbo].[CouponReceive] SET {0} WHERE Id = @Id";
            List<string> setFields = fields.Select(field=>$"[{field}]=@{field}").ToList();
           return (await  _orgUnitOfWork.ExecuteAsync(sql.FormatWith(string.Join(",", setFields)),couponReceive, _orgUnitOfWork.DbTransaction)) > 0;
        }

        public async Task<CouponReceive> FindFromOrderAsync(Guid orderId)
        {
            string sql = @"SELECT [Id],[Number],[CouponId],[UserId] ,[GetTime] ,[VaildStartTime] ,[VaildEndTime]
,[UsedTime] ,[Status] ,[OrderId] ,[OriginType] ,[ReadTime] ,[Remark] FROM [Organization].[dbo].[CouponReceive]
WHERE IsDel = 0 AND OrderId = @orderId";
            var res = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync(sql, new { orderId }, _orgUnitOfWork.DbTransaction);
            if (res == null) return null;
            return new CouponReceive(res.Id, res.CouponId, res.UserId, res.GetTime, res.VaildStartTime, res.VaildEndTime
                , (CouponReceiveState)res.Status, res.UsedTime, res.OrderId,(CouponReceiveOriginType)res.OriginType, res.ReadTime, res.Remark, res.Number);

        }
    }
}
