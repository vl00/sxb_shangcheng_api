using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class RefundStatusInSendbackDoAutoExpiredCancelCmdHandler : IRequestHandler<RefundStatusInSendbackDoAutoExpiredCancelCmd, Guid[]>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;                
        IConfiguration _config;
        ILock1Factory _lck1fay;

        public RefundStatusInSendbackDoAutoExpiredCancelCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            ILock1Factory lck1fay,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;            
            this._config = config;
            this._lck1fay = lck1fay;
        }

        public async Task<Guid[]> Handle(RefundStatusInSendbackDoAutoExpiredCancelCmd cmd, CancellationToken cancellation)
        {
            // 退货退款第一次审核通过，填写物流时间仅剩1天时
            // 发wx通知提醒用户

            var sql = $@"
select top 100 f.* from [OrderRefunds] f 
where f.IsValid=1 and f.type=@ty and f.status=@stt
and datediff(hour,f.StepOneTime,getdate())=24*{cmd.Days - 1}
and not exists(select 1 from T_RefundSendbackWLExpired1dayBeforeWxNotifyLog where id=f.id)
order by f.StepOneTime,f.id
";
            var orderRefunds = (await _orgUnitOfWork.QueryAsync<OrderRefunds>(sql, param: new 
            {
                ty = RefundTypeEnum.Return.ToInt(),
                stt = RefundStatusEnum.SendBack.ToInt(),
            })).AsList();
            if (orderRefunds.Count > 0)
            {
                var ls = new List<Guid>();
                foreach (var rfd in orderRefunds)
                {
                    if (rfd.Creator == default) continue;
                    try
                    {
                        sql = $@"select json_value(d.ctn,'$.title') from [OrderDetial] d where d.id=@OrderDetailId";
                        var title = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<string>(sql, new { rfd.OrderDetailId });
                        if (title == null) continue;
                        ls.Add(rfd.Id);
                        await _mediator.Send(new SendWxTemplateMsgCmd
                        {
                            UserId = rfd.Creator!.Value,
                            WechatTemplateSendCmd = new WechatTemplateSendCmd
                            {
                                KeyWord1 = $"您发起的《{title}》{rfd.Count}件商品退货退款申请仅剩1天填写退货物流信息！若未按时退还，系统将会自动撤销您的售后申请，请及时填写！",
                                KeyWord2 = DateTime.Now.ToDateTimeString(),
                                Remark = "点击下方【详情】立即填写物流信息",
                                MsyType = WechatMessageType.填写退货物流信息即将超时,
                                Args = new Dictionary<string, object>
                                {
                                    ["id"] = rfd.Id.ToString(),
                                }
                            }
                        });
                    }
                    catch { }
                }
                if (ls.Count > 0)
                {
                    try
                    {
                        sql = "insert T_RefundSendbackWLExpired1dayBeforeWxNotifyLog(id,addtime) select @id,getdate()";
                        await _orgUnitOfWork.ExecuteAsync(sql, new { id = ls });
                    }
                    catch { }
                }
            }
            {
                // delete too old
                try
                {
                    sql = "delete from T_RefundSendbackWLExpired1dayBeforeWxNotifyLog where datediff(day,addtime,getdate())>3";
                    await _orgUnitOfWork.ExecuteAsync(sql);
                }
                catch { }
            }

            //
            // 退货退款第一次审核通过，填写物流时间 已超时自动取消申请

            sql = $@"
select top 100 f.* 
from [OrderRefunds] f where f.IsValid=1 and f.type=@ty and f.status=@stt
and datediff(hour,f.StepOneTime,getdate())>=24*{cmd.Days}
order by f.StepOneTime
";
            orderRefunds = (await _orgUnitOfWork.QueryAsync<OrderRefunds>(sql, param: new
            {
                ty = RefundTypeEnum.Return.ToInt(),
                stt = RefundStatusEnum.SendBack.ToInt(),
            })).AsList();
            if (orderRefunds.Count < 1) return null;

            List<Guid> notOkIds = null;
            foreach (var rfd in orderRefunds)
            {
                try
                {
                    await _mediator.Send(new RefundCancelCmd { Id = rfd.Id, IsFromExpired = true });
                }
                catch (Exception)
                {
                    notOkIds ??= new List<Guid>();
                    notOkIds.Add(rfd.Id);
                }
            }

            return notOkIds?.ToArray();
        }        

    }
}
