using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 活动上下架
    /// </summary>
    public class UpdateActivityStatusCommandHandler:IRequestHandler<UpdateActivityStatusCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public UpdateActivityStatusCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public Task<ResponseResult> Handle(UpdateActivityStatusCommand request, CancellationToken cancellationToken)
        {
            string updateExtend = $" update [dbo].[ActivityExtend] set IsValid=@ExtendIsValid where type={ActivityExtendType.Special.ToInt()} and activityid='{request.Id}'; ;";
            var dp = new DynamicParameters()                
                .Set("id", request.Id)
                .Set("time", DateTime.Now)
                .Set("userId", request.UserId);
            if (request.Status == ActivityStatus.Ok.ToInt())//上架，
            {
                #region 旧 则需要判断该活动的关联专题是否已经被其他上架中的活动占用了，如果占用则去编辑界面
                //                var specialsTitles = _orgUnitOfWork.DbConnection.Query<string>($@"
                //select spe.title from [dbo].[ActivityExtend] as actE
                //left join [dbo].[Special] as spe  on actE.contentid=spe.id and spe.IsValid=1
                //where actE.activityid='{request.Id}' and  actE.[type]={ActivityExtendType.Special.ToInt()} and spe.status={SpecialStatusEnum.Ok.ToInt()} 
                //and spe.id in
                //(
                //select spe.id from [dbo].[ActivityExtend] as actE
                //left join [dbo].[Special] as spe  on actE.contentid=spe.id and spe.IsValid=1
                //left join [dbo].[Activity] as act on act.id=actE.activityid and act.IsValid=1 
                //where actE.activityid<>'{request.Id}' and  actE.[type]={ActivityExtendType.Special.ToInt()} and spe.status={SpecialStatusEnum.Ok.ToInt()} 
                //and act.status={ActivityStatus.Ok.ToInt()} and act.type={ActivityType.Hd2.ToInt()}
                //)
                //order by spe.sort;
                //");
                //                if(specialsTitles.Any()==true)//已被其他红包活动关联的该活动的专题
                //                {
                //                    return Task.FromResult(ResponseResult.Failed($"该活动关联的专题【{string.Join(',', specialsTitles)}】已被其他活动关联，请重新编辑再上架。"));
                //                } 
                #endregion

                dp.Set("ExtendIsValid", true)
                  .Set("status", ActivityStatus.Ok.ToInt());
            }
            else if (request.Status == ActivityStatus.Fail.ToInt())//下架
            {
                dp.Set("ExtendIsValid", false)
                  .Set("status", ActivityStatus.Fail.ToInt());
            }
            string updateSql = $" UPDATE  [dbo].[Activity] set status=@status,ModifyDateTime=@time,Modifier=@userId  where id=@id and IsValid=1;";
            try
            {
                _orgUnitOfWork.BeginTransaction();

                _orgUnitOfWork.DbConnection.Execute(updateSql+ updateExtend, dp,_orgUnitOfWork.DbTransaction);

                _orgUnitOfWork.CommitChanges();
                #region 清除API那边相关的缓存
                _redisClient.BatchDelAsync(new List<string>()
                    {
                         CacheKeys.Acd_id.FormatWith(request.Id)
                        ,CacheKeys.ActivitySimpleInfo.FormatWith(request.Id)
                        ,CacheKeys.Hd_spcl_acti.FormatWith("*")
                        ,"org:special:simple*"//专题列表
                    }, 10);
                #endregion
                return Task.FromResult(ResponseResult.Success("操作成功"));
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return Task.FromResult(ResponseResult.Failed($"操作失败：{ex.Message}"));
            }
          
        }
    }
}
