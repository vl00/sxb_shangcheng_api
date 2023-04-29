using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Lables;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 课程标签列表业务
    /// </summary>
    public class CoursesLablesByInfoQueryHandler:IRequestHandler<CoursesLablesByInfoQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        public CoursesLablesByInfoQueryHandler (IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(CoursesLablesByInfoQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var dy = new DynamicParameters();
            #region Where

            string sqlWhere = $@" where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and org.status={OrganizationStatusEnum.Ok.ToInt()}  and c.IsInvisibleOnline=0"; // and c.type={CourseTypeEnum.Course.ToInt()}

            //课程标题
            if (!string.IsNullOrEmpty(request.Title))
            {
                sqlWhere += $" and c.title like '%{request.Title}%'  ";
            }

            //机构名称
            if (!string.IsNullOrEmpty(request.OrgName))
            {
                sqlWhere += $"  and org.[name] like '%{request.OrgName}%' ";
            }

            //课程科目
            if (request.SubjectId != null && Enum.IsDefined(typeof(SubjectEnum), request.SubjectId))
            {
                dy.Add("@SubjectId", request.SubjectId);
                sqlWhere += $" and c.subject=@SubjectId  ";
            }   
            

            dy.Add("@PageIndex", request.PageInfo.PageIndex);
            dy.Add("@PageSize", request.PageInfo.PageSize);
            #endregion

            string sql = $@" 
                        select top {request.PageInfo.PageSize} * 
                        from(
                        	select ROW_NUMBER() over(order by c.LastOnShelfTime) rownum, c.* from Course c left join Organization org on c.orgid=org.id and org.IsValid=1                                                       
                            {sqlWhere} 
                        )TT where rownum> (@PageIndex-1)*@PageSize ;";
            string sqlPage = $@"
                            select COUNT(1) as TotalCount, {request.PageInfo.PageIndex} as PageIndex,{request.PageInfo.PageSize} as PageSize
                            from Course c left join Organization org on c.orgid=org.id and org.IsValid=1
                            {sqlWhere} 
                            ;";                   
            var courses = _unitOfWork.Query<Domain.Course>(sql, dy)?.ToList();
            if (courses.Any() == false)
            {
                return ResponseResult.Failed("无符合条件的数据");
            }
            var data = new CoursesLablesResponse();
            data.ListLables = courses.Select(_ => new CourseLable() { Id=_.Id,Id_s= UrlShortIdUtil.Long2Base32(Convert.ToInt64(_.No)), Price=_.Price, Title=_.Title
                ,CoverUrl=(string.IsNullOrEmpty(_.Banner)?null:JsonSerializationHelper.JSONToObject<List<string>>(_.Banner)?.FirstOrDefault() )}).ToList();
            
            data.PageInfo = new PageInfoResult();
            data.PageInfo = _unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();      
            data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);
            
            return ResponseResult.Success(data);
        }
    }
}
