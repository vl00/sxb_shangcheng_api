using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class SetCouponEnableRangeCommandHandler : IRequestHandler<SetCouponEnableRangeCommand>
    {

        OrgUnitOfWork _orgUnitOfWork;

        public SetCouponEnableRangeCommandHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<Unit> Handle(SetCouponEnableRangeCommand request, CancellationToken cancellationToken)
        {

            JArray enableRanges = JArray.Parse(request.CouponInfo.EnableRange_JSN);
            foreach (JObject range in enableRanges)
            {
                int type = range.Value<int>("Type");
                if (type == 1)
                {
                    await InsertSKUEnableRange(request.CouponInfo.Id, range);
                    continue;

                }
                if (type == 2)
                {
                    await InsertGoodTypeEnableRange(request.CouponInfo.Id, range);
                    continue;
                }
                if (type == 3)
                {
                    await InsertBrandEnableRange(request.CouponInfo.Id, range);
                    continue;
                }

            }
            return Unit.Value;

        }


        public async Task InsertSKUEnableRange(Guid couponId, JObject jObject)
        {
            int type = jObject.Value<int>("Type");
            bool isBind = jObject.Value<bool>("IsBind");
            var SKUItems = jObject["SKUItems"];
            _orgUnitOfWork.BeginTransaction();
            try
            {
                string del = @"
delete CouponSpecialSKUEnableRange where CouponId = @CouponId
delete CouponGoodsTypeEnableRange where CouponId = @CouponId
delete CouponBrandEnableRange where CouponId = @CouponId
delete CouponEnableRange where CouponId = @CouponId";
                await _orgUnitOfWork.ExecuteAsync(del, new { CouponId = couponId }, _orgUnitOfWork.DbTransaction);
                string inserEnableRangeSql = @"INSERT INTO [dbo].[CouponEnableRange]
           ([Id]
           ,[Type]
           ,[CouponId]
           ,[IsBind])
     VALUES
           (@Id
           ,@Type
           ,@CouponId
           ,@IsBind)";
                Guid enableRangeId = Guid.NewGuid();
                await _orgUnitOfWork.ExecuteAsync(inserEnableRangeSql, new { Id = enableRangeId, Type = type, CouponId = couponId, IsBind = isBind }, _orgUnitOfWork.DbTransaction);
                string inserSkuEnableRangeSql = @"INSERT INTO [dbo].[CouponSpecialSKUEnableRange]
           ([Id]
           ,[SKUID]
           ,[CourseName]
           ,[EnableRangeId]
           ,[CouponId])
     VALUES
           (@Id
           ,@SKUID
           ,@CourseName
           ,@EnableRangeId
           ,@CouponId)";
                foreach (var skuItem in SKUItems)
                {
                    Guid skuid = Guid.Parse(skuItem.Value<string>("Id"));
                    string courseName = skuItem.Value<string>("CourseName");
                    await _orgUnitOfWork.ExecuteAsync(inserSkuEnableRangeSql, new { Id = Guid.NewGuid(), SKUID = skuid, CourseName = courseName, EnableRangeId = enableRangeId, CouponId = couponId }, _orgUnitOfWork.DbTransaction);
                }


                _orgUnitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {

                _orgUnitOfWork.Rollback();
            }
        }


        public async Task InsertGoodTypeEnableRange(Guid couponId, JObject jObject)
        {
            int type = jObject.Value<int>("Type");
            int id = jObject.Value<int>("Id");
            var name = jObject.Value<string>("Name");
            _orgUnitOfWork.BeginTransaction();
            try
            {
                string del = @"
delete CouponSpecialSKUEnableRange where CouponId = @CouponId
delete CouponGoodsTypeEnableRange where CouponId = @CouponId
delete CouponBrandEnableRange where CouponId = @CouponId
delete CouponEnableRange where CouponId = @CouponId";
                await _orgUnitOfWork.ExecuteAsync(del, new { CouponId = couponId }, _orgUnitOfWork.DbTransaction);
                string inserEnableRangeSql = @"INSERT INTO [dbo].[CouponEnableRange]
           ([Id]
           ,[Type]
           ,[CouponId])
     VALUES
           (@Id
           ,@Type
           ,@CouponId)";
                Guid enableRangeId = Guid.NewGuid();
                await _orgUnitOfWork.ExecuteAsync(inserEnableRangeSql, new { Id = enableRangeId, Type = type, CouponId = couponId }, _orgUnitOfWork.DbTransaction);
                string inserSkuEnableRangeSql = @"INSERT INTO [dbo].[CouponGoodsTypeEnableRange]
           ([Id]
           ,[GoodsType]
           ,[GoddsTypeName]
           ,[EnableRangeId]
           ,[CouponId])
     VALUES
           (@Id
           ,@GoodsType
           ,@GoddsTypeName
           ,@EnableRangeId
           ,@CouponId)";
                await _orgUnitOfWork.ExecuteAsync(inserSkuEnableRangeSql, new { Id = Guid.NewGuid(), GoodsType = id, GoddsTypeName = name, EnableRangeId = enableRangeId, CouponId = couponId }, _orgUnitOfWork.DbTransaction);


                _orgUnitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
            }


        }



        public async Task InsertBrandEnableRange(Guid couponId, JObject jObject)
        {
            int type = jObject.Value<int>("Type");
            Guid id = Guid.Parse(jObject.Value<string>("Id"));
            var name = jObject.Value<string>("Name");
            _orgUnitOfWork.BeginTransaction();
            try
            {
                string del = @"
delete CouponSpecialSKUEnableRange where CouponId = @CouponId
delete CouponGoodsTypeEnableRange where CouponId = @CouponId
delete CouponBrandEnableRange where CouponId = @CouponId
delete CouponEnableRange where CouponId = @CouponId";
                await _orgUnitOfWork.ExecuteAsync(del, new { CouponId = couponId }, _orgUnitOfWork.DbTransaction);
                string inserEnableRangeSql = @"INSERT INTO [dbo].[CouponEnableRange]
           ([Id]
           ,[Type]
           ,[CouponId])
     VALUES
           (@Id
           ,@Type
           ,@CouponId)";
                Guid enableRangeId = Guid.NewGuid();
                await _orgUnitOfWork.ExecuteAsync(inserEnableRangeSql, new { Id = enableRangeId, Type = type, CouponId = couponId }, _orgUnitOfWork.DbTransaction);
                string inserSkuEnableRangeSql = @"INSERT INTO  [dbo].[CouponBrandEnableRange]
           ([Id]
           ,[BrandId]
           ,[BrandName]
           ,[EnableRangeId]
           ,[CouponId])
     VALUES
           (@Id
           ,@BrandId
           ,@BrandName
           ,@EnableRangeId
           ,@CouponId)";
                await _orgUnitOfWork.ExecuteAsync(inserSkuEnableRangeSql, new { Id = Guid.NewGuid(), BrandId = id, BrandName = name, EnableRangeId = enableRangeId, CouponId = couponId }, _orgUnitOfWork.DbTransaction);


                _orgUnitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
            }


        }

    }





}
