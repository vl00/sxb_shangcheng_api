using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Wechat;
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

namespace iSchool.Organization.Appliaction.Service.Evaluations
{

    /// <summary>
    /// 种草奖励审核不通过
    /// </summary>
    public class EvltRewardUnPassAuditCommandHandler : IRequestHandler<EvltRewardUnPassAuditCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        public EvltRewardUnPassAuditCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
            
        }
        public async Task<ResponseResult> Handle(EvltRewardUnPassAuditCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string updateSql = $@" 
update [dbo].[Evaluation] set auditstatus=@auditstatus,AuditRecord=@AuditRecord,Auditor=@Auditor,Modifier=@Modifier,ModifyDateTime=GETDATE() where IsValid=1 and id=@Id;
";
            var count = await _orgUnitOfWork.DbConnection.ExecuteAsync(updateSql, new DynamicParameters()
                .Set("auditstatus", EvltAuditStatusEnum.Failed.ToInt())
                .Set("Modifier", request.Operator)
                .Set("Auditor", request.Operator)
                .Set("Id", request.Id)
                .Set("AuditRecord", request.AuditRecord));

            if (count == 1)
            {
                var title = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<string>("select [Title] from [dbo].[Evaluation] where id=@Id", new { request.Id });

                //#region 微信通知     
                //try
                //{
                //    await _mediator.Send(new SendWxTemplateMsgCmd
                //    {
                //        UserId = request.UserId,
                //        WechatTemplateSendCmd = new WechatTemplateSendCmd()
                //        {
                //            KeyWord1 = $"您发布的种草文《{title}》未通过审核，不通过原因：{request.AuditRecord}，快去修改吧。修改后请联系审核人员。",
                //            KeyWord2 = DateTime.Now.ToDateTimeString(),
                //            Remark = "点击去修改",
                //            MsyType = WechatMessageType.种草审核不通过,
                //            EvltId = request.Id
                //        }
                //    });
                //}
                //catch { }
                //#endregion
                return ResponseResult.Success("操作成功");
            }
            else
            {
                return ResponseResult.Failed("操作失败");
            }
        }
    }

}
