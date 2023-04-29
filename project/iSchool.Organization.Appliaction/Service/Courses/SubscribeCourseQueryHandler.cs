using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Courses;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 期待上线
    /// </summary>
    public class SubscribeCourseQueryHandler : IRequestHandler<SubscribeCourseQuery, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;

        public SubscribeCourseQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }


        public Task<ResponseResult> Handle(SubscribeCourseQuery request, CancellationToken cancellationToken)
        {
            var dy = new DynamicParameters();
            dy.Add("@No", request.No);
            string sql = $@"select c.subscribe ,o.name as orgname,c.id  from [dbo].[Course] c left join [dbo].[Organization] o on c.orgid=o.id  and c.IsValid=1 and o.IsValid=1
                            where c.No=@No ;";
            var data= _orgUnitOfWork.DbConnection.Query<SubscribeCourseQueryResult>(sql,dy).FirstOrDefault();
            return Task.FromResult(ResponseResult.Success(data));
            //if(data!=null)
            //{
            //    string updateSql = @";";
            //    //data.Subscribe 
            //    int count= _orgUnitOfWork.DbConnection.Execute(sql, dy);
            //}
            //throw new NotImplementedException();
        }
    }
}
