using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class CourseBaseInfoQueryHandler : IRequestHandler<CourseBaseInfoQuery, Domain.Course>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;

        const int cache_exp = 60 * 30;

        public CourseBaseInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis, IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
        }

        public async Task<Domain.Course> Handle(CourseBaseInfoQuery req, CancellationToken cancellation)
        {
            var courseId = req.CourseId;
            string sql = null;
            string rdkNo = null, rdk = null;
            Domain.Course dy = null;

            // 由no确定出id
            if (courseId == default)
            {
                rdkNo = CacheKeys.courseidbyno.FormatWith(req.No);
                var str_Id = await redis.GetAsync<string>(rdkNo);
                if (str_Id != null)
                {
                    courseId = Guid.Parse(str_Id);
                }
            }
            // 由id确定出实体
            if (courseId != default)
            {
                rdk = CacheKeys.CourseBaseInfo.FormatWith(courseId);
                dy = await redis.GetAsync<Domain.Course>(rdk);                
            }
            // 查db
            if (dy == null)
            {
                sql = $@"
select c.* from Course c
where 1=1 {"and c.IsValid=1 and c.status=@status".If(!req.AllowNotValid)} 
{"and c.Id=@Id".If(courseId != default)} {"and c.no=@no".If(courseId == default)}
";
                dy = await unitOfWork.QueryFirstOrDefaultAsync<Domain.Course>(sql, new { no = req.No, Id = courseId, status = CourseStatusEnum.Ok.ToInt() });
                if (dy == null) throw new CustomResponseException($"无效的课程or好物no={req.No}", 404);
                courseId = dy.Id;

                rdkNo ??= CacheKeys.courseidbyno.FormatWith(dy.No);
                rdk ??= CacheKeys.CourseBaseInfo.FormatWith(courseId);
                await redis.StartPipe()
                    .Set(rdkNo, courseId, 60 * 60 * 24 * 365)
                    .Set(rdk, dy, 60 * 60)
                    .EndPipeAsync();
            }
            if (req.No == default) req.No = dy.No;

            return dy;
        }

        
    }
}
