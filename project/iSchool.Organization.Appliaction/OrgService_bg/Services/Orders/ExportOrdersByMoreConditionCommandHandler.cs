using AutoMapper;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.OrgService_bg.Course;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{
    public class ExportOrdersByMoreConditionCommandHandler : IRequestHandler<ExportOrdersByMoreConditionCommand, string>
    {
        IMediator _mediator;
        IMapper _mapper;
        IConfiguration _config;
        OrgUnitOfWork _orgUnitOfWork;

        public ExportOrdersByMoreConditionCommandHandler(IMediator mediator, IMapper mapper, IOrgUnitOfWork orgUnitOfWork,
            IConfiguration config)
        {
            this._mediator = mediator;
            this._mapper = mapper;
            this._config = config;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<string> Handle(ExportOrdersByMoreConditionCommand cmd, CancellationToken cancellation)
        {
            IEnumerable<(string r1c, Func<int, int, CoursesOrderItem, object> wr)> Wcell()
            {
                yield return ("(预)订单号", (row, col, data) => data.AdvanceOrderNo);
                yield return ("(子)订单号", (row, col, data) => data.Code);
                yield return ("支付时间", (row, col, data) => (data.PaymentTime?.ToString("yyyy-MM-dd HH:mm:ss")) ?? "");
                yield return ("机构名称", (row, col, data) => data.OrgName);
                yield return ("课程名称", (row, col, data) => data.Title);
                yield return ("属性", (row, col, data) => data.SetMeal);
                yield return ("数量", (row, col, data) => data.Number);
                yield return ("商品单价", (row, col, data) => Math.Round(data.Price, 2, MidpointRounding.AwayFromZero));
                yield return ("成本价", (row, col, data) => Math.Round(data.Costprice, 2, MidpointRounding.AwayFromZero));
                yield return ("货号", (row, col, data) => data.ArticleNo);
                yield return ("订单金额", (row, col, data) => data.TotalPayment == null ? "" : Math.Round(data.TotalPayment.Value, 2, MidpointRounding.AwayFromZero).ToString());
                yield return ("下单人电话", (row, col, data) => data.Mobile);
                yield return ("下单人", (row, col, data) => data.UserName);
                yield return ("收货人电话", (row, col, data) => data.RecvMobile);
                yield return ("收货人", (row, col, data) => data.RecvUserName);
                yield return ("上课电话", (row, col, data) => data.BeginClassMobile);
                yield return ("微信昵称", (row, col, data) => data.WXNickName);
                yield return ("年龄", (row, col, data) => data.Age);
                yield return ("省", (row, col, data) => data.RecvProvince);
                yield return ("市", (row, col, data) => data.RecvCity);
                yield return ("区", (row, col, data) => data.RecvArea);
                yield return ("具体地址", (row, col, data) => data.Address);
                yield return ("订单状态", (row, col, data) => ((OrderStatusV2)data.Status).GetDesc());
                yield return ("约课状态", (row, col, data) => data.AppointmentStatus == null ? "" : ((BookingCourseStatusEnum)data.AppointmentStatus).GetDesc());
                yield return ("物流单号", (row, col, data) => data.ExpressCode);
                yield return ("兑换码", (row, col, data) => data.ExchangeCode);
                yield return ("订单备注", (row, col, data) => data.Remark);
                yield return ("机构反馈", (row, col, data) => data.SystemRemark);
                yield return ("是否小程序订单", (row, col, data) => data.IsMinOrder);
            }

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Sheet1");
            int row = 1, col = 1;
            foreach (var (r1c, _) in Wcell())
            {
                sheet.Cells[row, col++].Value = r1c;
            }

            var items = await GetDatas(cmd);
            foreach (var item in items)
            {
                row++; col = 1;
                foreach (var (_, wr) in Wcell())
                {
                    sheet.Cells[row, col].Value = wr(row, col, item)?.ToString();
                    col++;
                }
            }

            var id = Guid.NewGuid().ToString("n");
            package.SaveAs(new FileInfo(Path.Combine(AppContext.BaseDirectory, _config["AppSettings:XlsxDir"], $"{id}.xlsx")));
            return id;
        }

        protected async Task<IEnumerable<CoursesOrderItem>> GetDatas(ExportOrdersByMoreConditionCommand cmd)
        {
            var sql = $@"
SELECT ord.id,ord.AdvanceOrderId,ord.AdvanceOrderNo, det.Ctn as Ctn0

, CONVERT(varchar(12), ord.CreateTime, 111 )+' ' + CONVERT(varchar(12), ord.CreateTime, 108) as CreateTime

,(case when json_value(det.ctn,'$.orgName') is not null then json_value(det.ctn,'$.orgName') else org.name end)as orgname
,(case when json_value(det.ctn,'$.title') is not null then json_value(det.ctn,'$.title') else c.title end)as title
,det.price,det.number
   
,ord.userid,'' as UserName,det.status
,appointmentStatus,ord.totalpayment
,ord.code,ord.PaymentTime,ord.age,ord.RecvArea,ord.Address
,ord.recvProvince,ord.recvCity,ord.recvUsername,ord.mobile as RecvMobile
,ord.Remark,ord.SystemRemark,ord.ExpressCode,cg.Costprice,cg.ArticleNo
,(select top 1 ex.code from [dbo].[Exchange] ex where ex.orderid=ord.id and ex.IsValid=1 and ex.status=1)as ExchangeCode
FROM dbo.[Order] as ord
left join [dbo].[OrderDetial] as det on det.orderid = ord.id
left join [dbo].[CourseGoods] as cg on det.productid = cg.Id and cg.IsValid = 1
LEFT JOIN dbo.Course as c ON det.courseid = c.id and c.IsValid = 1
LEFT JOIN [dbo].[Organization] as org on c.orgid = org.id and org.IsValid = 1
where ord.IsValid=1 and ord.type>={OrderType.BuyCourseByWx.ToInt()} {{0}}
order by ord.createtime desc,ord.AdvanceOrderId
";
            var dy = new DynamicParameters();
            var where = "";
            #region where
            //机构
            if (cmd.OrgId != null && cmd.OrgId != default)
            {
                where += "  and org.id=@orgId ";
                dy.Add("@orgId", cmd.OrgId);
            }
            //机构下的课程
            if (cmd.CourseId != null && cmd.CourseId != default)
            {
                where += "  and c.id=@cId ";
                dy.Add("@cId", cmd.CourseId);
            }
            //机构类型
            if (cmd.OrgTypeId != null && cmd.OrgTypeId > 0)
            {
                dy.Add("@orgType", cmd.OrgTypeId);

                where += $" and exists(select 1 from openjson(org.types)j1 where j1.[value]=@orgType) ";
            }
            //科目Subjects
            if (cmd.SubjectId != null && cmd.SubjectId > 0)
            {
                if (Enum.IsDefined(typeof(SubjectEnum), cmd.SubjectId))
                {
                    dy.Add("@subject", cmd.SubjectId);

                    where += " and exists(select 1 from openjson(c.Subjects)j1 where j1.[value]=@subject) ";
                }
            }
            //订单状态
            if (cmd.Status != null)
            {
                if (Enum.IsDefined(typeof(OrderStatusV2), cmd.Status))
                {
                    where += $"   and det.status=@status  ";
                    dy.Add("@status", cmd.Status);
                }

            }
            //课程名称
            if (!string.IsNullOrEmpty(cmd.Title?.Trim()))
            {
                where += $"   and c.Title like @title  ";
                dy.Set("title", $"%{cmd.Title}%");
            }
            //订单号
            if (!string.IsNullOrEmpty(cmd.OrdCode))
            {
                where += $"   and ord.code=@code ";
                dy.Add("code", cmd.OrdCode);
            }


            //下单时间
            if (cmd.StartTime != null && cmd.EndTime != null)//
            {
                where += " and ord.CreateTime between @StartTime and @EndTime  ";
                dy.Add("@StartTime", cmd.StartTime);

                DateTime etime = ((DateTime)cmd.EndTime).AddHours(23).AddMinutes(59).AddSeconds(59);
                dy.Add("@EndTime", etime);
            }

            // 下单人手机号
            if (!string.IsNullOrEmpty(cmd.Mobile?.Trim()))
            {
                var centerUsers = await _mediator.Send(new UserInfosByPhonesQuery { OrdMobile = cmd.Mobile.Trim(), UserIds = new List<Guid>() });
                var userIds = centerUsers?.Select(_ => _.UserId)?.Distinct()?.ToList();
                if (userIds.Any() == true)
                {
                    where += $"   and ord.userid in ('{string.Join("','", userIds)}') ";
                }
                else
                {
                    where += $"   and 1=2 ";
                }
            }


            //收货人手机号
            if (!string.IsNullOrEmpty(cmd.RecvMobile?.Trim()))
            {
                where += $"   and ord.mobile=@RecvMobile ";
                dy.Set("RecvMobile", cmd.RecvMobile);
            }
            //上课电话
            if (!string.IsNullOrEmpty(cmd.BeginClassMobile?.Trim()))
            {
                where += $" and  ord.BeginClassMobile=@BeginClassMobile ";
                dy.Set("BeginClassMobile", cmd.BeginClassMobile);
            }
            #endregion

            sql = string.Format(sql, where);
            var datas = (await _orgUnitOfWork.DbConnection.QueryAsync<CoursesOrderItem>(sql, dy)).AsList();

            // ctn
            foreach (var item in datas)
            {
                var ctn0 = item.Ctn0?.ToString();
                if (string.IsNullOrEmpty(ctn0)) item.Ctn = new JObject();
                else item.Ctn = JObject.Parse(ctn0);

                item.SetMeal = string.Join('-', ((JArray)item.Ctn["propItemNames"] ?? new JArray()).Select(_ => (string)_).Where(_ => !string.IsNullOrEmpty(_)));
                item.IsMinOrder = !string.IsNullOrEmpty((string)item.Ctn["_Ver"]) ? "是" : "否";
            }

            // 机构反馈
            foreach (var _d in datas)
            {
                if (!string.IsNullOrEmpty(_d.SystemRemark))
                {
                    var builder = new StringBuilder();
                    var sRList = _d.SystemRemark.Split("||");
                    foreach (var item in sRList)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            builder.Append(item);
                            builder.AppendLine();
                        }
                    }
                    _d.SystemRemark = builder.ToString();
                }
            }

            // users
            {
                var usersId = datas.Select(_ => _.UserId).Distinct().ToList();
                if (usersId.Any() == true)
                {
                    var userInfo = await _mediator.Send(new UserInfosByUserIdsOrMobileQuery() { UserIds = usersId });
                    datas.ForEach(_d =>
                    {
                        var u = userInfo.FirstOrDefault(_u => _u.UserId == _d.UserId);
                        _d.UserName = u?.NickName;
                        _d.Mobile = u?.Mobile;
                        _d.WXNickName = u?.WXNickName;
                        _d.StatusEnumDesc = ((OrderStatusV2)_d.Status).GetDesc();
                    });
                }
            }

            return datas;
        }

    }
}
