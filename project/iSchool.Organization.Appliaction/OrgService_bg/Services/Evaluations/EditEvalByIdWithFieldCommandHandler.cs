using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg.Evaluations;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
   
    /// <summary>
    /// 评测通用编辑方法
    /// </summary>
    public class EditEvalByIdWithFieldCommandHandler : IRequestHandler<EditEvalByIdWithFieldCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;

        public EditEvalByIdWithFieldCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient,IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
        }
        public Task<ResponseResult> Handle(EditEvalByIdWithFieldCommand request, CancellationToken cancellationToken)
        {
            request.Parameters.Add("@Id", request.Id);
            if (!string.IsNullOrEmpty(request.UpdateSql))
            {
                string updateSql = "";
                if (!string.IsNullOrEmpty(request.TableName))
                {
                    updateSql = $@" update [dbo].{request.TableName} set {string.Join(',', request.UpdateSql)} where id=@Id;";
                }
                else
                {
                    updateSql = $@" update [dbo].[Evaluation] set {string.Join(',', request.UpdateSql)} where id=@Id;";
                }              
                var count = _orgUnitOfWork.DbConnection.Execute(updateSql, request.Parameters);
                if (count == 1)
                {
                    _mediator.Send(new SingleFieldClearEvltCachesCommand() { Id = request.Id });
                    //CommonHelper.CSRedisClientHelper.BatchDel(_redisClient, new List<string>() 
                    //{ 
                    //    CacheKeys.Evlt.FormatWith(request.Id), 
                    //    "org:evltsMain:*", 
                    //    "org:spcl:*" 
                    //});
                    return Task.FromResult(ResponseResult.Success("操作成功"));
                }
                else
                {
                    return Task.FromResult(ResponseResult.Failed("操作失败"));
                }
            }
            else
            {
                return Task.FromResult(ResponseResult.Failed("操作失败"));
            }
        }
    }

}
