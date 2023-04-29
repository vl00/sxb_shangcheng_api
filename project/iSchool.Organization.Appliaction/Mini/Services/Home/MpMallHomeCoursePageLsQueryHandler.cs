using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class MpMallHomeCoursePageLsQueryHandler : IRequestHandler<MpMallHomeCoursePageLsQuery, CoursesByOrgIdQueryResponse>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redisClient;
        IConfiguration _config;

        public MpMallHomeCoursePageLsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redisClient = redis;
            this._config = config;
        }

        public async Task<CoursesByOrgIdQueryResponse> Handle(MpMallHomeCoursePageLsQuery query, CancellationToken cancellation)
        {
            await Task.CompletedTask;
            string key = string.Format(CacheKeys.MpMallHomeCoursePageLs, query.PageIndex);
            var data = _redisClient.Get<CoursesByOrgIdQueryResponse>(key);
            if (data != null)
            {
                return data;
            }
            data = new CoursesByOrgIdQueryResponse();

            var dy = new DynamicParameters();
            #region Where
            string sqlWhere = $@" where 1=1 and c.IsInvisibleOnline=0  and o.IsValid=1  and o.status=1 and c.status=1 {$"and c.type={CourseTypeEnum.Goodthing.ToInt()}".If(query.ExcludeCourseType1)} ";
            //过滤掉新人专享和限时折扣
            sqlWhere += " and c.LimitedTimeOffer=0 and c.NewUserExclusive=0 ";
            //精选 优先上架时间，然后销量进行排序取10条，滚到底部后自动加载
            var sortFilter = "order by c.SetTop DESC, c.LastOnShelfTime desc,c.sellcount desc";
            dy.Add("@PageIndex", query.PageIndex);
            dy.Add("@PageSize", query.PageSize);
            #endregion

            string sql = $@" 
                        select top {query.PageSize} * 
                        from(
                        	select ROW_NUMBER() over({sortFilter}) rownum,c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive  from [dbo].[Course] c
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
            var dBDatas = _orgUnitOfWork.Query<CoursesDataDB>(sql, dy).ToList();
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
                        NewUserExclusive = course.NewUserExclusive
                    };


                    data.CoursesDatas.Add(addM);
                }
            }
            data.PageInfo = new PageInfoResult();
            data.PageInfo = _orgUnitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();
            data.PageInfo.PageIndex = query.PageIndex;
            data.PageInfo.PageSize = query.PageSize;
            data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);

            return data;
        }

    }
}
