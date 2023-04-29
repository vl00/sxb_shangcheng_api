using CSRedis;
using Dapper;
using iSchool.Domain.Enum;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.KeyVal;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Course
{
    public class MiniCoursesByInfoMutiFilterQueryHandler : IRequestHandler<MiniCoursesByInfoMutiFilterQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        private readonly IMediator _mediator;
        public MiniCoursesByInfoMutiFilterQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IMediator mediator)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
        }
        public async Task<ResponseResult> Handle(MiniCoursesByInfoMutiFilterQuery request, CancellationToken cancellationToken)
        {

            //判断参数合法性
            //if (!Enum.IsDefined(typeof(CourseTypeEnum), request.CourseType))
            //{
            //    throw new CustomResponseException("访问已被拒绝");
            //}
            await Task.CompletedTask;
            string key = string.Format(CacheKeys.MiniCourseCenterV1, string.Join(',', request.CatogroyIds ?? new List<int>()), string.Join(',', request.AgeGroupId ?? new List<int>()), request.PageIndex + "&" + request.PageSize, request.Sort, request.CourseType, request.PriceMin, request.PriceMax);
            var data = new CoursesByOrgIdQueryResponse();
            if (string.IsNullOrEmpty(request.SearchText))
            {

                data = _redisClient.Get<CoursesByOrgIdQueryResponse>(key);
                if (data != null)
                {
                    return ResponseResult.Success(data);
                }
            }
            data = GetList(request);



            if (string.IsNullOrEmpty(request.SearchText))//搜索不放缓存
            {
                _redisClient.Set(key, data, time);
            }
            return ResponseResult.Success(data);


        }

        public CoursesByOrgIdQueryResponse GetList(MiniCoursesByInfoMutiFilterQuery request)
        {
            var dy = new DynamicParameters();
            #region Where


            string sqlWhere = $@"   and c.IsInvisibleOnline=0  and o.IsValid=1  and o.status=1 and c.status=1 and o.authentication=1  ";
            if (request.CourseType>0)
            {
                sqlWhere += $"and c.type={request.CourseType}";
            }
            
            //价格区间
            if (request.PriceMin > 0)
            {
                dy.Add("@minprice", request.PriceMin);
                sqlWhere += $"  and c.[price]>@minprice ";

            }
            if (request.PriceMax > 0)
            {
                dy.Add("@maxprice", request.PriceMax);
                sqlWhere += $"  and c.[price]<@maxprice ";

            }
            //商品类别
            if (request.CatogroyIds != null && request.CatogroyIds.Count > 0)
            {
                dy.Add("@CatogryIds", request.CatogroyIds);
                sqlWhere += $"  and AA.[catogoryid] in @CatogryIds  ";
            }

            //年龄段
            if (request.AgeGroupId != null && request.AgeGroupId.Count > 0)
            {
                var listAge = new List<int>() { };
                foreach (var item in request.AgeGroupId)
                {
                    if (Enum.IsDefined(typeof(AgeGroup), item))
                    {
                        var ages_str = EnumUtil.GetDesc((AgeGroup)item).Split('-');
                        var minAge = Convert.ToInt32(ages_str[0]);
                        listAge.Add(minAge);
                        var maxAge = Convert.ToInt32(ages_str[1]);
                        listAge.Add(maxAge);

                    }

                }
                dy.Add("@minAge", listAge.Min());
                dy.Add("@maxAge", listAge.Max());
                sqlWhere += @$"   and ( (c.minage>=@minAge and c.maxage<=@maxAge)or (c.minage<=@minAge and c.maxage>=@minAge)or (c.minage<=@maxAge and c.maxage>=@maxAge)) and c.maxage>0 ";
                //dy.Add("@AgeGroupId", request.AgeGroupId);
                //sqlWhere += $" and age=@AgeGroupId ";

            }
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                sqlWhere += $" and  ( c.name like '%{request.SearchText}%' or o.name  like '%{request.SearchText}%' or c.title like '%{request.SearchText}%' )";
            }
            if (null != request.Types && request.Types.Count > 0)
            {  //过滤掉新人专享和限时折扣
                //sqlWhere += " and c.LimitedTimeOffer=0 and c.NewUserExclusive=0 ";
                var mutiFilte="";
                foreach (var item in request.Types)
                {
                    switch (item)
                    {
                        case (int)CourseFilterCutomizeTypeV1.ForNew:
                            if (string.IsNullOrEmpty(mutiFilte))
                                mutiFilte += $"  c.NewUserExclusive=1 ";
                            else
                                mutiFilte += $" or c.NewUserExclusive=1 ";
                            break;
                        case (int)CourseFilterCutomizeTypeV1.HotRank:
                            if (string.IsNullOrEmpty(mutiFilte))
                                mutiFilte += $"  c.IsExplosions=1 ";
                            else
                                mutiFilte += $" or c.IsExplosions=1 ";

                            break;
                        case (int)CourseFilterCutomizeTypeV1.LimitTime:
                            if (string.IsNullOrEmpty(mutiFilte))
                                mutiFilte += $"  c.LimitedTimeOffer=1 ";
                            else
                                mutiFilte += $" or c.LimitedTimeOffer=1 ";

                            break;

                    }
                }
                sqlWhere+= $"and ({mutiFilte})";
                

            }

            //精选 优先上架时间，然后销量进行排序取10条，滚到底部后自动加载
            var sortFilter = "order by SetTop desc, LastOnShelfTime desc,sellcount desc";
            // var sortFilter = "order by authentication desc,no desc,CreateTime desc";
            switch (request.Sort)
            {
                case CourseFilterSortType.Default://综合排序
                    break;
                case CourseFilterSortType.New:
                    sortFilter = "order by DATEDIFF(s,GETDATE(),ISNULL(LastOnShelfTime,CreateTime)) desc,[no] desc";
                    break;
                case CourseFilterSortType.NewAsc:
                    sortFilter = "order by DATEDIFF(s,GETDATE(),ISNULL(LastOnShelfTime,CreateTime)) asc,[no] asc ";
                    break;
                case CourseFilterSortType.PriceLowToHigh:
                    sortFilter = "order by Price ";
                    break;
                case CourseFilterSortType.PriceHighToLow:
                    sortFilter = "order by Price  DESC";
                    break;
                case CourseFilterSortType.SaleVolume:
                    sortFilter = "order by sellcount desc,[no] desc";
                    break;
                case CourseFilterSortType.SaleVolumeAsc:
                    sortFilter = "order by sellcount asc,[no] asc ";
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
SELECT distinct	c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.CreateTime,c.LastOnShelfTime,c.sellcount,c.CanNewUserReward,c.SetTop,c.NewUserExclusive,c.CommodityTypes  
from [dbo].[Course] c left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1  	left join (SELECT id, value AS [catogoryid] FROM [Course]CROSS APPLY OPENJSON([CommodityTypes]))	AA on  c.id=AA.id
                          where 1=1   {sqlWhere} 
                          ) t ) TT  where rownum> (@PageIndex-1)*@PageSize ;";
            string sqlPage = $@"
                            select COUNT(1) as TotalCount  from
                           (    SELECT distinct	c.id
                            from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid  and c.IsValid=1 left join (SELECT id, value AS [catogoryid] FROM [Course]CROSS APPLY OPENJSON([CommodityTypes]))	AA on  c.id=AA.id
                            where 1=1 {sqlWhere} 
                            )T1 ;";

            #region 种草数量排序查询独立的语句
            if (request.Sort == CourseFilterSortType.GrassCountDesc || request.Sort == CourseFilterSortType.GrassCountAsc)
            {
                sql = $@" 
SELECT distinct	c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.CreateTime,c.LastOnShelfTime,c.sellcount,c.CanNewUserReward,c.SetTop,c.NewUserExclusive 
,c.CommodityTypes from [dbo].[Course] c left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1 left join (SELECT id, value AS [catogoryid] FROM [Course]CROSS APPLY OPENJSON([CommodityTypes]))	AA on  c.id=AA.id
where 1=1 and   c.id in (SELECT c.id FROM dbo.Course AS c --这里有个隐患，就是用in，这10条数据未能正确按真正的排序返回，但是前端不显示数量，不影响
LEFT JOIN dbo.EvaluationBind AS  bind ON c.id=bind.courseid
 LEFT JOIN dbo.Evaluation eval ON  bind.evaluationid=eval.id  left join [dbo].[Organization]
o on o.id=c.orgid and c.IsValid=1 left join (SELECT id, value AS [catogoryid] FROM [Course]CROSS APPLY OPENJSON([CommodityTypes]))
AA on  c.id=AA.id
 WHERE 1=1  {sqlWhere} 
GROUP BY c.id ORDER BY COUNT(bind.id)  {(request.Sort == CourseFilterSortType.GrassCountDesc ? "Desc,c.[id] desc" : "asc,c.[id] asc")}  OFFSET (@pageIndex-1)*@pageSize ROWS FETCH NEXT @pageSize ROWS ONLY )    {sqlWhere} "
                     ;


            }
            #endregion

            var data = new CoursesByOrgIdQueryResponse();
            data.CoursesDatas = new List<CoursesData>();
            var dBDatas = _unitOfWork.Query<CoursesDataDB>(sql, dy).ToList();
            if (dBDatas != null)
            {
                //var listCatogry = (List<LevelThreeCatogoryVm>)_mediator.Send(new CatogryQuery() { Root = 3 }).Result.Data;
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
                    //商品分类标签  20211224 产品说不用展示分类了
                    //if (null != listCatogry && listCatogry.Count > 0&&!string.IsNullOrEmpty(course.CommodityTypes))
                    //{
                    //    var cs = JArray.Parse(course.CommodityTypes);
                    //    foreach (var item in cs)
                    //    {
                    //        var c = listCatogry.FirstOrDefault(x => x.Key ==Convert.ToInt32(item));
                    //        if(null!=c) Tags.Add(c.Value);

                    //    }

                    //}
                    if (request.CourseType == 1)
                    {

                        //低价体验
                        if (course.Price <= 10)
                            Tags.Add("低价体验");
                    }
                    else {
                        if (course.NewUserExclusive)
                            Tags.Add("新人专享");
                        if (course.CanNewUserReward)
                            Tags.Add("新人立返");
                        if (course.LimitedTimeOffer)
                            Tags.Add("限时补贴");
                    }

                  
                  
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
                        CanNewUserReward = course.CanNewUserReward

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
