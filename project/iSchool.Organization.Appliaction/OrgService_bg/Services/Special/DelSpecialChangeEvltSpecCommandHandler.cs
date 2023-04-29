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
    /// 删除专题并把专题下评测归为其他专题
    /// </summary>
    public class DelSpecialChangeEvltSpecCommandHandler : IRequestHandler<DelSpecialChangeEvltSpecCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public DelSpecialChangeEvltSpecCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public Task<ResponseResult> Handle(DelSpecialChangeEvltSpecCommand request, CancellationToken cancellationToken)
        {
            var isBinged = _orgUnitOfWork.DbConnection.Query<int>($" SELECT count(1) FROM [Organization].[dbo].[ActivityExtend] where type={ActivityExtendType.Special.ToInt()} and contentid='{request.DelId}' ").FirstOrDefault() > 0 ? true : false;
            if (isBinged)//专题已绑定活动
                return Task.FromResult(ResponseResult.Failed("该专题已绑定活动，不允许删除"));


            string upSpeEvltSql = "";
            if (request.SpecialType == SpecialTypeEnum.SmallSpecial.ToInt())//小专题则重新绑定专题与评测的关系
            {
                upSpeEvltSql = $" update [dbo].[SpecialBind] set specialid=@newId where specialid=@delId and IsValid=1; ";
            }
            else if(request.SpecialType == SpecialTypeEnum.BigSpecial.ToInt())//大专题，则直接删除或者重新绑定大小专题的关系
            {
                if (request.NewId == default)//直接删除
                {
                    upSpeEvltSql = $" update SpecialSeries set IsValid=0 where IsValid=1 and  bigspecial=@delId  ";
                }
                else//重新绑定
                {
                    upSpeEvltSql = $" update SpecialSeries set bigspecial=@newId where IsValid=1 and  bigspecial=@delId  ";
                }
            }
            
            string delSpeSql = $" update [dbo].[Special] set IsValid=0,ModifyDateTime=@time,Modifier=@userId where id=@delId;     ";
            var dy = new DynamicParameters().Set("newId", request.NewId)
                .Set("delId", request.DelId)
                .Set("time",DateTime.Now)
                .Set("userId",request.UserId);
            try
            {
                _orgUnitOfWork.BeginTransaction();
                _orgUnitOfWork.DbConnection.Execute(upSpeEvltSql, dy,_orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.DbConnection.Execute(delSpeSql, dy, _orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.CommitChanges();

                #region 清除API那边相关的缓存--待删除专题，和待删除专题的评测归为A专题。两个专题相关缓存都需删除
                _redisClient.BatchDelAsync( new List<string>()
                    {
                         "org:spcl:*"//专题                          
                        ,"org:special:simple"
                        ,"org:evlt:info:*"//评测详情有机构信息、课程信息、专题
                    },10);
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
