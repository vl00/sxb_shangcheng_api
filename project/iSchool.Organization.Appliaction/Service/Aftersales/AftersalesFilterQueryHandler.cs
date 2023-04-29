using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Aftersales;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class AftersalesFilterQueryHandler : IRequestHandler<AftersalesFilterQuery, AftersalesCollection>
    {
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;
        public AftersalesFilterQueryHandler(IOrgUnitOfWork orgUnitOfWork
            , IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _mediator = mediator;
        }

        public async Task<AftersalesCollection> Handle(AftersalesFilterQuery request, CancellationToken cancellationToken)
        {
            string sql = @"
SELECT * INTO #USERINFOTEMP FROM iSchoolUser.dbo.userInfo WHERE EXISTS(SELECT 1  FROM ORDERREFUNDS WHERE OrderRefunds.IsValid = 1  AND userInfo.id = OrderRefunds.RefundUserId);
SELECT 
OrderRefunds.Id,
OrderRefunds.Code, --退款编号
OrderDetial.productid SKUId,
OrderDetial.ctn SKU_Json ,--退货商品
OrderRefunds.[Count], --退货数量
OrderRefunds.[Type], --售后类型
(ISNULL(OrderDetial.price,0) * OrderRefunds.Count) PayAmount, --实付金额
OrderRefunds.Price ApplyRefund, -- 申请退货金额
OrderRefunds.RefundPrice , -- 实退金额
OrderRefunds.Reason, -- 退款描述
OrderRefunds.Voucher, -- 退款凭证
OrderRefunds.SpecialReason, -- 特殊原因
OrderRefunds.Remark, -- 特殊原因备注
OrderRefunds.SendBackExpressType,
OrderRefunds.SendBackExpressCode,
OrderRefunds.SendBackAddress,
OrderRefunds.SendBackMobile,
OrderRefunds.SendBackUserName,
OrderRefunds.SendBackTime,
OrderRefunds.CreateTime, --申请时间
OrderRefunds.Cause, --退货理由
userInfo.nickname, --退货人昵称
userInfo.mobile, -- 退货人手机号
[Order].code OrderNumber, -- 订单号
OrderRefunds.[Status], -- 综合状态
OrderRefunds.[PreStatus], -- 流转前状态
OrderRefunds.StepOneAuditor,
OrderRefunds.StepOneAuditRecord,
OrderRefunds.StepOneTime,
OrderRefunds.StepTwoAuditor,
OrderRefunds.StepTwoAuditRecord,
OrderRefunds.StepTwoTime,
OrderRefunds.RefundTime,
[Order].CreateTime OrderCreateTime,
[Order].[status] OrderState
FROM ORDERREFUNDS
LEFT JOIN OrderDetial ON OrderDetial.id = ORDERREFUNDS.OrderDetailId
LEFT JOIN #USERINFOTEMP userInfo ON userInfo.id = OrderRefunds.RefundUserId
LEFT JOIN [Order] ON [Order].id = OrderRefunds.OrderId
WHERE 
OrderRefunds.IsValid = 1 
And ({0})
ORDER BY CreateTime DESC
OFFSET @offset ROW
FETCH NEXT @limit ROWS ONLY;
--
SELECT 
Count(1)
FROM ORDERREFUNDS
LEFT JOIN OrderDetial ON OrderDetial.id = ORDERREFUNDS.OrderDetailId
LEFT JOIN #USERINFOTEMP userInfo ON userInfo.id = OrderRefunds.RefundUserId
LEFT JOIN [Order] ON [Order].id = OrderRefunds.OrderId
WHERE 
OrderRefunds.IsValid = 1 
And ({0});
";
            List<string> filters = new List<string>();
            DynamicParameters parameters = new DynamicParameters();
            int offsest = (request.Page - 1) * request.PageSize;
            parameters.Add("offset", offsest); parameters.Add("limit", request.PageSize);
            if (request.RefundReason != null)
            {
                filters.Add("(OrderRefunds.Cause = @Cause)");
                parameters.Add("Cause", request.RefundReason);
            }
            if (request.OrderState != null)
            {
                filters.Add("([Order].[status] = @OrderState)");
                parameters.Add("OrderState", request.OrderState);
            }
            if (request.AftersalesState != null)
            {
                filters.Add("(OrderRefunds.Status IN @AftersalesGoodsState)");
                parameters.Add("AftersalesGoodsState", AftersalesState2Status(request.AftersalesState.Value));
            }
            if (request.AuditState != null)
            {
                filters.Add("(OrderRefunds.Status IN @AuditState)");
                parameters.Add("AuditState", AuditState2Status(request.AuditState.Value));
            }
            if (request.Type != null)
            {
                filters.Add("(OrderRefunds.Type = @Type)");
                parameters.Add("Type", request.Type.Value);
            }
            if (!string.IsNullOrEmpty(request.OrderNumber))
            {
                filters.Add("([Order].code = @OrderNumber)");
                parameters.Add("OrderNumber", request.OrderNumber);
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                filters.Add("(userInfo.mobile = @mobile)");
                parameters.Add("mobile", request.PhoneNumber);
            }
            if (!string.IsNullOrEmpty(request.UserNickName))
            {
                filters.Add("(userInfo.nickname like  @nickname)");
                parameters.Add("nickname",$"%{ request.UserNickName}%");
            }
            if (!string.IsNullOrEmpty(request.GoodsOrBrandName))
            {
                filters.Add("((JSON_Value(OrderDetial.ctn, '$.title') like @GoodsOrBrandName or JSON_Value(OrderDetial.ctn, '$.orgName') like @GoodsOrBrandName))");
                parameters.Add("GoodsOrBrandName", $"%{request.GoodsOrBrandName}%");
            }
            if (request.SDateTime != null)
            {
                filters.Add("(OrderRefunds.CreateTime >=  @SDateTime)");
                parameters.Add("SDateTime", request.SDateTime);
            }
            if (request.EDateTime != null)
            {
                filters.Add("(OrderRefunds.CreateTime <  @EDateTime)");
                parameters.Add("EDateTime", request.EDateTime);
            }
            if (filters.Count == 0)
            {
                filters.Add("(1 = 1)");
            }
            using (var grid = await _orgUnitOfWork.QueryMultipleAsync(string.Format(sql, string.Join(" And ", filters)), parameters))
            {
                var result = await grid.ReadAsync();
                AftersalesCollection aftersalesCollection = MapAftersalesCollection(result);
                aftersalesCollection.Total = await grid.ReadFirstAsync<int>();
                return aftersalesCollection;
            }
        }

        AftersalesCollection MapAftersalesCollection(IEnumerable<dynamic> result)
        {
            AftersalesCollection aftersalesCollection = new AftersalesCollection();
            var auditorInfos = GetAuditorInfos(result);

            aftersalesCollection.Datas = result.Select(r =>
            {
                try {
                    var aftersales = new ViewModels.Aftersales.Aftersales()
                    {
                        Id = r.Id,
                        ApplyDateTime = r.CreateTime,
                        ApplyRefundAmount = r.ApplyRefund,
                        PayAmount = r.PayAmount,
                        ExpressInfo = GetExpressInfo(r.SendBackExpressType, r.SendBackExpressCode),
                        Type = (AftersalesType)r.Type,
                        Number = r.Code,
                        OrderNumber = r.OrderNumber,
                        Reason = (RefundReason?)r.Cause,
                        ReasonDesc = r.Reason,
                        RefundAmount = r.RefundPrice,
                        RefundTime = r.RefundTime,
                        RefundUserNickName = r.nickname,
                        RefundUserPhoneNumber = r.mobile,
                        ReturnCount = r.Count,
                        SendBackAddress = r.SendBackAddress,
                        SendBackMobile = r.SendBackMobile,
                        SendBackUserName = r.SendBackUserName,
                        SendBackTime = r.SendBackTime,
                        State = Status2AftersalesState(r.Status),
                        Vouchers = r.Voucher == null ? null : Newtonsoft.Json.JsonConvert.DeserializeObject(r.Voucher),
                        SKU = SKU_JSon2SKU(r.SKUId, r.SKU_Json),
                        OrderInfo = new OrderInfo { State = r.OrderState, CreateTime = r.OrderCreateTime },
                        SpecialReason = (Domain.Enum.OrderRefundSpecialReason)r.SpecialReason,
                        Remark = r.Remark

                    };



                    if (r.StepTwoAuditor != null)
                    {
                        aftersales.SecondAuditResult = new AuditResult()
                        {
                            AuditorId = r.StepTwoAuditor,
                            AuditorName = auditorInfos.FirstOrDefault(a => a.Key == r.StepTwoAuditor).Value,
                            AuditDateTime = r.StepTwoTime,
                            Remark = r.StepTwoAuditRecord,
                            State = Status2AuditState(r.Status)
                        };
                        aftersales.FirstAuditResult = new AuditResult()
                        {
                            AuditorId = r.StepOneAuditor,
                            AuditorName = auditorInfos.FirstOrDefault(a => a.Key == r.StepOneAuditor).Value,
                            AuditDateTime = r.StepOneTime,
                            Remark = r.StepOneAuditRecord,
                            State =  AuditState.AduitSuccess //有第二次审核，第一次必然是审核成功的。
                        };
                    }
                    else if (r.StepOneAuditor != null)
                    {
                        aftersales.FirstAuditResult = new AuditResult()
                        {
                            AuditorId = r.StepOneAuditor,
                            AuditorName = auditorInfos.FirstOrDefault(a => a.Key == r.StepOneAuditor).Value,
                            AuditDateTime = r.StepOneTime,
                            Remark = r.StepOneAuditRecord,
                            State = Status2AuditState(r.Status)//有第二次审核，第一次必然是审核成功的。
                        };
                    }

                    return aftersales;
                }
                catch(Exception ex) {
                    throw new Exception( Newtonsoft.Json.JsonConvert.SerializeObject(r), ex);
                }

            });
            return aftersalesCollection;
        }


        ExpressInfo GetExpressInfo(string type, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            else
            {
                var expressInfo = new ExpressInfo()
                {
                    Code = code,
                };
                if (!string.IsNullOrEmpty(type))
                {
                    var company = _mediator.Send(KuaidiServiceArgs.GetCode(type)).Result.GetResult<KdCompanyCodeDto>();
                    expressInfo.Name  = company.Com;
                    expressInfo.Type = type;
                }
                return expressInfo;
            }

        }

        SKU SKU_JSon2SKU(Guid? skuId, string sku_json)
        {
            if (skuId == null) return null;
            if (!string.IsNullOrEmpty(sku_json))
            {
                var jobj = JObject.Parse(sku_json);
                SKU sku = new SKU()
                {
                    Id = skuId.Value,
                    GoodsId = Guid.Parse(jobj["id"].Value<string>()),
                    GoodsName = jobj["title"].Value<string>(),
                    PropId = Guid.Parse(jobj["propItemIds"].ToArray().FirstOrDefault().Value<string>()),
                    PropName = jobj["propItemNames"].ToArray().FirstOrDefault().Value<string>()
                };


                return sku;
            }
            else
            {

                return null;
            }

        }

        Dictionary<Guid, string> GetAuditorInfos(IEnumerable<dynamic> result)
        {
            var auditorIds = result.SelectMany(r =>
            {
                var ls = new List<Guid>();
                if (r.StepOneAuditor != null)
                {
                    ls.Add(r.StepOneAuditor);
                }
                if (r.StepTwoAuditor != null)
                {
                    ls.Add(r.StepTwoAuditor);
                }
                return ls;
            });
            return AdminInfoUtil.GetNames(auditorIds);
        }

        /// <summary>
        /// 将售后商品状态转为综合状态。
        /// </summary>
        /// <param name="aftersalesState"></param>
        /// <returns></returns>
        List<int> AftersalesState2Status(AftersalesState aftersalesState)
        {
            //退款 / 换货状态
            //1.提交申请  2.平台审核(发货) 3.平台审核(未发货)   4.平台退款  5.退款成功  6.审核失败
            //11.提交申请   12.平台审核   13.审核失败   14.寄回商品  15平台收货  16.验货失败   17.退款成功
            //20.(用户主动)取消申请 21.因过期而取消申请
            List<int> status = new List<int>();
            switch (aftersalesState)
            {
                case AftersalesState.WaitAudit:
                    status.Add(2);
                    status.Add(3);
                    status.Add(12);
                    break;
                case AftersalesState.WaitReback:
                    status.Add(14);
                    break;
                case AftersalesState.HasReback:
                    status.Add(15);
                    break;
                case AftersalesState.ApplyRefus:
                    status.Add(6);
                    status.Add(13);
                    status.Add(16);
                    break;
                case AftersalesState.ApplyCancel:
                    status.Add(20);
                    status.Add(21);
                    break;
                case AftersalesState.HasRefund:
                    status.Add(5);
                    status.Add(17);
                    break;
            }
            return status;
        }
        /// <summary>
        /// 综合状态转售后商品状态
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        AftersalesState Status2AftersalesState(int? status)
        {
            //退款 / 换货状态
            //1.提交申请  2.平台审核(已发货) 3.平台审核(未发货)   4.平台退款  5.退款成功  6.审核失败
            //11.提交申请   12.平台审核   13.审核失败   14.寄回商品(第一次审核通过)  15平台收货  16.验货失败   17.退款成功
            if (status == 2 || status == 3 || status == 12)
            {
                return AftersalesState.WaitAudit;
            }
            if (status == 14)
            {
                return AftersalesState.WaitReback;
            }
            if (status == 15)
            {
                return AftersalesState.HasReback;
            }
            if (status == 6 || status == 13 || status  == 16)
            {
                return AftersalesState.ApplyRefus;
            }
            if (status == 20 || status == 21 )
            {
                return AftersalesState.ApplyCancel;
            }
            if (status == 5 || status == 17)
            {
                return AftersalesState.HasRefund;
            }
            return AftersalesState.WaitAudit;
        }

        /// <summary>
        /// 将审核状态转为对应的综合状态。
        /// </summary>
        /// <param name="auditState"></param>
        /// <returns></returns>
        List<int> AuditState2Status(AuditState auditState)
        {
            //退款 / 换货状态
            //1.提交申请  2.平台审核(发货) 3.平台审核(未发货)   4.平台退款  5.退款成功  6.审核失败
            //11.提交申请   12.平台审核   13.审核失败   14.寄回商品  15平台收货  16.验货失败   17.退款成功
            List<int> status = new List<int>();
            switch (auditState)
            {
                case AuditState.UnAudit:
                    status.Add(2);
                    status.Add(3);
                    status.Add(12);
                    status.Add(1);
                    status.Add(11);
                    break;
                case AuditState.AduitSuccess:
                    status.Add(4);
                    status.Add(5);
                    status.Add(17);
                    break;
                case AuditState.AduitFail:
                    status.Add(6);
                    status.Add(13);
                    status.Add(16);
                    break;
            }
            return status;
        }

        /// <summary>
        /// 将综合状态转为审核状态
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        AuditState Status2AuditState(int status)
        {
            //退款 / 换货状态
            //1.提交申请  2.平台审核(发货) 3.平台审核(未发货)   4.平台退款  5.退款成功  6.审核失败
            //11.提交申请   12.平台审核   13.审核失败   14.寄回商品  15平台收货  16.验货失败   17.退款成功
            if (status == 4  || status == 5  ||status == 14 || status == 15 || status == 17)
            {
                return AuditState.AduitSuccess;
            }
            if (status == 13 || status == 16 || status == 6)
            {
                return AuditState.AduitFail;
            }
            return AuditState.UnAudit;

        }

    }
}
