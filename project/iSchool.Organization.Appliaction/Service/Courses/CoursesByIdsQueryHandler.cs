using CSRedis;
using Dapper;
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
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    ///  学校--根据id集合，查询课程列表
    /// </summary>
    public class CoursesByIdsQueryHandler:IRequestHandler<CoursesByIdsQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        public CoursesByIdsQueryHandler (IOrgUnitOfWork unitOfWork)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
        }
        public async Task<ResponseResult> Handle(CoursesByIdsQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            if (request.CourseIds.Any() == false)
                return ResponseResult.Failed("课程Id不能为空集！");
            string sql = $@" select c.id,c.no,c.name,c.Title,c.banner,c.price,c.origprice,c.stock,o.authentication  from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid
                            where  c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and o.status={OrganizationStatusEnum.Ok.ToInt()} {(1== request.IncludeGoodThing?$" and c.type={CourseTypeEnum.Course.ToInt()}":"")}
                            and c.id in ('{string.Join("','",request.CourseIds)}') ;";

            if (1 == request.IncludeGoodThing)
            { 
            
            }
            var data = _unitOfWork.Query<CoursesQueryResult>(sql, null).ToList();
            if (data?.Any()==true)
            {
                
                for (int i = 0; i < data.Count; i++)
                {
                    data[i].No = UrlShortIdUtil.Long2Base32(Convert.ToInt64(data[i].No));                    
                }
                List<CoursesQueryResult> result = data;
                return ResponseResult.Success(result);

            }
            else
            {
                return ResponseResult.Success("暂无数据");
            }          
            
        }
    }
}
