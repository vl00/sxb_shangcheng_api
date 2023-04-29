using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 后台管理--课程订单列表
    /// </summary>
    public class SearchOrdersQueryHandler : IRequestHandler<SearchOrdersQuery, PagedList<CoursesOrderItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        public SearchOrdersQueryHandler(IMediator mediator, IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public Task<PagedList<CoursesOrderItem>> Handle(SearchOrdersQuery request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters();
            dy.Add("@skipCount", (request.PageIndex - 1) * request.PageSize);
            dy.Add("@pageSize", request.PageSize);
            dy.Add("@keyValueType", KeyValueType.SubjectType);
            dy.Add("@orderType", OrderType.CourseBuy);

            #region where
            string where = $"  and ord.type>={OrderType.BuyCourseByWx.ToInt()} ";//目前只查询微信购买的订单

            //供应商
            if (request.SupplierId != null && request.SupplierId != default)
            {
                where += "  and supplier.id=@supplierid ";
                dy.Add("@supplierid", request.SupplierId);
            }

            //机构
            if (request.OrgId != null && request.OrgId != default)
            {
                where += "  and org.id=@orgId ";
                dy.Add("@orgId", request.OrgId);
            }
            //机构下的课程
            if (request.CourseId != null && request.CourseId != default)
            {
                where += "  and c.id=@cId ";
                dy.Add("@cId", request.CourseId);
            }
            //机构类型
            if (request.OrgTypeId != null && request.OrgTypeId > 0)
            {
                where += "  and TT.type=@orgType ";
                dy.Add("@orgType", request.OrgTypeId);
            }
            //科目Subjects
            if (request.SubjectId != null && request.SubjectId > 0)
            {
                if (Enum.IsDefined(typeof(SubjectEnum), request.SubjectId))
                {
                    where += " and SS.csubject=@subject ";
                    dy.Add("@subject", request.SubjectId);
                }
            }
            //订单状态
            if (request.Status != null)
            {
                if (Enum.IsDefined(typeof(OrderStatusV2), request.Status))
                {
                    where += $"   and ord.status=@status  ";
                    dy.Add("@status", request.Status);
                }

            }
            //课程名称
            if (!string.IsNullOrEmpty(request.Title?.Trim()))
            {
                where += $"   and c.Title like '%{request.Title}%'  ";
            }
            //订单号
            if (!string.IsNullOrEmpty(request.OrdCode))
            {
                where += $"   and ord.code=@code ";
                dy.Add("code", request.OrdCode);
            }
            //订单号
            if (!string.IsNullOrEmpty(request.PayOrderNo))
            {
                where += $"   and exists (select 1 from [10.1.0.181].iSchoolFinance.dbo.PayOrder where OrderNo=@OrderNo and OrderId=ord.AdvanceOrderId) ";
                dy.Add("OrderNo", request.PayOrderNo);
            }
            // 下单人手机号
            if (!string.IsNullOrEmpty(request.Mobile?.Trim()))
            {
                var centerUsers = _mediator.Send(new UserInfosByPhonesQuery() { OrdMobile = request.Mobile.Trim(), UserIds = new List<Guid>() }).Result;
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
            if (!string.IsNullOrEmpty(request.RecvMobile?.Trim()))
            {
                where += $"   and ord.mobile=@RecvMobile ";
                dy.Set("RecvMobile", request.RecvMobile);
            }
            //上课电话
            if (!string.IsNullOrEmpty(request.BeginClassMobile?.Trim()))
            {
                where += $" and  ord.BeginClassMobile=@BeginClassMobile ";
                dy.Set("BeginClassMobile", request.BeginClassMobile);
            }
            //商品分类
            if (request.CourseType != null && Enum.IsDefined(typeof(CourseTypeEnum), request.CourseType))
            {
                where += $" and c.type=@coursetype  ";
                dy.Set("coursetype", request.CourseType);
            }

            //付款时间
            if (request.StartTime != null && request.EndTime != null)//
            {
                where += " and ord.paymenttime between @StartTime and @EndTime  ";
                dy.Add("@StartTime", request.StartTime);

                DateTime etime = ((DateTime)request.EndTime).AddHours(23).AddMinutes(59).AddSeconds(59);
                dy.Add("@EndTime", etime);
            }

            #endregion

            string pageItemsSql = $@" select DISTINCT ord.id,c.id as courseid
                                            ,CONVERT(varchar(12) , ord.CreateTime, 111 )+' '+CONVERT(varchar(12) , ord.CreateTime, 108 ) as CreateTime
                                            ,org.name as orgname,c.Title
            ,c.Subjects as [subject]
            ,det.price,det.Origprice,det.Point,isnull(discount.DiscountAmount,0) couponAmount,ord.userid,'' as UserName,ord.status                                 
                                            ,appointmentStatus,ord.totalpayment,det.Payment,ord.totalpoints
                                            ,ord.code,ord.PaymentTime,ord.age,ord.RecvArea,ord.Address
                                            ,ord.recvProvince,ord.recvCity,ord.recvUsername,ord.mobile  as RecvMobile
                                            ,ord.Remark ,ord.SystemRemark,ord.AdvanceOrderId,ord.AdvanceOrderNo
                                            ,det.Ctn as Ctn0,det.Id OrderDetailId,det.producttype     
                                            ,ord.ExpressType,ISNULL(ord.ExpressCode,(STUFF((SELECT DISTINCT '\n\n'+ExpressCode FROM dbo.OrderLogistics 
											WHERE IsValid=1 AND OrderDetailId=det.id FOR XML PATH('')) , 1, 2, '')))
											ExpressCode ,ord.SendExpressTime 
                                            ,ex.code as ExchangeCode ,ord.BeginClassMobile 
                                            ,(CASE WHEN CHARINDEX(det.ctn,'costprice')>0 THEN (SELECT costprice FROM OPENJSON(det.ctn)WITH (costprice DECIMAL(18,2))) ELSE  cg.Costprice END) Costprice,cg.ArticleNo,supplier.Name AS SupplierName
                                            ,det.number,ord.IsMultipleExpress
                                            from  [dbo].[Order] as ord
                                            left join [dbo].[OrderDetial] as det on det.orderid=ord.id
                                            LEFT JOIN dbo.OrderDiscount AS discount ON det.id=discount.OrderId
                                            left join [dbo].[CourseGoods] as cg on det.productid=cg.Id and cg.IsValid=1
                                            LEFT JOIN dbo.Supplier AS supplier ON supplier.Id=cg.SupplierId
                                            --left join [dbo].[OrderDetial] as det on det.orderid=ord.id
                                            --left join [dbo].[CourseGoodsPropItem] as proI on det.productid=proI.GoodsId 
                                            --left join [dbo].[CoursePropertyItem] as item on proI.PropItemId=item.Id and item.IsValid=1
                                            --left join [dbo].[CourseProperty] as pro on pro.IsValid=1 and pro.Id=item.Propid and pro.IsValid=1

                                            left join  [dbo].[Course] as c on cg.courseid=c.id and  c.IsValid=1 								
                                            left join [dbo].[Organization] as org on c.orgid=org.id and org.IsValid=1
                                            left join (SELECT id, value AS type FROM [Organization]CROSS APPLY OPENJSON(types)) TT on org.id=TT.id
                                            left join (SELECT id, value AS csubject FROM [Course]CROSS APPLY OPENJSON(Subjects)) SS on c.id=SS.id
                                            --left join [dbo].[KeyValue] as ky on ky.[key]=c.subject and ky.IsValid=1 and ky.[type]=@keyValueType  
                                            left join [dbo].[Exchange] ex on ex.orderid=ord.id and ex.IsValid=1 and ex.status={ExchangeStatus.Converted.ToInt()}
                                            --left join [dbo].[KuaidiNuData] kd on kd.nu=ord.ExpressCode and kd.Company=ord.ExpressType
                                            where ord.IsValid=1  {where} ";


            //    var pageItemsSql = $@"SELECT 
            //                         ord.id,STUFF(
            //                     (SELECT DISTINCT '\n\n' + title + '+' + propname + '*' + CAST(detial.number AS NVARCHAR(100)) FROM dbo.OrderDetial AS detial
            //                      CROSS  APPLY  OPENJSON(ctn) WITH(title  NVARCHAR(100) '$.title', propname NVARCHAR(100) '$.propItemNames[0]')
            //                      WHERE detial.orderid = ord.id  FOR XML PATH('')) , 1, 2, ''
            //) AS title
            //                        , CONVERT(varchar(12), ord.CreateTime, 111 )+' ' + CONVERT(varchar(12), ord.CreateTime, 108) as CreateTime
            //                        ,MAX(org.name) as orgname,ord.userid,'' as UserName,ord.status
            //                        ,appointmentStatus,ord.totalpayment
            //                        ,ord.code,ord.PaymentTime,ord.age,ord.RecvArea,ord.Address
            //                        ,ord.recvProvince,ord.recvCity,ord.recvUsername,ord.mobile as RecvMobile
            //                        ,ord.Remark ,ord.SystemRemark,ord.AdvanceOrderId,ord.AdvanceOrderNo,ord.ExpressCode,ex.code as ExchangeCode
            //                         FROM dbo.[Order] as ord
            //                        left join[dbo].[OrderDetial] as det on det.orderid = ord.id
            //                        left join[dbo].[CourseGoods] as cg on det.productid = cg.Id and cg.IsValid = 1
            //                        LEFT JOIN  dbo.Course as c ON det.courseid = c.id and c.IsValid = 1
            //                        LEFT JOIN[dbo].[Organization] as org on c.orgid = org.id and org.IsValid = 1
            //                        left join(SELECT id, value AS type FROM [Organization] CROSS APPLY OPENJSON(types)) TT on org.id = TT.id
            //                        left join(SELECT id, value AS csubject FROM [Course] CROSS APPLY OPENJSON(Subjects)) SS on c.id = SS.id
            //                        left join[dbo].[Exchange] ex on ex.orderid = ord.id and ex.IsValid = 1 and ex.status = 1
            //                        where ord.IsValid = 1 {where} GROUP BY ord.id, ord.CreateTime,ord.code,ord.age,ord.RecvArea,ord.Address
            //,ord.recvProvince,ord.recvCity,ord.recvUsername,ord.mobile ,ord.Remark ,ord.SystemRemark,ord.userid,ord.status
            //,appointmentStatus,ord.totalpayment,ord.PaymentTime,ord.AdvanceOrderId,ord.AdvanceOrderNo,ord.ExpressCode,ex.code 
            //                    ";

            string listSql = $@" 
                                SELECT ROW_NUMBER()over(order by CreateTime desc) as RowNum,* from (
                                {pageItemsSql}
                                )TT
                                order by rownum OFFSET @skipCount ROWS FETCH NEXT @pageSize ROWS ONLY
                            ;";
            string countSql = $@" 
                    SELECT COUNT(1) FROM(
                                SELECT  detail.id
                               from  [dbo].[Order] as ord
		                       LEFT JOIN  dbo.OrderDetial AS detail ON detail.orderid=ord.id
                                left join [dbo].[CourseGoods] as cg on detail.productid=cg.Id and cg.IsValid=1
                               left join  [dbo].[Course] as c on detail.courseid=c.id and  c.IsValid=1 								
                               left join [dbo].[Organization] as org on c.orgid=org.id and org.IsValid=1
                                LEFT JOIN dbo.Supplier AS supplier ON supplier.Id=cg.SupplierId
                               left join (SELECT id, value AS type FROM [Organization]CROSS APPLY OPENJSON(types)) TT on org.id=TT.id
                               left join (SELECT id, value AS csubject FROM [Course]CROSS APPLY OPENJSON(Subjects)) SS on c.id=SS.id
                              --left join [dbo].[KeyValue] as ky on ky.[key]=c.subject and ky.IsValid=1 and ky.[type]=@keyValueType                        
                               where ord.IsValid=1   {where} group by  detail.id)a
                             ;";



            var data = request.SearchType == 999 ? _orgUnitOfWork.Query<CoursesOrderItem>(pageItemsSql + " order by CreateTime desc", dy).ToList() : _orgUnitOfWork.Query<CoursesOrderItem>(listSql, dy).ToList();

            var totalItemCount = request.SearchType == 999 ? data.Count() : _orgUnitOfWork.Query<int>(countSql, dy).FirstOrDefault();
            if (data.Any() == false)
            {
                var rr = data.ToPagedList(request.PageSize, request.PageIndex, totalItemCount);
                return Task.FromResult(rr);
            }

            foreach (var item in data)
            {
                item.Ctn = !item.Ctn0.IsNullOrEmpty() ? JObject.Parse(item.Ctn0) : new JObject();
                if (item.CourseId == default) item.CourseId = (Guid?)item.Ctn["id"] ?? default;
                item.OrgName ??= (string)item.Ctn["orgName"] ?? "";
                item.Title ??= (string)item.Ctn["title"] ?? "";
            }

            #region 用户信息

            var courseids = data.Select(_ => _.CourseId).ToList();

            var proInfos = GetProInfo(courseids);
            var usersId = data.Select(_ => _.UserId).Distinct().ToList();
            if (usersId.Any() == true)
            {
                var userInfo = _mediator.Send(new UserInfosByUserIdsOrMobileQuery() { UserIds = usersId }).Result;
                data.ForEach(_d =>
                {
                    var u = userInfo.FirstOrDefault(_u => _u.UserId == _d.UserId);
                    _d.UserName = u?.NickName;
                    _d.Mobile = u?.Mobile;
                    _d.WXNickName = u?.WXNickName;
                    //_d.StatusEnumDesc = GetOrderStatusV2Desc(_d.Status.ToString());
                    _d.StatusEnumDesc = ((OrderStatusV2)_d.Status).GetDesc();
                    if (!string.IsNullOrEmpty(_d.Ctn0))
                    {
                        var proItem = JsonSerializationHelper.JSONToObject<CourseGoodsOrderCtnDto>(_d.Ctn0);
                        try
                        {
                            var propItem = proInfos.FirstOrDefault(_ => _.itemId == proItem.PropItemIds[0]);
                            _d.SetMeal = propItem.Name + "-" + proItem.PropItemNames[0];
                        }
                        catch { }
                        //_d.SetMeal = proInfos.FirstOrDefault(_ => _.itemId == proItem.PropItemIds[0]).Name + "-" + proItem.PropItemNames[0];
                        _d.IsMinOrder = !string.IsNullOrEmpty(proItem._Ver) ? "是" : "否";
                    }
                    //机构反馈
                    if (!string.IsNullOrEmpty(_d.SystemRemark))
                    {
                        StringBuilder builder = new StringBuilder();
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

                    #region 暂时不启用
                    //科目()
                    //if (!string.IsNullOrEmpty(_d.Subject))
                    //{
                    //    var list = JsonSerializationHelper.JSONToObject<List<int>>(_d.Subject);
                    //    var subjectNames = new List<string>();
                    //    foreach (var item in list)
                    //    {
                    //        subjectNames.Add(((SubjectEnum)item).GetDesc());
                    //    }
                    //    _d.Subject = string.Join(",", subjectNames);
                    //} 
                    #endregion
                });

            }



            #endregion

            if (request.SearchType != 999)//导出不需要
            {
                #region 约课状态下拉框       
                var bookingCourse = EnumUtil.GetSelectItems<BookingCourseStatusEnum>();
                bookingCourse.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "请选择", Value = "-1" });
                for (int i = 0; i < data?.Count; i++)
                {
                    data[i].AppointmentStatusList = GetSelectListItems(bookingCourse, data[i].AppointmentStatus);
                }
                #endregion
            }

            #region 机构反馈

            #endregion

            #region 直接用前端用户输入的名称，而不是登录用户名称
            //#region 获取用户信息--用户昵称            
            //var userInfo = _mediator.Send(new UserSimpleInfoQuery() {  UserIds= data.Select(_ => _.UserId) }).Result;
            //for (int i = 0; i < data.Count; i++)
            //{
            //    data[i].UserName = userInfo.FirstOrDefault(_ => _.Id == data[i].UserId).Nickname;               
            //}
            //#endregion

            #endregion



            var result = data.ToPagedList(request.PageSize, request.PageIndex, totalItemCount);
            return Task.FromResult(result);
        }

        private List<SelectListItem> GetSelectListItems(List<SelectListItem> bookingCourse, int? selectedValue)
        {

            var retOptions = new List<SelectListItem>();
            for (int i = 0; i < bookingCourse.Count; i++)
            {
                var item = new SelectListItem();
                item.Value = bookingCourse[i].Value;
                item.Text = bookingCourse[i].Text;
                if (bookingCourse[i].Value == selectedValue?.ToString())
                    item.Selected = true;
                else
                    item.Selected = false;

                retOptions.Add(item);
            }
            return retOptions;
        }

        private string GetOrderStatusV2Desc(string status)
        {
            var ordStatues = EnumUtil.GetSelectItems<OrderStatusV2>();
            foreach (var item in ordStatues)
            {
                if (item.Value == status)
                    return item.Text;
            }
            return "";
        }


        private List<ItemId_ProInfo> GetProInfo(List<Guid> courseids)
        {
            string sql = $@"select distinct item.Id as itemId,pro.* from [dbo].[CourseProperty] as  pro
left join [dbo].[CoursePropertyItem] as item on item.Propid=pro.Id and item.IsValid=1 and pro.Courseid=item.Courseid
where pro.Courseid in('{string.Join("','", courseids)}')";

            return _orgUnitOfWork.Query<ItemId_ProInfo>(sql).ToList();

        }

    }

    public class ItemId_ProInfo : CourseProperty
    {
        /// <summary>
        /// 选项Id
        /// </summary>
        public Guid itemId { get; set; }
    }


}
