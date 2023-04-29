using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{
    /// <summary>
    /// 批量导入更新订单的物流信息
    /// </summary>
    public class ExcelImportWLInfoCommandHandler : IRequestHandler<ExcelImportWLInfoCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;


        public ExcelImportWLInfoCommandHandler(IOrgUnitOfWork orgUnitOfWork, CSRedisClient redisClient, IMediator mediator)
        {
            _orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
        }

        public async Task<ResponseResult> Handle(ExcelImportWLInfoCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            using (var package = new ExcelPackage())
            {
                package.Load(request.Excel);

                var worksheet = package.Workbook.Worksheets.First(); //sheet1

                var ls = new List<Domain.Order>();
                StringBuilder errMsgs = new StringBuilder();
                for (var i = 2; i <= worksheet.Dimension.Rows; i++)
                {
                    //A-订单号、B-课程、C-具体购买、D-价格、E-收货人昵称、F-收货人电话、G-省、H-市、I-区、J-详细地址、K-快递单号、L-快递公司-编号

                    var code = worksheet.Cells[$"A{i}"].Value?.ToString()?.Trim();//订单号
                    if (code == null)//订单号未空，则视为空行
                    {
                        continue;
                    }
                    var recvMobile = worksheet.Cells[$"F{i}"].Value?.ToString()?.Trim();//收货人电话
                    var expressCode = worksheet.Cells[$"K{i}"].Value?.ToString()?.Trim();//快递单号
                    expressCode = expressCode?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
                    var expressType = worksheet.Cells[$"L{i}"].Value?.ToString()?.Trim();//快递公司编号
                    expressType = expressType?.Substring(expressType.IndexOf('-') + 1);
                    if (expressCode.IsNullOrEmpty() || expressType.IsNullOrEmpty())
                    {
                        errMsgs.AppendLine($"第{i}行没填物流单号或物流公司");
                    }
                    var order = new Domain.Order();
                    order.Code = code;                    
                    order.Mobile = recvMobile;
                    order.ExpressCode = expressCode;
                    order.ExpressType = expressType;
                    order.Modifier = request.UserId;
                    order.ModifyDateTime = DateTime.Now;
                    order.Remark = worksheet.Cells[$"L{i}"].Value?.ToString()?.Trim();

                    #region 校验
                    var checkData = CheckData(i, order);
                    string checkMsg = checkData.Item1;
                    if (string.IsNullOrEmpty(checkMsg))//校验通过
                    {
                        order.Id = checkData.Item2.Id;
                        order.Remark = checkData.Item2.Remark;//该字段暂时用来存快递公司名称
                        ls.Add(order);
                    }                        
                    else//校验不通过
                    {
                        errMsgs.AppendLine();
                        errMsgs.AppendLine(checkMsg);
                    }
                    #endregion
                }
                if (errMsgs.Length > 0)
                {
                    return ResponseResult.Failed(errMsgs.ToString());
                }
                if (ls.Any() == true && errMsgs.Length == 0)//全部校验通过
                {
                    for (int i = 0; i < ls.Count; i++)
                    {
                        var order = ls[i];
                        string sql = $@"
update [dbo].[Order] set ExpressCode=@ExpressCode,ExpressType=@ExpressType,SendExpressTime=getdate()
,Modifier=@Modifier,ModifyDateTime=GETDATE(),[status]=@orderstatus,[ShippingTime]=@time
where IsValid=1 and code=@code and Mobile=@Mobile

update [dbo].[OrderDetial] set [status]=@orderstatus where [orderid] in (select [id] from [order] where code=@code and Mobile=@Mobile)
";
                        var dp = new DynamicParameters();
                        dp.Set("ExpressCode", order.ExpressCode);
                        dp.Set("ExpressType", order.ExpressType);
                        dp.Set("Modifier", order.Modifier);
                        dp.Set("code", order.Code);
                        dp.Set("Mobile", order.Mobile);
                        dp.Set("orderstatus", OrderStatusV2.Shipping.ToInt());
                        dp.Set("time", DateTime.Now);
                        try
                        {
                            _orgUnitOfWork.DbConnection.Execute(sql, dp);

                            #region 微信通知         
                            try
                            {
                                var courseName = ""; //课程名称          
                                if (ls[i].Courseid != null)
                                {
                                    courseName = _orgUnitOfWork.QueryFirstOrDefault<string>($@" select title from dbo.Course where IsValid=1 and id='{ls[i].Courseid}'; ");
                                    courseName = $"《{courseName}》";
                                }
                                else
                                {
                                    var title = _orgUnitOfWork.QueryFirstOrDefault<string>($@"select top 1 json_value(ctn,'$.title') from OrderDetial where orderid='{ls[i].Id}'; ");
                                    courseName = $"《{title}》等商品";
                                }
                                var openid = _orgUnitOfWork.Query<string>($@" select openID from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and userID='{ls[i].Userid}'; ").FirstOrDefault();
                                var openId = openid;//代写            
                                var deleverCompany = ls[i].Remark; //物流公司
                                var deleverNo = order.ExpressCode;//物流单号
                                string msg = "";
                                msg += $"{deleverCompany}：{deleverNo}";
                                var wechatNotify = new WechatTemplateSendCmd()
                                {
                                    KeyWord1 = $"您购买的{courseName}已发货，{msg}。",
                                    KeyWord2 = DateTime.Now.ToDateTimeString(),
                                    OpenId = openId,
                                    Remark = "点击查看订单详情",
                                    MsyType = WechatMessageType.物流,
                                    OrderID = ls[i].Id
                                };
                                await _mediator.Send(wechatNotify);
                            }
                            catch { }
                            #endregion
                        }
                        catch
                        {
                            return ResponseResult.Failed($"Excel表第{i + 1}行及之后导入失败，请重新导入");
                        }

                    }

                }
                return ResponseResult.Success("导入成功");
            }
        }

        private (string, Domain.Order) CheckData(int row, Domain.Order order)
        {
            string checkMsg = "";            
            //1、订单号&收货人手机号，跟库匹配
            var dbOrder = _orgUnitOfWork.DbConnection.Query<Domain.Order>($"select * from [dbo].[Order] where IsValid=1 and code='{order.Code}' and mobile='{order.Mobile}'").FirstOrDefault();
            if (dbOrder == null)
                checkMsg += $"订单号[{order.Code}]与收货人手机号[{order.Mobile}]不属于同一个订单！";
            else 
            {
                if (dbOrder.Status != OrderStatusV2.Ship.ToInt() && !string.IsNullOrEmpty(dbOrder.ExpressCode))
                    checkMsg += $"订单状态为：{((OrderStatusV2)dbOrder.Status).GetDesc()}，不能导入物流信息！";

                order.Id = dbOrder.Id;
                order.Courseid = dbOrder.Courseid;
                order.Userid = dbOrder.Userid;
            }
            try
            {
                //2、物流单号、物流公司校验
                if (order.ExpressCode == null || order.ExpressType == null)
                    checkMsg += $"物流单号或者物流公司编号为空！";

                else
                {
                    order.Remark = order.Remark.Split('-')[0];
                    var rr = _mediator.Send(KuaidiServiceArgs.CheckNu(order.ExpressCode, order.ExpressType)).Result;
                    if (rr == null)
                    {
                        checkMsg += $"快递单号错误！";
                    }
                    var result = _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery() { Com = order.ExpressType, Nu = order.ExpressCode }).Result;
                    
                }

            }
            catch (Exception ex)
            {
                checkMsg += $"{ex.Message}！";
            }
            if (checkMsg != "")
                checkMsg = $"Excel表的第{row}行：{checkMsg}";
            return (checkMsg,order);
        }
    }
}
