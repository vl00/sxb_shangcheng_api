using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg.Evaluations;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    /// <summary>
    /// 评测内容更新科目
    /// </summary>
    public class UpdateSubjectCommandHandler:IRequestHandler<UpdateSubjectCommand,ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;

        public UpdateSubjectCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
        }
        public Task<ResponseResult> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
        {
            if(!Enum.IsDefined(typeof(SubjectEnum), request.Subject))
                return Task.FromResult(ResponseResult.Failed("非法科目！"));

            string updateSql = $@"merge into [dbo].[EvaluationBind] eb
using (select @evltId as evaluationid)v on v.evaluationid = eb.evaluationid and eb.IsValid = 1
when matched then
update set subject = @subject
when not matched then
insert([id],[evaluationid],[subject],[IsValid])values(NEWID(), @evltId, @subject, 1); ";

            try
            {
                _orgUnitOfWork.DbConnection.Execute(updateSql, new DynamicParameters().Set("evltId", request.EvltId).Set("subject", request.Subject));                
                //CommonHelper.CSRedisClientHelper.BatchDel(_redisClient, new List<string>() 
                //{ 
                //    CacheKeys.Evlt.FormatWith(request.EvltId),
                //    "org:evltsMain:*", 
                //    "org:spcl:*" 
                //});
                return Task.FromResult(ResponseResult.Success("操作成功"));
            }
            catch(Exception ex)
            {
                throw new CustomResponseException("系统错误：" + ex.Message);
            }
            
        }
    }

}
