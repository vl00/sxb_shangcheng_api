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
using iSchool.Organization.Domain.Enum;
using System.Linq;
namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 专题上下架
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
        public Task<ResponseResult> Handle(UpdateSpecialStatusCommand request, CancellationToken cancellationToken)
        {
            if (request.Status == SpecialStatusEnum.Fail.ToInt())//下架专题，则需判断是否绑定活动，是则不允许下架
            {
                var  isBinged = _orgUnitOfWork.DbConnection.Query<int>($" SELECT count(1) FROM [Organization].[dbo].[ActivityExtend] where type={ActivityExtendType.Special.ToInt()} and contentid='{request.Id}' ").FirstOrDefault()>0?true:false;
                if(isBinged)
                    return Task.FromResult(ResponseResult.Failed("该专题已绑定活动，不允许下架"));
            }
            string updateSql = $" UPDATE  [dbo].[Special] set status=@status,ModifyDateTime=@time,Modifier=@userId  where id=@id and IsValid=1;";
            var count = _orgUnitOfWork.DbConnection.Execute(updateSql,new DynamicParameters()
                .Set("status", request.Status)
                .Set("id",request.Id)
                .Set("time",DateTime.Now)
                .Set("userId",request.UserId));
            if (count >= 1)
            {
                #region 清除API那边相关的缓存
                _redisClient.BatchDelAsync( new List<string>()
                    {
                         "org:spcl:*"
                        ,"org:special:simple"
                        ,"org:evlt:info:*"//评测详情有机构信息、课程信息、专题
                       
                    },10);
                #endregion
                return Task.FromResult(ResponseResult.Success("操作成功"));
            }
            else
            {
                return Task.FromResult(ResponseResult.Failed("操作失败"));
            }
        }
    }
}
