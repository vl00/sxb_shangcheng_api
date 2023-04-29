using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    public class ClaimOrganizationRequestHandler : IRequestHandler<ClaimOrganizationRequest, ResponseResult>
    {
        OrgUnitOfWork unitOfWork;
        public ClaimOrganizationRequestHandler(IOrgUnitOfWork unitOfWork)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public  Task<ResponseResult> Handle(ClaimOrganizationRequest request, CancellationToken cancellationToken)
        {
            var re = @"^1\d{10}$";//正则表达式
            if (!Regex.IsMatch(request.Mobile, re))
            {
                return Task.FromResult(ResponseResult.Failed("手机号码格式不正确！"));
            }
            var dy = new DynamicParameters();
            dy.Add("@id", Guid.NewGuid());
            dy.Add("@orgid", request.Orgid);
            dy.Add("@name", request.Name.Replace(" ","").Trim());
            dy.Add("@mobile", request.Mobile);
            dy.Add("@position", request.Position.Replace(" ", "").Trim());
            dy.Add("@CreateTime", DateTime.Now);
            dy.Add("@Creator", request.Creator);
            dy.Add("@IsValid", 1);

            string insertSql = $@"
            INSERT INTO DBO.Authentication([id], [orgid], [name], [mobile], [position], [CreateTime], [Creator], [IsValid])
            VALUES(@id, @orgid, @name, @mobile, @position, @CreateTime, @Creator, @IsValid);
            ";
            var count = unitOfWork.DbConnection.Execute(insertSql, dy);
          
            if (count == 1)
            {
                return Task.FromResult(ResponseResult.Success("提交成功"));
            }
            else
            {
                return Task.FromResult(ResponseResult.Failed("认领失败"));
            }
        }
    }
}
