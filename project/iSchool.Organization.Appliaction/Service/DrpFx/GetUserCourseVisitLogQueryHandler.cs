using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.DrpFx;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class GetUserCourseVisitLogQueryHandler : IRequestHandler<GetUserCourseVisitLogQuery, List<UserCourseVisitLog>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;
        IUserInfo me;

        public GetUserCourseVisitLogQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config, IUserInfo me,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
            this.me = me;
        }

        public async Task<List<UserCourseVisitLog>> Handle(GetUserCourseVisitLogQuery query, CancellationToken cancellation)
        {
            var r = new List<UserCourseVisitLog>();
            foreach (var userid in query.UserIds)
            {
                var addM = new UserCourseVisitLog() { };
                addM.UserId = userid;
                addM.VisitCourseLog = new List<CourseVisitLogDetail>();
                var logKey = CacheKeys.CourseVisitLog.FormatWith(userid);
                var log = await _redis.GetAsync<List<CourseVisitLog>>(logKey);
                if (null != log && log.Count > 0)
                {

                    var cmd = new CoursesByIdsQuery() { CourseIds = log.Select(x => x.CourseId).ToList() };
                    var listCouseDetail = await _mediator.Send(cmd);
                    if (null != listCouseDetail && null != listCouseDetail.Data)
                    {
                        foreach (var item in (List<CoursesQueryResult>)listCouseDetail.Data)
                        {
                            var source = log.FirstOrDefault(x => x.CourseId == item.Id);
                            var cd = new CourseVisitLogDetail() { CourseId = item.Id, CourseName = item.Title, CourseImgUrl = item.Banner, CourseNo = item.No, CoursePrice = item.Price, CourseOriginPrice = item.OrigPrice, AddTime = source.AddTime };
                            addM.VisitCourseLog.Add(cd);
                        }

                    }



                }
                addM.VisitCourseLog = addM.VisitCourseLog.OrderByDescending(x => x.AddTime).ToList();
                r.Add(addM);
            }


            return r;

        }
    }
}
