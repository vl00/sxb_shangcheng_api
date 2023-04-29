using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 机构--相关课程
    /// </summary>
    public class OrganizationRelatedCoursesQueryHandler : IRequestHandler<OrganizationRelatedCoursesQuery, List<RelatedCourses>>
    {
        OrgUnitOfWork unitOfWork;
        CSRedisClient cSRedis;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OrganizationRelatedCoursesQueryHandler(IOrgUnitOfWork unitOfWork,CSRedisClient cSRedis, IHttpContextAccessor httpContextAccessor)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.cSRedis = cSRedis;
            this._httpContextAccessor = httpContextAccessor;
        }


        public async Task<List<RelatedCourses>> Handle(OrganizationRelatedCoursesQuery request, CancellationToken cancellationToken)
        {

            var agentType = 0;
            //ios不展示课程
            if (UserAgentUtils.IsIos(_httpContextAccessor.HttpContext))
            {
                agentType = 1;
            }
            await Task.CompletedTask;
            string key = CacheKeys.CoursesByOrg.FormatWith(request.OrganizationId, request.PageInfo.PageIndex + "&" + request.PageInfo.PageSize, agentType);
            var response = cSRedis.Get<List<RelatedCourses>>(key);
            if (response != null)
            {
                return response;
            }
            else
            {
                var dy = new DynamicParameters();
                dy.Add("@OrganizationId", request.OrganizationId);
                dy.Add("@PageIndex", request.PageInfo.PageIndex);
                dy.Add("@PageSize", request.PageInfo.PageSize);

                string sql = $@" 
                        select top {request.PageInfo.PageSize} * 

                        from(
                        	select ROW_NUMBER() over(order by c.id desc) rownum,c.no AS CNO,c.id,c.name,c.banner,c.title,c.price,c.origprice,c.stock,o.authentication  from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1 and o.IsValid=1 
                            where o.id=@OrganizationId  and c.status=1 and o.status=1 and c.IsInvisibleOnline=0 {(agentType == 1 ? "and c.type=2" : "")}
                        )TT where rownum>((@PageIndex-1)*@PageSize)";
                response = unitOfWork.Query<RelatedCourses>(sql, dy).ToList();
                for (int i = 0; i < response.Count; i++)
                {
                    response[i].CNO= UrlShortIdUtil.Long2Base32(Convert.ToInt64(response[i].CNO));
                }
                cSRedis.Set(key, response,60*60);
                return response;
            }
        }
    }
}
