using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.Mini.RequestModels.Courses;
using iSchool.Organization.Appliaction.Mini.ResponseModels.Courses;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Mini.Services.Courses
{
    using Dapper;
    public class MiniCoursePointsGoodsQueryHandler : IRequestHandler<MiniCoursePointsGoodsQuery, IEnumerable<MiniCoursePointsGoods>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public MiniCoursePointsGoodsQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<IEnumerable<MiniCoursePointsGoods>> Handle(MiniCoursePointsGoodsQuery request, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            string updateSql = @"--检查新增插入
INSERT INTO tmp_points_course
SELECT
	   Course.id
	  ,abs(CHECKSUM(NEWID())%100+1) 
	  ,GETDATE()
  FROM Course  
  WHERE
  NOT EXISTS(SELECT 1 FROM tmp_points_course WHERE tmp_points_course.COURSEID= Course.id )
--更新今日随机值
UPDATE tmp_points_course SET Random = abs(CHECKSUM(NEWID())%100+1),MidifyTime=GETDATE() WHERE CAST(MidifyTime AS date) != CAST(GETDATE()AS date)";
            await _orgUnitOfWork.DbConnection.ExecuteAsync(updateSql, new { offset = request.Offset, limit = request.Limit });
            string sql = @"
--查询随机结果
SELECT
	   Course.id
      ,Course.[no]
	  ,Course.title
	  ,(SELECT MIN(Origprice) FROM CourseGoods WHERE 
Courseid = Course.id AND SHOW = 1 AND IsValid = 1
AND EXISTS(SELECT 1 FROM CourseGoodsExchange WHERE CourseGoods.Id = CourseGoodsExchange.GoodId AND CourseGoodsExchange.Show = 1 AND CourseGoodsExchange.IsValid = 1) ) origprice
	  ,Course.banner 
	  ,Course.banner_s 
      ,(SELECT MIN(Point) FROM CourseGoodsExchange  WHERE CourseId = Course.id and  show =1 and IsValid =1 ) points
	  ,Course.minage 
	  ,Course.maxage
	  ,Course.[subject]
	  ,Course.Price price
	  ,Course.NewUserExclusive newUserExclusive
	  ,Course.CanNewUserReward canNewUserReward
	  ,Course.LimitedTimeOffer limitedTimeOffer
  FROM Course  
  JOIN tmp_points_course ON tmp_points_course.COURSEID = Course.id
  WHERE
  IsValid =1
  AND 
  [status] = 1
  AND
  Course.IsPointExchange = 1 
  AND
  EXISTS( SELECT 1 FROM CourseGoods WHERE IsValid = 1 AND  Show = 1 AND Courseid = Course.id )
  AND
  EXISTS( SELECT 1 FROM CourseGoodsExchange WHERE IsValid = 1 AND  Show = 1 AND Courseid = Course.id )
ORDER BY tmp_points_course.Random
OFFSET @offset ROWS 
FETCH NEXT @limit ROW ONLY;
";
            var res = await _orgUnitOfWork.QueryAsync(sql, new { offset = request.Offset, limit = request.Limit });
            return res.Select(s => { return (MiniCoursePointsGoods)MapPointsGoods(s); });
        }



        MiniCoursePointsGoods MapPointsGoods(dynamic res)
        {

            var pointGoods = new MiniCoursePointsGoods()
            {
                Id = res.id,
                Title = res.title,
                Origprice = res.origprice ?? 0,
                Points = res.points ?? 0,
                NoStr = UrlShortIdUtil.Long2Base32((long)res.no)

            };
            if (!string.IsNullOrEmpty(res.banner))
                pointGoods.Banner = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(res.banner);
            if (!string.IsNullOrEmpty(res.banner_s))
                pointGoods.Banner_s = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(res.banner_s);

            var Tags = new List<string>();
            //年龄标签
            if (res.minage != null && res.maxage != null)
            {
                Tags.Add($"{res.minage}-{res.maxage}岁");
            }
            else if (res.minage != null && res.maxage == null)
            {
                Tags.Add($"大于{res.minage}岁");
            }
            else if (res.maxage != null && res.minage == null)
            {
                Tags.Add($"小于{res.maxage}岁");
            }

            //科目标签
            if (res.subject != null)
                Tags.Add(EnumUtil.GetDesc((SubjectEnum)res.subject));


            //低价体验
            if (res.price <= 10)
                Tags.Add("低价体验");
            if (res.newUserExclusive)
                Tags.Add("新人专享");
            if (res.canNewUserReward)
                Tags.Add("新人立返");
            if (res.limitedTimeOffer)
                Tags.Add("限时补贴");
            pointGoods.Tags = Tags;
            return pointGoods;
        }
    }
}
