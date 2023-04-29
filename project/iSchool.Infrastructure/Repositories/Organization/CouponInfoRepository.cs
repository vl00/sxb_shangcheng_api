using Dapper.Contrib.Extensions;
using iSchool.Domain;
using iSchool.Infras.Locks;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;
namespace iSchool.Infrastructure.Repositories.Organization
{
    public class CouponInfoRepository : ICouponInfoRepository
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        public IUnitOfWork UnitOfWork => _orgUnitOfWork;


        public CouponInfoRepository(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public CouponInfo Add(CouponInfo couponInfo)
        {
            _orgUnitOfWork.DbConnection.Insert(couponInfo, _orgUnitOfWork.DbTransaction);
            return couponInfo;
        }

        public async Task<CouponInfo> GetAsync(Guid id)
        {
            return await _orgUnitOfWork.DbConnection.GetAsync<CouponInfo>(id, _orgUnitOfWork.DbTransaction);
        }

        public async Task<bool> UpdateAsync(CouponInfo couponInfo)
        {
            return await _orgUnitOfWork.DbConnection.UpdateAsync(couponInfo, _orgUnitOfWork.DbTransaction);
        }

        public async Task<bool> UpdateAsync(CouponInfo couponInfo, params string[] fields)
        {
            if (fields == null || !fields.Any())
            {
                throw new ArgumentNullException("fields not hanve any element.");
            }

            string sql = @"UPDATE [dbo].[CouponInfo] SET {0} WHERE Id = @Id";
            List<string> setFields = fields.Select(field => $"[{field}]=@{field}").ToList();
            return (await _orgUnitOfWork.ExecuteAsync(sql.FormatWith(string.Join(",", setFields)), couponInfo, _orgUnitOfWork.DbTransaction)) > 0;
        }

        public async Task<CouponInfo> FindFromNumberAsync(int number)
        {
            string sql = @"SELECT * FROM CouponInfo WHERE Number = @number";
            return await _orgUnitOfWork.DbConnection.QueryFirstAsync<CouponInfo>(sql, new { number }, _orgUnitOfWork.DbTransaction);
        }
    }
}
