using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels.Evaluations;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 评测收藏或取消
    /// </summary>
    public class EvaluationCollectionCommandHandler : IRequestHandler<EvaluationCollectionCommand, ResponseResult>
    {
        IMediator mediator;
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public EvaluationCollectionCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            this.mediator = mediator;
        }
        public async Task<ResponseResult> Handle(EvaluationCollectionCommand request, CancellationToken cancellationToken)
        {
            var evaluation = await mediator.Send(new EvaluationSimpleQuery { Id = request.EvaluationId });
            if (evaluation.Status != (byte)EvaluationStatusEnum.Ok)
                throw new CustomResponseException("当前状态不能收藏！");

            try
            {
                _orgUnitOfWork.BeginTransaction();
                var dy = new DynamicParameters()
                .Set("id", Guid.NewGuid())
                .Set("dataID", request.EvaluationId)
                .Set("userID", request.UserId)
                .Set("dataType", CollectionEnum.Evaluation)
                .Set("CreateTime", DateTime.Now)
                .Set("Creator", request.UserId)
                .Set("ModifyDateTime", DateTime.Now)
                .Set("Modifier", request.UserId)
                .Set("IsValid", true);
                //1、课程表
                var clcountsql = $"select count(1) from [dbo].[Collection] where dataID=@dataID and userID=@userID and dataType={CollectionEnum.Evaluation.ToInt()} and IsValid=1;";
                var clcount = _orgUnitOfWork.DbConnection.Query<int>(clcountsql, dy, _orgUnitOfWork.DbTransaction).FirstOrDefault();
                var isAdd = clcount == 0 ? true : false;
                dy.Add("@clcount", clcount == 0 ? 1 : -1);



                string cSql = $@"  update [dbo].[Evaluation] set [collectioncount]+=@clcount where id=@dataID  and IsValid=1 ;";

                //2、收藏表(匹配则代表已经收藏--改为未收藏；不匹配表示没收藏，则收藏)
                string updOrInsertSql = $@" merge into [dbo].[Collection] s
                                        using(select @id as id, @dataID as dataID,@userID as userID,@dataType as dataType,@CreateTime as CreateTime,
                                        @Creator as Creator,@ModifyDateTime as ModifyDateTime,@Modifier as Modifier,@IsValid as IsValid) c
                                        on c.userID=s.userID and c.dataID=s.dataID and c.dataType=s.dataType and c.IsValid=s.IsValid and s.IsValid=1
                                        when not matched then
                                        insert ([id], [dataID], [userID], [dataType], [CreateTime], [Creator], [ModifyDateTime], [Modifier], [IsValid])
                                        values(@id, @dataID, @userID, @dataType, @CreateTime, @Creator, @ModifyDateTime, @Modifier, @IsValid)
                                        when matched then 
                                        update set s.IsValid=0
                                        ;";

                var retCount = _orgUnitOfWork.Execute(cSql + updOrInsertSql, dy, _orgUnitOfWork.DbTransaction);

                _orgUnitOfWork.CommitChanges();
                if (retCount >= 2)
                {

                    _redisClient.Del(string.Format(CacheKeys.Evlt, request.EvaluationId));
                  
                    return ResponseResult.Success(true, isAdd ? "收藏成功" : "取消收藏");
                }

                return ResponseResult.Failed("操作失败");
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return ResponseResult.Failed("操作失败");
            }

        }
    }
}
