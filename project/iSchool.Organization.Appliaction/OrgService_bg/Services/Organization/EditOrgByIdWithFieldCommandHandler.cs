using CSRedis;
using Dapper;
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
    public class UpdateSpecialStatusCommandHandler:IRequestHandler<UpdateSpecialStatusCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public UpdateSpecialStatusCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(UpdateSpecialStatusCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            request.Parameters.Add("@Id", request.OrgId);
            if (!string.IsNullOrEmpty(request.UpdateSql))
            {
                string updateSql = $@" update [dbo].[Organization] set {string.Join(',', request.UpdateSql)} where id=@Id;";
                var count = _orgUnitOfWork.DbConnection.Execute(updateSql, request.Parameters);
                if (count == 1)
                {
                    #region 清除API那边相关的缓存
                    await _redisClient.BatchDelAsync( new List<string>()
                    {
                        //机构移动需清除的缓存
                         CacheKeys.Del_Organizations.FormatWith("*")
                        ,CacheKeys.OrgDetails.FormatWith(request.OrgId)
                        ,"org:courses:*"//课程列表有用到机构名称
                        ,"org:course:courseid:*"//课程详情有用到机构名称
                        ,"org:evlt:info:*"//评测详情有机构信息、课程信息、专题信息

                        //机构pc需清除的缓存
                        ,$"org:organization:orgid:{request.OrgId}:pc:*"
                        ,$"org:organization:orgz:{request.OrgId}:info"

                    ,"org:pc:relatedEvlts:*"//pc评测详情-相关评测s、pc课程详情-相关评测s、pc机构详情-相关评测s
                    },10);
                    #endregion
                    return ResponseResult.Success("操作成功");
                }
                else
                {
                    return ResponseResult.Failed("操作失败");
                }
            }
            else
            {
                return ResponseResult.Failed("操作失败");
            }
        }
    }
}
