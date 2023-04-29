using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 后台管理--购买留资列表
    /// </summary>
    public class SearchCoursesOrderQueryHandler : IRequestHandler<SearchCoursesOrderQuery, PagedList<CoursesOrderItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        public SearchCoursesOrderQueryHandler(IMediator mediator, IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public Task<PagedList<CoursesOrderItem>> Handle(SearchCoursesOrderQuery request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters();
            dy.Add("@skipCount", (request.PageIndex-1)*request.PageSize);
            dy.Add("@pageSize", request.PageSize);
            dy.Add("@keyValueType", KeyValueType.SubjectType);
            dy.Add("@orderType", OrderType.CourseBuy);

            #region where
            string where = $"  and ord.type={OrderType.BuyCourseByWx.ToInt()} ";//目前只查询微信购买的订单

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
            //科目
            if (request.SubjectId != null && request.SubjectId > 0)
            {
                if (Enum.IsDefined(typeof(SubjectEnum), request.SubjectId))
                {
                    where += " and c.subject=@subject ";
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
            // 下单人手机号
            if (!string.IsNullOrEmpty(request.Mobile))
            {
                where += $"   and ord.Mobile=@Mobile ";
                dy.Add("Mobile", request.Mobile);
            }
            #endregion

            string listSql = $@" 
                                SELECT ROW_NUMBER()over(order by CreateTime desc) as RowNum,* from (select DISTINCT ord.id,c.id as courseid
                                ,CONVERT(varchar(12) , ord.CreateTime, 111 )+' '+CONVERT(varchar(12) , ord.CreateTime, 108 ) as CreateTime
                                ,org.name as orgname,c.Title,ky.[name] as [subject],c.price,ord.mobile,ord.userid,'' as UserName,ord.status
                                ,case ord.status when 3 then '待发货' else (case  ord.status when 4 then '已发货' else (case  ord.status when 5 then '退货中' else (case ord.status when 6 then '已退货' else '已取消' end)end) end) end as StatusEnumDesc  
                                ,appointmentStatus
                                ,ord.code
                                ,ord.recvProvince,ord.recvCity 
                                ,pro.[Name]+'-'+item.[Name] as SetMeal
                                ,ord.ExpressCode
                                ,ord.code as ExchangeCode                               
                                from  [dbo].[Order] as ord
                                --只有一个属性可用该法
                                left join [dbo].[OrderDetial] as det on det.orderid=ord.id
                                left join [dbo].[CourseGoodsPropItem] as proI on det.productid=proI.GoodsId 
                                left join [dbo].[CoursePropertyItem] as item on proI.PropItemId=item.Id and item.IsValid=1
                                left join [dbo].[CourseProperty] as pro on pro.IsValid=1 and pro.Id=item.Propid and pro.IsValid=1

                                left join  [dbo].[Course] as c on ord.courseid=c.id and  c.IsValid=1 								
                                left join [dbo].[Organization] as org on c.orgid=org.id and org.IsValid=1
                                left join (SELECT id, value AS type FROM [Organization]CROSS APPLY OPENJSON(types)) TT on org.id=TT.id
                                left join [dbo].[KeyValue] as ky on ky.[key]=c.subject and ky.IsValid=1 and ky.[type]=@keyValueType  
                                left join [dbo].[Exchange] ex on ex.orderid=ord.id and ex.IsValid=1 and ex.status={ExchangeStatus.Converted.ToInt()}
                                where ord.IsValid=1  {where}
                                )TT
                                order by rownum OFFSET @skipCount ROWS FETCH NEXT @pageSize ROWS ONLY
                            ;";
            string countSql = $@" 
                               SELECT count(DISTINCT ord.id)
                               from  [dbo].[Order] as ord
                               left join  [dbo].[Course] as c on ord.courseid=c.id and  c.IsValid=1 								
                               left join [dbo].[Organization] as org on c.orgid=org.id and org.IsValid=1
                               left join (SELECT id, value AS type FROM [Organization]CROSS APPLY OPENJSON(types)) TT on org.id=TT.id
                              --left join [dbo].[KeyValue] as ky on ky.[key]=c.subject and ky.IsValid=1 and ky.[type]=@keyValueType                                
                               where ord.IsValid=1   {where}
                             ;";
            var totalItemCount = _orgUnitOfWork.DbConnection.Query<int>(countSql,dy).FirstOrDefault();
            var data = _orgUnitOfWork.DbConnection.Query<CoursesOrderItem>(listSql, dy).ToList();

            #region 用户信息
            var userInfo = _mediator.Send(new UserSimpleInfoQuery() { UserIds =data.Select(_=>_.UserId) }).Result;
            data.ForEach(_d => _d.UserName=userInfo.FirstOrDefault(_u=>_u.Id==_d.UserId).Nickname);
            #endregion

            #region 约课状态       
            var bookingCourse = EnumUtil.GetSelectItems<BookingCourseStatusEnum>();
            bookingCourse.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "请选择", Value = "-1" });
            for (int i = 0; i < data.Count; i++)
            {
                data[i].AppointmentStatusList = GetSelectListItems(bookingCourse,data[i].AppointmentStatus);
            }
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



            var result= data.ToPagedList(request.PageSize, request.PageIndex, totalItemCount);
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
    }
}
