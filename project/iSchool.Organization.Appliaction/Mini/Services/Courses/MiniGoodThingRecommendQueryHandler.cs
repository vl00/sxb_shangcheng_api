using CSRedis;
using Dapper;
using iSchool.Domain.Enum;
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
    public class MiniGoodThingRecommendQueryHandler : IRequestHandler<MiniGoodThingRecommendQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        public MiniGoodThingRecommendQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(MiniGoodThingRecommendQuery request, CancellationToken cancellationToken)
        {


            await Task.CompletedTask;
            if (request.Type != 2)
            {
                return await GetList(request);
            }
            else if (request.Type == 2)
            {
                return await GeRecommendSameAgeList(request);
            }
            return null;

        }
        public async Task<ResponseResult> GetList(MiniGoodThingRecommendQuery request)
        {
            string key = string.Format(CacheKeys.MiniGoodThingCenter, request.Type, request.PageIndex);
            var data = _redisClient.Get<CoursesByOrgIdQueryResponse>(key);
            if (data != null)
            {
                return ResponseResult.Success(data);
            }
            data = new CoursesByOrgIdQueryResponse();

            var dy = new DynamicParameters();
            #region Where
            string sqlWhere = $@" where 1=1 and c.IsInvisibleOnline=0  and o.IsValid=1  and o.status=1 and c.status=1 and o.authentication=1  and c.type={CourseTypeEnum.Goodthing.ToInt()}";
            //过滤掉新人专享和限时折扣
            sqlWhere += " and c.LimitedTimeOffer=0 and c.NewUserExclusive=0 ";
            var sortFilter = "";
            switch (request.Type)
            {
                case 0://精选 优先上架时间，然后销量进行排序取10条，滚到底部后自动加载
                    sortFilter = "order by c.SetTop DESC, c.LastOnShelfTime desc,c.sellcount desc";
                    break;
                case 1://爆款 显示后台上货架时标注为爆款推荐的商品，最多取爆款里售卖数量最高的前6个
                    sqlWhere += $" and c.IsExplosions=1 ";
                    sortFilter = "order by c.ModifyDateTime desc, isnull(c.sellcount,0) desc";
                    break;

            }
            dy.Add("@PageIndex", request.PageIndex);
            dy.Add("@PageSize", request.PageSize);
            #endregion

            string sql = $@" 
                        select top {request.PageSize} * 
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
                        NewUserExclusive = course.NewUserExclusive
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
        /// <summary>
        /// 同年龄段推荐，只取6条数据，每次查询返回结果不一样
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseResult> GeRecommendSameAgeList(MiniGoodThingRecommendQuery request)
        {
            string key = string.Format(CacheKeys.MiniGoodThingCenter, request.Type, 1);
            var old = _redisClient.Get<CoursesByOrgIdQueryResponse>(key);
            var notInCludeIds = new List<Guid>() {};
            if (null != request.CourseId)//推荐排除自己
            {
                notInCludeIds.Add(request.CourseId.Value);
            }
            if (old != null)
            {
                notInCludeIds.AddRange(old.CoursesDatas.Select(x => x.Id));
            }
            var r = new CoursesByOrgIdQueryResponse();

            var dy = new DynamicParameters();
            #region Where
            string sqlWhere = $@" where 1=1 and c.IsInvisibleOnline=0  and o.IsValid=1  and o.status=1 and c.status=1 and o.authentication=1  and c.type={request.CourseType}";
            if (notInCludeIds.Count > 0)
            {
                sqlWhere += $" and c.id not in @ids";
                dy.Add("@ids", notInCludeIds);
            }
            var courseM = _unitOfWork.QueryFirstOrDefault<iSchool.Organization.Domain.Course>($"select * from [dbo].[Course] where id=@id", new { id=request.CourseId});
            if(null==courseM) return  ResponseResult.Failed("无此商品");
            //同年龄段推荐
            if (null != courseM.Age)
            {
                if (Enum.IsDefined(typeof(AgeGroup), courseM.Age))
                {
                    var ages_str = EnumUtil.GetDesc((AgeGroup)courseM.Age).Split('-');
                    var minAge = Convert.ToInt32(ages_str[0]);

                    var maxAge = Convert.ToInt32(ages_str[1]);
                    dy.Add("@minAge", minAge);
                    dy.Add("@maxAge", maxAge);
                    sqlWhere += @$"   and ( (c.minage>=@minAge and c.maxage<=@maxAge)or (c.minage<=@minAge and c.maxage>=@minAge)or (c.minage<=@maxAge and c.maxage>=@maxAge)) and c.maxage>0 ";

                }

            }
          


         
            #endregion

            string sql = $@" 
                        select top 6  c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive  from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1
                            {sqlWhere}   ORDER BY NEWID() --随机
                         ;";


            r.CoursesDatas = new List<CoursesData>();
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
                        NewUserExclusive = course.NewUserExclusive
                    };


                    r.CoursesDatas.Add(addM);
                }
            }


            return ResponseResult.Success(r);
        }
    }
}
