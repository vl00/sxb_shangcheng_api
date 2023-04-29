using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Evaluations;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 评测上下架
    /// </summary>
    public class EvltSetOffOrOnTheShelfCommandHandler:IRequestHandler<EvltSetOffOrOnTheShelfCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        private readonly IMediator _mediator;

        public EvltSetOffOrOnTheShelfCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient,IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
        }
        public Task<ResponseResult> Handle(EvltSetOffOrOnTheShelfCommand request, CancellationToken cancellationToken)
        {
           
            string updateSql = $" UPDATE  [dbo].[Evaluation] set status=@status,ModifyDateTime=@time,Modifier=@userId  where id=@id and IsValid=1;";
            try
            {
                var count = _orgUnitOfWork.DbConnection.Execute(updateSql, new DynamicParameters()
                .Set("status", request.Status)
                .Set("id", request.Id)
                .Set("time", DateTime.Now)
                .Set("userId", request.UserId));
                if (count == 1)//清除API那边相关的缓存
                {
                    //异步删除
                    _mediator.Send(new SingleFieldClearEvltCachesCommand() { Id = request.Id });
                    return Task.FromResult(ResponseResult.Success("操作成功"));
                }
                else
                {
                    return Task.FromResult(ResponseResult.Failed("操作失败"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(ResponseResult.Failed($"系统错误：【{ex.Message}】"));
            }
            
        }
    }
}
