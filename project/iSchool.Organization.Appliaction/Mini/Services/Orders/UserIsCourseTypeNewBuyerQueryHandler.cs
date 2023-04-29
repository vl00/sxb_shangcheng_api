using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class UserIsCourseTypeNewBuyerQueryHandler : IRequestHandler<UserIsCourseTypeNewBuyerQuery, UserIsCourseTypeNewBuyerQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;        
        IMapper _mapper;
        IConfiguration _config;

        public UserIsCourseTypeNewBuyerQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<UserIsCourseTypeNewBuyerQryResult> Handle(UserIsCourseTypeNewBuyerQuery query, CancellationToken cancellation)
        {
            var result = new UserIsCourseTypeNewBuyerQryResult();
            await default(ValueTask);

            var sql = default(string);
            var dys = new DynamicParameters();
            var naf = default(DateTime?);

            if (query.ExcludedOrderIds?.Length > 0)
            {
                sql = @"select min(o.CreateTime) from [order] o where o.id in @ids";
                naf = await _orgUnitOfWork.QueryFirstOrDefaultAsync<DateTime?>(sql, new { ids = query.ExcludedOrderIds });
            }

            switch (query.CourseType)
            {
                // 只要第一次购买>0.1元的网课就不再是新用户
                case CourseTypeEnum.Course:
                    {
                        var priceRange = MathInterval.Parse(_config["AppSettings:EvltReward:CourseBonus:newbuyer:in"]);
                        sql = $@"
select top 1 o.id,o.code,o.CreateTime from [order] o 
join OrderDetial p on p.orderid=o.id and p.producttype={CourseTypeEnum.Course.ToInt()}
where o.IsValid=1 and o.type>={OrderType.BuyCourseByWx.ToInt()} and (o.status>={OrderStatusV2.Paid.ToInt()} ) --or o.status>300
{(!double.IsInfinity(priceRange.B) ? $"and p.price{(priceRange.Ib ? ">" : ">=")}@B" : "")}
and o.userid=@userId {"and o.id not in @ExcludedOrderIds".If(query.ExcludedOrderIds?.Length > 0)}
{"and o.CreateTime<=@naf".If(naf != null)}
--order by o.status,o.CreateTime desc
";
                        dys.Set("userId", query.UserId);
                        dys.Set("ExcludedOrderIds", query.ExcludedOrderIds);
                        dys.Set("B", priceRange.B);
                        dys.Set("naf", naf);
                    }
                    break;

                // 未购买过任何商品
                case CourseTypeEnum.Goodthing:
                    {
                        sql = $@"
select top 1 o.id,o.code,o.CreateTime from [order] o 
join OrderDetial p on p.orderid=o.id --and p.producttype={CourseTypeEnum.Goodthing.ToInt()}
where o.IsValid=1 and o.type>={OrderType.BuyCourseByWx.ToInt()} and (o.status>={OrderStatusV2.Paid.ToInt()} ) --or o.status>300
and o.userid=@userId {"and o.id not in @ExcludedOrderIds".If(query.ExcludedOrderIds?.Length > 0)}
{"and o.CreateTime<=@naf".If(naf != null)}
";
                        dys.Set("userId", query.UserId);
                        dys.Set("ExcludedOrderIds", query.ExcludedOrderIds);
                        dys.Set("naf", naf);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            var q = await _orgUnitOfWork.QueryFirstOrDefaultAsync(sql, dys);
            result.IsNewBuyer = q == null;

            return result;
        }

    }
}
