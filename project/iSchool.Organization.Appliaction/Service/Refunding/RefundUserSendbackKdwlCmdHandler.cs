using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class RefundUserSendbackKdwlCmdHandler : IRequestHandler<RefundUserSendbackKdwlCmd, object>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        IUserInfo me;
        NLog.ILogger log;
        ILock1Factory _lock1Factory;

        public RefundUserSendbackKdwlCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IUserInfo me, NLog.ILogger log, ILock1Factory _lock1Factory,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this.me = me;
            this.log = log;
            this._lock1Factory = _lock1Factory;
        }

        public async Task<object> Handle(RefundUserSendbackKdwlCmd cmd, CancellationToken cancellation)
        {
            await using var _lck = await _lock1Factory.LockAsync(CacheKeys.Refund_SendbackKdLck.FormatWith(me.UserId), 1 * 60 * 1000);
            if (!_lck.IsAvailable) throw new CustomResponseException("系统繁忙", Consts.Err.RefundApplyCheck_CannotGetLck);

            var orderRefund = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<OrderRefunds>($@"
                select * from OrderRefunds where IsValid=1 and id=@Id
            ", new { cmd.Id });

            if (orderRefund == null)
            {
                throw new CustomResponseException("无效的退款单");
            }
            if (me.UserId != orderRefund.RefundUserId)
            {
                throw new CustomResponseException("非法操作", Consts.Err.RefundCancel_NotSameUser);
            }
            if (((RefundTypeEnum)orderRefund.Type) != RefundTypeEnum.Return
                || ((RefundStatusEnum)orderRefund.Status) != RefundStatusEnum.SendBack)
            {
                throw new CustomResponseException("当前退款单状态不能填写寄回物流信息");
            }
            if (orderRefund.StepOneTime == null)
            {
                throw new CustomResponseException("当前退款单状态不能填写寄回物流信息", Consts.Err.RefundSendback_NoStepOneTime);
            }
            if (DateTime.Now - orderRefund.StepOneTime.Value >= TimeSpan.FromDays(7))
            {
                try { await _mediator.Send(new RefundCancelCmd { Id = orderRefund.Id, IsFromExpired = true }); }
                catch { }

                throw new CustomResponseException("退货填写物流已超时申请已取消", Consts.Err.RefundSendback_Timeout);
            }

            var rr = (await _mediator.Send(KuaidiServiceArgs.CheckNu(cmd.Nu, cmd.Com))).GetResult<KdCompanyCodeDto>();
            if (rr == null)
            {
                throw new CustomResponseException("物流信息有误, 请检查");
            }

            var i = await _orgUnitOfWork.ExecuteAsync($@"
                update OrderRefunds set SendBackExpressCode=@Nu,SendBackExpressType=@Com,SendBackTime=getdate(),[Status]={RefundStatusEnum.Receiving.ToInt()},
                    [ModifyDateTime]=getdate(),Modifier=@UserId 
                where id=@Id and IsValid=1 and [Status]={RefundStatusEnum.SendBack.ToInt()}
            ", new { cmd.Id, cmd.Nu, cmd.Com, me.UserId });

            if (i < 1)
            {
                throw new CustomResponseException("当前退款单状态不能填写寄回物流信息", Consts.Err.RefundSendback_WriteKd2DbError);
            }

            return null;
        }

    }
}
