using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Domain.Enum;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{

    /// <summary>
    /// 机构后台--获取课程详情
    /// </summary>
    public class CourseDetailsByIdQueryHandler : IRequestHandler<QueryCourseById, ResponseResult>
    {
        
        OrgUnitOfWork _orgUnitOfWork;        

        public CourseDetailsByIdQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;            
        }

        public async Task<ResponseResult> Handle(QueryCourseById request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var dy = new DynamicParameters()
            .Set("CourseId", request.CourseId);

            string where = "";
            if (!request.IgnoreStatus)//只查询上架
            {
                where += "  and c.status=@status  ";
                dy.Add("@status", CourseStatusEnum.Ok);
            }

            #region 课程详情查询
            string sql = $@" select c.* from [dbo].[Course] c  where  c.id=@CourseId and c.IsValid=1 {where};";
            var dBData = _orgUnitOfWork.DbConnection.Query<Domain.Course>(sql, dy).FirstOrDefault();

            if (dBData == null) throw new CustomResponseException("课程不存在！");
            #endregion
            return ResponseResult.Success(dBData);
        }

    }
}
