using CSRedis;
using Dapper;
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
    public class CoursesByInfoQueryHandler:IRequestHandler<CoursesByInfoQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        public CoursesByInfoQueryHandler (IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(CoursesByInfoQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            

            string key = string.Format(CacheKeys.CourseCenter,request.SubjectId,request.AgeGroupId, request.PageInfo.PageIndex + "&" + request.PageInfo.PageSize,request.isAuth);
            var data = _redisClient.Get<CoursesByOrgIdQueryResponse>(key);
            if (data != null)
            {
                return ResponseResult.Success(data);
            }
            else
            {

                var dy = new DynamicParameters();
                #region Where

                string sqlWhere = $@" where 1=1  and o.IsValid=1  and o.status=1 and c.status=1 and c.type={CourseTypeEnum.Course.ToInt()} and c.IsInvisibleOnline=0 ";
                //科目
                if (request.SubjectId != null && Enum.IsDefined(typeof(SubjectEnum), request.SubjectId))
                {
                    dy.Add("@SubjectId", request.SubjectId);
                    sqlWhere += $" and subject=@SubjectId  ";
                }
                //年龄段
                if (request.AgeGroupId != null && Enum.IsDefined(typeof(AgeGroup), request.AgeGroupId))
                {
                    var ages_str = EnumUtil.GetDesc((AgeGroup)request.AgeGroupId).Split('-');
                    var minAge = Convert.ToInt32(ages_str[0]);
                    var maxAge = Convert.ToInt32(ages_str[1]);
                    dy.Add("@minAge", minAge);
                    dy.Add("@maxAge", maxAge);
                    sqlWhere += @$"   and ( (c.minage>=@minAge and c.maxage<=@maxAge)or (c.minage<=@minAge and c.maxage>=@minAge)or (c.minage<=@maxAge and c.maxage>=@maxAge)) and c.maxage>0 ";
                    //dy.Add("@AgeGroupId", request.AgeGroupId);
                    //sqlWhere += $" and age=@AgeGroupId ";
                }
                //认证课程
                if (1==request.isAuth)
                {
                    sqlWhere += $" and o.authentication=1 ";
                }
                dy.Add("@PageIndex", request.PageInfo.PageIndex);
                dy.Add("@PageSize", request.PageInfo.PageSize);
                #endregion

                string sql = $@" 
                        select top {request.PageInfo.PageSize} * 
                        from(
                        	select ROW_NUMBER() over(order by o.authentication desc,c.no desc,c.CreateTime desc) rownum,c.id,c.no,c.name,c.Title,c.banner,c.price,c.origprice,c.stock,o.authentication  from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1
                            {sqlWhere} 
                        )TT where rownum> (@PageIndex-1)*@PageSize ;";
                string sqlPage = $@"
                            select COUNT(1) as TotalCount 
                            from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid  and c.IsValid=1
                            {sqlWhere} 
                            ;";
                data = new CoursesByOrgIdQueryResponse();
                data.CoursesDatas = new List<CoursesData>();
                var dBDatas= _unitOfWork.Query<CoursesDataDB>(sql, dy).ToList();
                if (dBDatas != null)
                {
                    for (int i = 0; i < dBDatas.Count; i++)
                    {
                        data.CoursesDatas.Add(new CoursesData()
                        {
                            Authentication = dBDatas[i].Authentication,
                            Banner = dBDatas[i].Banner == null ? null : JsonSerializationHelper.JSONToObject<List<string>>(dBDatas[i].Banner),
                            Id = dBDatas[i].Id,
                            Name = dBDatas[i].Name,
                            No = UrlShortIdUtil.Long2Base32(Convert.ToInt64(dBDatas[i].No)) ,
                            OrigPrice = dBDatas[i].OrigPrice,
                            Price = dBDatas[i].Price,
                            Stock = dBDatas[i].Stock,
                            Title = dBDatas[i].Title
                        });
                    }
                }                
                data.PageInfo = new PageInfoResult();               
                data.PageInfo = _unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();
                data.PageInfo.PageIndex = request.PageInfo.PageIndex;
                data.PageInfo.PageSize = request.PageInfo.PageSize;
                data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);                
                _redisClient.Set(key, data, time);
                return ResponseResult.Success(data);
            }
            
        }
    }
}
