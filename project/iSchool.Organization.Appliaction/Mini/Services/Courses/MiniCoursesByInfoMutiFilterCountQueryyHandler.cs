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
    public class MiniCoursesByInfoMutiFilterCountQueryyHandler : IRequestHandler<MiniCoursesByInfoMutiFilterCountQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        public MiniCoursesByInfoMutiFilterCountQueryyHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(MiniCoursesByInfoMutiFilterCountQuery request, CancellationToken cancellationToken)
        {


            await Task.CompletedTask;

            var count = GoodThingCount(request);

            return ResponseResult.Success(count);


        }

        public int GoodThingCount(MiniCoursesByInfoMutiFilterCountQuery request)
        {
            var dy = new DynamicParameters();
            #region Where

            string sqlWhere = $@" where 1=1 and c.IsInvisibleOnline=0  and o.IsValid=1  and o.status=1 and c.status=1 and o.authentication=1 ";
            if (request.CourseType > 0)
            {
                sqlWhere += $" and c.type={request.CourseType}";
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
                var mutiFilte = "";
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
                sqlWhere += $"and ({mutiFilte})";


            }

            #endregion
            string sqlPage = $@"
                            select COUNT(1) as TotalCount  from
                           (    SELECT distinct	c.id
                            from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid  and c.IsValid=1 left join (SELECT id, value AS [catogoryid] FROM [Course]CROSS APPLY OPENJSON([CommodityTypes]))	AA on  c.id=AA.id
                            {sqlWhere} 
                            )T1 ;";

            return _unitOfWork.Query<int>(sqlPage, dy).FirstOrDefault();

        }
    }
}
