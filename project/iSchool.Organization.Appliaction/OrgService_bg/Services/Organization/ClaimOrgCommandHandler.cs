 using CSRedis;
using Dapper;
using iSchool.Domain.Enum;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 编辑机构通用方法
    /// </summary>
    public class ClaimOrgCommandHandler : IRequestHandler<ClaimOrgCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public ClaimOrgCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public Task<ResponseResult> Handle(ClaimOrgCommand request, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(ClaimStatusEnum), request.Stats))
            {
                return Task.FromResult(ResponseResult.Failed("非法枚举"));
            }

            var dy = new DynamicParameters()
                .Set("OrgId",request.OrgId)
                .Set("Id",request.Id)
                .Set("Stats", request.Stats)
                .Set("Authentication",request.Stats==(int)ClaimStatusEnum.Claimed?true:false);
            string updata_Org = $"  update [dbo].[Organization] set authentication=@Authentication where id=@OrgId ";
            string update_Authentication = " Update [dbo].[Authentication]  set status=@Stats where id=@Id ";
            try
            {
                _orgUnitOfWork.BeginTransaction();

                _orgUnitOfWork.DbConnection.Execute(updata_Org, dy, _orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.DbConnection.Execute(update_Authentication, dy, _orgUnitOfWork.DbTransaction);

                _orgUnitOfWork.CommitChanges();

                #region 清除API机构列表、相关机构详情的缓存
                _redisClient.BatchDelAsync( new List<string>()
                {
                     CacheKeys.Del_Organizations.FormatWith("*")
                    ,CacheKeys.OrgDetails.FormatWith(request.OrgId)
                   
                    ,"org:course:courseid:*"//课程详情有机构认证信息
                    ,"org:courses:*"//课程列表有机构认证

                    ,"org:evlt:info:*"//评测详情有专题信息、机构信息、课程信息

                    
                    ,$"org:organization:orgid:{request.OrgId}:pc:relatedcourses"//pc机构详情-机构(相关)课程s
                    ,"org:pc:relatedEvlts:*"//pc评测详情-相关评测s、pc课程详情-相关评测s、pc机构详情-相关评测s


                },10) ;
                #endregion

                return Task.FromResult(ResponseResult.Success("操作成功"));
            }
            catch(Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return Task.FromResult(ResponseResult.Failed("操作失败"));
            }
           
        }
    }
}
