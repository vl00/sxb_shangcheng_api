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
    /// <summary>
    /// 活动课程
    /// </summary>
    public class MiniActivityCourseListQueryHandler : IRequestHandler<MiniActivityCourseListQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        public MiniActivityCourseListQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(MiniActivityCourseListQuery request, CancellationToken cancellationToken)
        {

            await Task.CompletedTask;
            var data = new CoursesByOrgIdQueryResponse();
            var dy = new DynamicParameters();
            #region Where
            string sqlWhere = $@" where 1=1 and c.IsInvisibleOnline=0  and o.IsValid=1  and o.status=1 and c.status=1 and o.authentication=1  ";
            switch (request.ActivityType)
            {
                case 0://新人专享
                    sqlWhere += " and NewUserExclusive=1";
                    break;
                case 1:
                    sqlWhere += " and LimitedTimeOffer=1 and  c.LastOffShelfTime>GETDATE() ";
                    break;
            }

            dy.Add("@PageIndex", request.PageIndex);
            dy.Add("@PageSize", request.PageSize);
            #endregion

            string sql = $@"
                        select top {request.PageSize} * 
                        from(
                        	select ROW_NUMBER() over(order by c.LastOffShelfTime) rownum,c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive,c.LimitedTimeOffer  from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1  	
                            {sqlWhere} 
                        )TT where rownum> (@PageIndex-1)*@PageSize ;";
            string sqlPage = $@"
                            select COUNT(1) as TotalCount 
                            from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid  and c.IsValid=1 
                            {sqlWhere} 
                            ;";

            data.CoursesDatas = new List<CoursesData>();
            var dBDatas = _unitOfWork.Query<CoursesDataDB>(sql, dy).ToList();
            if (dBDatas != null)
            {
                for (int i = 0; i < dBDatas.Count; i++)
                {
                    var course = dBDatas[i];
                    var Tags = new List<string>();

                    //年龄标签
                    if (course.Minage != null && course.Maxage != null)
                    {
                        Tags.Add($"{course.Minage}-{course.Maxage}岁");
                    }
                    else if (course.Minage != null && course.Maxage == null)
                    {
                        Tags.Add($"大于{course.Minage}岁");
                    }
                    else if (course.Maxage != null && course.Minage == null)
                    {
                        Tags.Add($"小于{course.Maxage}岁");
                    }

                    //科目标签
                    //if (course.Subject != null)
                    //    Tags.Add(EnumUtil.GetDesc((SubjectEnum)course.Subject.Value));


                    //低价体验
                    if (course.Price <= 10)
                        Tags.Add("低价体验");
                    if (course.NewUserExclusive)
                        Tags.Add("新人专享");
                    if (course.CanNewUserReward)
                        Tags.Add("新人立返");
                    if (course.LimitedTimeOffer)
                        Tags.Add("限时补贴");
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
                        Tags = Tags,
                        LastOffShelfTime = course.LastOffShelfTime.UnixTicks(),
                        CanNewUserReward = course.CanNewUserReward,
                        LimitedTimeOffer = course.LimitedTimeOffer,
                        NewUserExclusive = course.NewUserExclusive,

                    };


                    data.CoursesDatas.Add(addM);
                }
            }
            data.PageInfo = new PageInfoResult();
            data.PageInfo = _unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();
            data.PageInfo.PageIndex = request.PageIndex;
            data.PageInfo.PageSize = request.PageSize;
            data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);

            return ResponseResult.Success(data);


        }


    }
}
