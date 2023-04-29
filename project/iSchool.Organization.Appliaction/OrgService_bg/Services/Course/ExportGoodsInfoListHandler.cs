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
    public class ExportGoodsInfoListHandler: IRequestHandler<ExportGoodsInfoList, ResponseResult>
    {

        OrgUnitOfWork _orgUnitOfWork;
        public ExportGoodsInfoListHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public async Task<ResponseResult> Handle(ExportGoodsInfoList request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            //var dy = new DynamicParameters()
            //.Set("CourseId", request.CourseId);

            //string where = "";
            //if (!request.IgnoreStatus)//只查询上架
            //{
            //    where += "  and c.status=@status  ";
            //    dy.Add("@status", CourseStatusEnum.Ok);
            //}

            #region 课程详情查询
            string sql = "exec ExportSKUDetail";
            var dBData = _orgUnitOfWork.DbConnection.Query(sql);

            #endregion
            return ResponseResult.Success(dBData);
        }
    }
}
