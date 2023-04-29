using CSRedis;
using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
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
    /// <summary>
    /// 选择评测品牌--相关课程
    /// </summary>
    public class CoursesByOrgIdQueryHandler : IRequestHandler<CoursesByOrgIdQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int slideTime = 60 * 60;
     
        public CoursesByOrgIdQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            
        }
        public async Task<ResponseResult> Handle(CoursesByOrgIdQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string key = string.Format(CacheKeys.BrandCoursesByOrg, request.OrgId,request.PageInfo.PageIndex+"&"+request.PageInfo.PageSize);
            var data = _redisClient.Get<CoursesByOrgIdQueryResponse>(key);
            if (data != null)
            {
                return ResponseResult.Success(data);
            }
            else
            {
                var dy = new DynamicParameters();
                dy.Add("@OrgId", request.OrgId);
                dy.Add("@PageIndex", request.PageInfo.PageIndex);
                dy.Add("@PageSize", request.PageInfo.PageSize);
                string sql = $@" 
                        select top {request.PageInfo.PageSize} * 
                        from(
                        	select ROW_NUMBER() over(order by c.id desc) rownum,c.id,c.no,c.name,c.banner,c.title,c.price,c.origprice,c.stock,o.authentication  from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1 and o.IsValid=1 
                            where o.id=@OrgId and  c.status=1 and o.status=1 and c.type={CourseTypeEnum.Course.ToInt()}
                        )TT where rownum>((@PageIndex-1)*@PageSize)";
                string sqlPage = $@"
                            select COUNT(1)  AS TotalCount                                
                            from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1 and o.IsValid=1 
                            where   o.id=@OrgId  and  c.status=1 and o.status=1 and c.type={CourseTypeEnum.Course.ToInt()}
                                 ;";
                data = new CoursesByOrgIdQueryResponse();
                data.PageInfo = new PageInfoResult();
                data.PageInfo = _unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();
                data.PageInfo.PageIndex = request.PageInfo.PageIndex;
                data.PageInfo.PageSize = request.PageInfo.PageSize;
                data.PageInfo.TotalPage= (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);

                var dBDatas = _unitOfWork.Query<CoursesDataDB>(sql, dy).ToList();
                if (dBDatas.Any()==true)
                {
                    data.CoursesDatas = new List<CoursesData>();
                    for (int i = 0; i < dBDatas.Count; i++)
                    {
                        data.CoursesDatas.Add(new CoursesData()
                        {
                            Authentication = dBDatas[i].Authentication,
                            Banner = dBDatas[i].Banner == null ? null : JsonSerializationHelper.JSONToObject<List<string>>(dBDatas[i].Banner),
                            Id = dBDatas[i].Id,
                            Name = dBDatas[i].Name,
                            No = UrlShortIdUtil.Long2Base32(Convert.ToInt64(dBDatas[i].No)),
                            OrigPrice = dBDatas[i].OrigPrice,
                            Price = dBDatas[i].Price,
                            Stock = dBDatas[i].Stock,
                            Title = dBDatas[i].Title
                        });
                    }
                }
                //data.CoursesDatas = _unitOfWork.DbConnection.Query<CoursesData>(sql, dy).ToList();
                _redisClient.Set(key, JsonSerializationHelper.Serialize(data), slideTime);
                return ResponseResult.Success(data);
            }
            
        }
    }
}
