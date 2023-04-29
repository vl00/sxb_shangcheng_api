using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class MiniCourseDetailExtendQueryHandler : IRequestHandler<MiniCourseDetailExtendQuery, MiniCourseDetailExtendDto>
    {
        private readonly IRepository<Domain.Course> _coureseRepository;
        private readonly IRepository<Domain.CourseNotices> _courseNoticesRepository;
        private readonly IRepository<Evaluation> _evaluationRepository;
        private readonly OrgUnitOfWork _unitOfWork;
        private readonly CSRedisClient _redis;

        public MiniCourseDetailExtendQueryHandler(
            IRepository<Domain.Course> coureseRepository,
            IRepository<CourseNotices> courseNoticesRepository,
            IRepository<Evaluation> evaluationRepository,
            IOrgUnitOfWork unitOfWork,
            CSRedisClient redis)
        {
            _coureseRepository = coureseRepository;
            _courseNoticesRepository = courseNoticesRepository;
            _evaluationRepository = evaluationRepository;
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redis = redis;
        }

        public Task<MiniCourseDetailExtendDto> Handle(MiniCourseDetailExtendQuery request, CancellationToken cancellationToken)
        {
            //获取课程信息
            var course = _coureseRepository.Get(p => p.IsValid == true && p.Status == 1 && p.No == request.No);
            if (course == null)
                throw new CustomResponseException("暂无该课程！");

            var dto = new MiniCourseDetailExtendDto();
            var key = string.Format(CacheKeys.CourseExtend, request.No);
            if (_redis.Exists(key))
            {
                dto = _redis.Get<MiniCourseDetailExtendDto>(key);
            }
            else
            {
                var tags = new List<string>();
                //年龄标签
                if (course.Minage != null && course.Maxage != null)
                {
                    tags.Add($"{course.Minage}-{course.Maxage}岁");
                }
                else if (course.Minage != null && course.Maxage == null)
                {
                    tags.Add($"大于{course.Minage}岁");
                }
                else if (course.Maxage != null && course.Minage == null)
                {
                    tags.Add($"小于{course.Maxage}岁");
                }

                //科目标签
                //if (course.Subject != null)
                //    tags.Add(EnumUtil.GetDesc((SubjectEnum)course.Subject.Value));

                //低价体验
                if (course.Price <= 100)
                    tags.Add("低价体验");

                if (course.Price == 0)
                    tags.Add("免费");

                dto.Tags = tags;

                //获取课程公告
                var notices = _courseNoticesRepository
                    .GetAll(p => p.IsValid == true && p.CourseId == course.Id)
                    .OrderBy(p => p.Sort).Select(p => new CourseNoticeItem
                    {
                        Title = p.Title,
                        Content = p.Content
                    });
                dto.Notices = notices;


                _redis.Set(key, dto, 60 * 30);
            }
            var sql = @"
            SELECT DISTINCT TOP (9) eval.id,
                           eval.title,
                           eval.cover,
                           eval.isPlaintext,
                           eval.mode,
                           eval.stick,
                           eval.userid,
                           eval.likes,
                           eval.shamlikes,
                           eval.status,
                           eval.commentcount,
                           eval.collectioncount,
                           eval.viewcount,
                           eval.crawlerId,
                           eval.MTime,
                           eval.CreateTime,
                           eval.Creator,
                           eval.ModifyDateTime,
                           eval.Modifier,
                           eval.IsValid,
                           eval.Completion,
                           eval.No,
                           eval.IsOfficial,
                           eval.HasVideo,
                           eval.SharedTime FROM dbo.Evaluation AS eval
			               LEFT JOIN dbo.EvaluationBind bind ON eval.id=bind.evaluationid AND bind.IsValid=1
			                WHERE eval.IsValid=1 AND  eval.status=1 AND ( eval.HasVideo=1 OR  eval.isPlaintext=0)
				             AND  bind.courseid=@courseid
			               ORDER BY eval.CreateTime DESC, eval.likes DESC";

            var covers = _unitOfWork.Query<Evaluation>(sql,
                new { courseid = course.Id })
                .Select(p => new EvaluationCover
                {
                    Id = p.Id,
                    Id_s = UrlShortIdUtil.Long2Base32(p.No),
                    Cover = p.Cover,
                    IsPicture = !p.HasVideo
                });
            dto.Covers = covers;
            return Task.FromResult(dto);
        }
    }
}
