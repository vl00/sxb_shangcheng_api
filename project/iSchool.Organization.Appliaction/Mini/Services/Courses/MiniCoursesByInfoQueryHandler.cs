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
    public class MiniGoodThingByInfoQueryHandler : IRequestHandler<MiniCoursesByInfoQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        public MiniGoodThingByInfoQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(MiniCoursesByInfoQuery request, CancellationToken cancellationToken)
        {

            //判断参数合法性
            //if (!Enum.IsDefined(typeof(CourseTypeEnum), request.CourseType))
            //{
            //    throw new CustomResponseException("访问已被拒绝");
            //}
            await Task.CompletedTask;
            string key = string.Format(CacheKeys.MiniCourseCenter, request.SubjectId, request.AgeGroupId, request.PageIndex + "&" + request.PageSize, request.Sort, request.Type, request.CourseType, request.GoodThingType);
            var data = new CoursesByOrgIdQueryResponse();
            //if (string.IsNullOrEmpty(request.SearchText))
            //{

            //    data = _redisClient.Get<CoursesByOrgIdQueryResponse>(key);
            //    if (data != null)
            //    {
            //        return ResponseResult.Success(data);
            //    }
            //}
            if (1 == request.CourseType)
            {
                data = CourseList(request);

            }
            else if (2 == request.CourseType)
            {
                data = GoodThingList(request);
            }
            if (string.IsNullOrEmpty(request.SearchText))//搜索不放缓存
            {
                _redisClient.Set(key, data, time);
            }
            return ResponseResult.Success(data);


        }
        public CoursesByOrgIdQueryResponse CourseList(MiniCoursesByInfoQuery request)
        {
            var dy = new DynamicParameters();
            #region Where

            string sqlWhere = $@" where 1=1 and c.IsInvisibleOnline=0  and o.IsValid=1  and o.status=1 and c.status=1 and o.authentication=1 and  c.type={request.CourseType}";

            //过滤掉新人专享和限时折扣
            sqlWhere += " and c.LimitedTimeOffer=0 and c.NewUserExclusive=0 ";
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
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                sqlWhere += $" and  ( c.name like '%{request.SearchText}%' or o.name  like '%{request.SearchText}%' or c.title like '%{request.SearchText}%' )";
                //sqlWhere += $" and  ( c.title like '%{request.SearchText}%' or o.name  like '%{request.SearchText}%'  )";
            }
            switch (request.Type)
            {
                case CourseFilterCutomizeType.SystemCourse:
                    sqlWhere += $" and c.IsSystemCourse=1 ";
                    break;
                case CourseFilterCutomizeType.OfficialAuth:
                    sqlWhere += $" and o.authentication=1 ";
                    break;
                case CourseFilterCutomizeType.Free:
                    sqlWhere += $" and c.price=0 ";
                    break;
                case CourseFilterCutomizeType.LowPriceExpirence:
                    sqlWhere += $" and c.price<=10 ";//产品定义为10
                    break;
                case CourseFilterCutomizeType.Default:
                    break;
            }
            var sortFilter = "order by c.SetTop desc, o.authentication desc,c.no desc,c.CreateTime desc";
            switch (request.Sort)
            {
                case CourseFilterSortType.Default:
                    break;

                case CourseFilterSortType.New:
                    sortFilter = "order by DATEDIFF(s,GETDATE(),ISNULL(c.LastOnShelfTime,c.CreateTime)) desc";
                    break;
                case CourseFilterSortType.PriceLowToHigh:
                    sortFilter = "order by c.Price ";
                    break;
                case CourseFilterSortType.PriceHighToLow:
                    sortFilter = "order by c.Price  DESC";
                    break;
                case CourseFilterSortType.SaleVolume:
                    sortFilter = "order by c.sellcount desc";
                    break;
                default:
                    break;
            }
            dy.Add("@PageIndex", request.PageIndex);
            dy.Add("@PageSize", request.PageSize);
            #endregion

            string sql = $@" 
                        select top {request.PageSize} * 
                        from(
                        	select ROW_NUMBER() over({sortFilter}) rownum,c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.SetTop  from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1  
                            {sqlWhere} 
                        )TT where rownum> (@PageIndex-1)*@PageSize ;";
            string sqlPage = $@"
                            select COUNT(1) as TotalCount 
                            from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid  and c.IsValid=1  
                            {sqlWhere} 
                            ;";
            var data = new CoursesByOrgIdQueryResponse();
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
                        LastOffShelfTime = course.LastOffShelfTime.UnixTicks()

                    };


                    data.CoursesDatas.Add(addM);
                }
            }
            data.PageInfo = new PageInfoResult();
            data.PageInfo = _unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();
            data.PageInfo.PageIndex = request.PageIndex;
            data.PageInfo.PageSize = request.PageSize;
            data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);
            return data;

        }
        public CoursesByOrgIdQueryResponse GoodThingList(MiniCoursesByInfoQuery request)
        {
            var dy = new DynamicParameters();
            #region Where

            string sqlWhere = $@" where 1=1 and c.IsInvisibleOnline=0  and o.IsValid=1  and o.status=1 and c.status=1 and o.authentication=1  and c.type={request.CourseType}";
            //过滤掉新人专享和限时折扣
            sqlWhere += " and c.LimitedTimeOffer=0 and c.NewUserExclusive=0 ";

            //好物类别
            if (request.GoodThingType != null)
            {
                dy.Add("@Type", request.GoodThingType);
                sqlWhere += $"  and AA.[types]=@Type  ";
            }

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
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                sqlWhere += $" and  ( c.name like '%{request.SearchText}%' or o.name  like '%{request.SearchText}%' or c.title like '%{request.SearchText}%' )";
            }
            switch (request.Type)
            {
                case CourseFilterCutomizeType.SystemCourse:
                    sqlWhere += $" and c.IsSystemCourse=1 ";
                    break;
                case CourseFilterCutomizeType.OfficialAuth:
                    sqlWhere += $" and o.authentication=1 ";
                    break;
                case CourseFilterCutomizeType.Free:
                    sqlWhere += $" and c.price=0 ";
                    break;
                case CourseFilterCutomizeType.LowPriceExpirence:
                    sqlWhere += $" and c.price<=10 ";//产品定义为10
                    break;
                case CourseFilterCutomizeType.Default:
                    break;
            }
             //精选 优先上架时间，然后销量进行排序取10条，滚到底部后自动加载
             var sortFilter = "order by SetTop desc, LastOnShelfTime desc,sellcount desc";
           // var sortFilter = "order by authentication desc,no desc,CreateTime desc";
            switch (request.Sort)
            {
                case CourseFilterSortType.Default://综合排序
                    break;
                case CourseFilterSortType.New:
                    sortFilter = "order by DATEDIFF(s,GETDATE(),ISNULL(LastOnShelfTime,CreateTime)) desc";
                    break;
                case CourseFilterSortType.PriceLowToHigh:
                    sortFilter = "order by Price ";
                    break;
                case CourseFilterSortType.PriceHighToLow:
                    sortFilter = "order by Price  DESC";
                    break;
                case CourseFilterSortType.SaleVolume:
                    sortFilter = "order by sellcount desc";
                    break;
                default:
                    break;
            }
            dy.Add("@PageIndex", request.PageIndex);
            dy.Add("@PageSize", request.PageSize);
            #endregion

            string sql = $@" 
                        select top {request.PageSize} * 
                        from(
                        	select ROW_NUMBER() over({sortFilter}) rownum,*  
											from (
SELECT distinct	c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.CreateTime,c.LastOnShelfTime,c.sellcount,c.CanNewUserReward,c.SetTop,c.NewUserExclusive  from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1  	left join (SELECT id, value AS [types] FROM [Course]CROSS APPLY OPENJSON([GoodthingTypes]))	AA on  c.id=AA.id
                            {sqlWhere} 
                          ) t ) TT  where rownum> (@PageIndex-1)*@PageSize ;";
            string sqlPage = $@"
                            select COUNT(1) as TotalCount  from
                           (    SELECT distinct	c.id
                            from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid  and c.IsValid=1 left join (SELECT id, value AS [types] FROM [Course]CROSS APPLY OPENJSON([GoodthingTypes]))	AA on  c.id=AA.id
                            {sqlWhere} 
                            )T1 ;";
            var data = new CoursesByOrgIdQueryResponse();
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
                    if (course.Subject != null)
                        Tags.Add(EnumUtil.GetDesc((SubjectEnum)course.Subject.Value));


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
                        CanNewUserReward=course.CanNewUserReward

                    };


                    data.CoursesDatas.Add(addM);
                }
            }
            data.PageInfo = new PageInfoResult();
            data.PageInfo = _unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();
            data.PageInfo.PageIndex = request.PageIndex;
            data.PageInfo.PageSize = request.PageSize;
            data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);
            return data;
        }
    }
}
