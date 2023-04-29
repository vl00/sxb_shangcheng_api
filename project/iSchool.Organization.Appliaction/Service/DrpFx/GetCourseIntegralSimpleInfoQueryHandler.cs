using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetCourseIntegralSimpleInfoQueryHandler : IRequestHandler<GetCourseIntegralSimpleInfoQuery, IntegralInfo>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;        

        public GetCourseIntegralSimpleInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator,
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
        }

        public async Task<IntegralInfo> Handle(GetCourseIntegralSimpleInfoQuery query, CancellationToken cancellation)
        {
            var rdkey = CacheKeys.CourseIntegrlInfo.FormatWith(query.CourseId);
            var result = await _redis.GetAsync<IntegralInfo>(rdkey);
            if (result == null)
            {
                var sqlGoods = "SELECT Id from CourseGoods where  CourseId = @CourseId and IsValid=1 and Show=1";
                var goodIds = await _orgUnitOfWork.QueryAsync<Guid>(sqlGoods, new { query.CourseId });
                if (null!= goodIds && goodIds.Count()>0)
                {
                    var sqlIntegral = @" SELECT CASE cashhback.PointCashBackType WHEN 1 THEN (goods.Price-goods.Costprice)*cashhback.PointCashBackValue ELSE cashhback.PointCashBackValue END
        FROM dbo.CourseGoodPointCashBack  AS cashhback
      LEFT JOIN dbo.CourseGoods AS goods ON goods.Id = cashhback.GoodId AND goods.IsValid=1
      WHERE cashhback.IsValid=1 AND goods.Id in @GoodsIds
      AND goods.Costprice IS NOT NULL  AND goods.Costprice>0
       AND goods.Price>goods.Costprice AND cashhback.PointCashBackValue>0";

                    var list = await _orgUnitOfWork.QueryAsync<double>(sqlIntegral, new { GoodsIds= goodIds });
                    if (null != list && list.Count() > 0)
                    {
                        var min = list.Min();
                        var max = list.Max();
                        if (0 != min || max != 0)
                        {
                            result = new IntegralInfo() { Min = Convert.ToInt32(Math.Floor(min)), Max = Convert.ToInt32(Math.Floor(max)) };
                            await _redis.SetAsync(rdkey, result, 60 * 60 * 1);
                            return result;

                        }

                    }

                   
                }

              
            }
            return result;
        }

        
    }
}
