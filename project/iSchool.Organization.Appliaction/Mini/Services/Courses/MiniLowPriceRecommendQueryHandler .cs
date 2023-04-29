using CSRedis;
using Dapper;
using iSchool.Domain.Enum;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.Mini.Services.Courses;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Course
{
    public class MiniLowPriceRecommendQueryHandler : IRequestHandler<MiniLowPriceRecommendQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        public MiniLowPriceRecommendQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(MiniLowPriceRecommendQuery request, CancellationToken cancellationToken)
        {


            await Task.CompletedTask;
            string key = string.Format(CacheKeys.LowPriceRecomend);
            var data = _redisClient.Get<List<CoursesData>>(key);
            if (data != null)
            {
                return ResponseResult.Success(data);
            }
            data = new List<CoursesData>();

            var dy = new DynamicParameters();
            var sql = @"
SELECT top 1 c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive
from Course c left join [dbo].[Organization] o on o.id=c.orgid and o.IsValid=1  	
where c.NewUserExclusive=1 and c.IsValid=1 and c.status=1 and c.LastOffShelfTime>GETDATE() 
ORDER BY c.LastOffShelfTime 

SELECT top 1 c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive
from Course c left join [dbo].[Organization] o on o.id=c.orgid and o.IsValid=1  	
where c.LimitedTimeOffer=1 and c.IsValid=1 and c.status=1 and c.LastOffShelfTime>GETDATE()  
ORDER BY c.LastOffShelfTime 
";
            var gr = await _unitOfWork.DbConnection.QueryMultipleAsync(sql, dy);
            var dbData = new List<CoursesDataDB>();
            var newExclusive = await gr.ReadFirstOrDefaultAsync<CoursesDataDB>();
            var limitedTimeOffer = await gr.ReadFirstOrDefaultAsync<CoursesDataDB>();
            if (null != newExclusive) dbData.Add(newExclusive);
            if (null != limitedTimeOffer) dbData.Add(limitedTimeOffer);
            if (dbData != null)
            {
                for (int i = 0; i < dbData.Count; i++)
                {
                    var course = dbData[i];
                    var addM = new CoursesData()
                    {
                        Authentication = course.Authentication,
                        Banner = course.Banner == null ? null : JsonSerializationHelper.JSONToObject<List<string>>(course.Banner),
                        Id = course.Id,
                        Name = course.Name,
                        No = UrlShortIdUtil.Long2Base32(Convert.ToInt64(course.No)),
                        OrigPrice = course.OrigPrice,
                        Price = course.Price,
                        Stock = course.Stock,
                        Title = course.Title,
                        LastOffShelfTime = course.LastOffShelfTime.UnixTicks(),
                        CanNewUserReward = course.CanNewUserReward,
                        LimitedTimeOffer = course.LimitedTimeOffer,
                        NewUserExclusive = course.NewUserExclusive,
                    };
                    data.Add(addM);
                }
            }




            return ResponseResult.Success(data);


        }


    }
}
