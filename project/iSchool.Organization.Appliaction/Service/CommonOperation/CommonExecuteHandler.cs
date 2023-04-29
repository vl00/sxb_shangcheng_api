using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.CommonOperation
{
    /// <summary>
    /// 通用Execute
    /// </summary>
    public class CommonExecuteHandler : IRequestHandler<CommonExecute, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public CommonExecuteHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }
        public Task<ResponseResult> Handle(CommonExecute request, CancellationToken cancellationToken)
        {
           
            if (!string.IsNullOrEmpty(request.Sql))
            {               
                var count = _orgUnitOfWork.DbConnection.Execute(request.Sql, request.Parameters);
                if (count == 1)
                {
                    return Task.FromResult(ResponseResult.Success("操作成功"));
                }
                else
                {
                    return Task.FromResult(ResponseResult.Failed("操作失败！"));
                }
            }
            else
            {
                return Task.FromResult(ResponseResult.Failed("操作失败！"));
            }
        }
    }
}
