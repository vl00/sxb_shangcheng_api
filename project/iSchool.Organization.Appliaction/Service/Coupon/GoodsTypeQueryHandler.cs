using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class GoodsTypeQueryHandler : IRequestHandler<GoodsTypeQuery, IEnumerable<GoodsTypeEnableRange>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public GoodsTypeQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<IEnumerable<GoodsTypeEnableRange>> Handle(GoodsTypeQuery request, CancellationToken cancellationToken)
        {
            DynamicParameters parameters = new DynamicParameters();
            if (request.Type == 1)
            {
                parameters.Add("type", 15);
            }
            else if (request.Type == 2)
            {
                parameters.Add("type", 14);
            }
            else {
                throw new Exception("暂不支持该类型。");
            }
            string sql = @"select [key] id,[name] from KeyValue where IsValid=1 and [type]=@type order by sort";
           var res = await  _orgUnitOfWork.QueryAsync<GoodsTypeEnableRange>(sql, parameters);
            return res;

        }
    }
}
