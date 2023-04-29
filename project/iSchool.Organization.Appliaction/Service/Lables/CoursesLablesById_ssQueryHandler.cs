﻿using CSRedis;
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
    /// 【课程短Id集查询】课程标签列表业务
    /// </summary>
    public class CoursesLablesById_ssQueryHandler:IRequestHandler<CoursesLablesById_ssQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        public CoursesLablesById_ssQueryHandler (IOrgUnitOfWork unitOfWork)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
        }
        public async Task<ResponseResult> Handle(CoursesLablesById_ssQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            
            #region Where

            string sqlWhere = $@" where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and org.status={OrganizationStatusEnum.Ok.ToInt()}   "; // and c.type={CourseTypeEnum.Course.ToInt()}

            //课程短Id集合
            if (request.Id_ss.Any()==false)
            {
                return ResponseResult.Failed("Id不允许为空集！");
            }
            else
            {
                var nos= request.Id_ss.Select(_ => UrlShortIdUtil.Base322Long(_)).ToList();
                sqlWhere += $" and c.no in ({string.Join(",",nos)})  ";
            }
            #endregion

            string sql = $@" 
                        select c.* from Course c left join Organization org on c.orgid=org.id and org.IsValid=1                                                       
                            {sqlWhere}  ;";
           
            var courses = _unitOfWork.Query<Domain.Course>(sql, null)?.ToList();
            if (courses.Any() == false)
            {
                return ResponseResult.Success("无符合条件的数据");
            }
            var data = courses.Select(_ => new CourseLable() { Id=_.Id,Id_s= UrlShortIdUtil.Long2Base32(Convert.ToInt64(_.No)), Price=_.Price, Title=_.Title
                ,CoverUrl=(string.IsNullOrEmpty(_.Banner)?null:JsonSerializationHelper.JSONToObject<List<string>>(_.Banner)?.FirstOrDefault() )}).ToList();
                        
             return ResponseResult.Success(data);
        }
    }
}
