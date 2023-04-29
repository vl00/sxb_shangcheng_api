using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 后台管理--课程列表
    /// </summary>
    public class SearchCoursesQueryHandler : IRequestHandler<SearchCoursesQuery, PagedList<CoursesItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public SearchCoursesQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<PagedList<CoursesItem>> Handle(SearchCoursesQuery request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters();
            dy.Add("@skipCount", (request.PageIndex-1)*request.PageSize);
            dy.Add("@pageSize", request.PageSize);
            dy.Add("@keyValueType", KeyValueType.SubjectType);
            dy.Add("@orderStatus", OrderStatus.Returned);
            dy.Add("@orderType", OrderType.CourseBuy);
            string where ="";

            //机构
            if(request.OrgId!=null && request.OrgId != default)
            {
                where += "  and org.id=@orgId ";
                dy.Add("@orgId", request.OrgId);
            }

            //供应商
            if (request.SupplierId != null && request.SupplierId != default)
            {
                where += "  and cg.SupplierId=@supplierId ";
                dy.Add("@supplierId", request.SupplierId);
            }

            //科目
            if (request.SubjectId!=null && request.SubjectId != default)
            {
                if (Enum.IsDefined(typeof(SubjectEnum), request.SubjectId))
                {       
                    where += " and SS.csubject=@subject ";
                    dy.Add("@subject", request.SubjectId);
                }                
            }

            //课程状态
            if (request.Status != null)
            {
                if (Enum.IsDefined(typeof(CourseStatusEnum), request.Status))
                {
                    where += $"   and c.status=@status  ";
                    dy.Add("@status", (CourseStatusEnum)request.Status);
                }
            }

            //商品分类
            if (request.Type != null)
            {
                if (Enum.IsDefined(typeof(CourseTypeEnum), request.Type))
                {
                    where += $"   and c.Type=@Type  ";
                    dy.Add("@Type", (CourseTypeEnum)request.Type);
                }
            }

            //课程标题
            if (!string.IsNullOrEmpty(request.Title?.Trim()))
            {
                where += $"   and c.title like '%{request.Title}%'  ";
            }
            
            string listSql = $@" 
select ROW_NUMBER()over(order by CreateTime desc) as RowNum,* from (
SELECT distinct c.CreateTime,c.id,org.name as orgname,org.id as OrgId
,c.title,c.subtitle,c.Subjects,c.GoodthingTypes,c.Type
--ky.[name] as [subject],
,c.price,c.stock,c.sellcount,c.[count],c.subject as SubjectId
,c.ChargebackCount,c.[status],c.no as Id_s,c.IsInvisibleOnline,c.no 
,CONVERT(varchar(12) , c.LastOnShelfTime, 111 ) as LastOnShelfTime
,CONVERT(varchar(12) , c.LastOffShelfTime, 111 ) as LastOffShelfTime
,Suppliernames = STUFF((
SELECT DISTINCT ',' + r.name
FROM [dbo].CourseGoods AS cg
inner join [dbo].Supplier r on cg.SupplierId = r.ID and r.IsValid = 1
WHERE cg.courseid =c.id FOR XML PATH('')), 1, 1, '')
 from  [dbo].[Course] as c 
left join (SELECT id, value AS csubject FROM [Course]CROSS APPLY OPENJSON(Subjects)) SS on c.id=SS.id
left join [dbo].[Organization] as org on c.orgid=org.id and org.IsValid=1
left join CourseGoods cg on cg.courseid = c.id and cg.IsValid=1
--left join [dbo].[KeyValue] as ky on ky.[key]=c.subject and ky.IsValid=1 and ky.[type]=@keyValueType
--left join (select count(1) as ChargebackCount,courseid from [dbo].[Order] where IsValid=1 and status=@orderStatus  and type=@orderType group by  courseid) as o on o.courseid=c.id
where c.IsValid=1 {where}
)TT
order by rownum OFFSET @skipCount ROWS FETCH NEXT @pageSize ROWS ONLY
;";
            string countSql = $@" 
SELECT count(distinct c.id)
 from  [dbo].[Course] as c 
left join (SELECT id, value AS csubject FROM [Course]CROSS APPLY OPENJSON(Subjects)) SS on c.id=SS.id
left join [dbo].[Organization] as org on c.orgid=org.id and org.IsValid=1
left join CourseGoods cg on cg.courseid = c.id and cg.IsValid=1
--left join [dbo].[KeyValue] as ky on ky.[key]=c.subject and ky.IsValid=1 and ky.[type]=@keyValueType
--left join (select count(1) as ChargebackCount,courseid from [dbo].[Order] where IsValid=1 and status=@orderStatus  and type=@orderType group by  courseid) as o on o.courseid=c.id
where c.IsValid=1 {where}
;";
            var totalItemCount = _orgUnitOfWork.DbConnection.Query<int>(countSql,dy).FirstOrDefault();
            var items = _orgUnitOfWork.DbConnection.Query<CoursesItem>(listSql, dy).ToList();
            foreach (var item in items)
            {
                item.Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(item.Id_s));

                var subOrGoodsNames = new List<string>();
                //科目
                if (item.Type == CourseTypeEnum.Course.ToInt() && !string.IsNullOrEmpty(item.Subjects))
                {
                    item.SubjectsOrGoodthingTypes = item.Subjects;
                    var list = JsonSerializationHelper.JSONToObject<List<int>>(item.SubjectsOrGoodthingTypes);
                    foreach (var e in list)
                    {
                        subOrGoodsNames.Add(((SubjectEnum)e).GetDesc());
                    }
                }
                //好物    
                else if (item.Type == CourseTypeEnum.Goodthing.ToInt() && !string.IsNullOrEmpty(item.GoodthingTypes))
                {
                    item.SubjectsOrGoodthingTypes = item.GoodthingTypes;
                    var list = JsonSerializationHelper.JSONToObject<List<int>>(item.SubjectsOrGoodthingTypes);
                    foreach (var e in list)
                    {
                        subOrGoodsNames.Add(((GoodthingCfyEnum)e).GetDesc());
                    }
                }
                item.SubjectsOrGoodthingTypes = string.Join(",", subOrGoodsNames);
               
            }
            var data = items.ToPagedList(request.PageSize,request.PageIndex, totalItemCount);
            
            return Task.FromResult(data);
        }
    }
}
