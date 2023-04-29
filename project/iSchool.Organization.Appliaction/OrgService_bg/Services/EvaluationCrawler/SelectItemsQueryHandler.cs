using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.EvaluationCrawler
{
    /// <summary>
    /// 抓取评测详情页-下拉框通用
    /// </summary>
    public class SelectItemsQueryHandler : IRequestHandler<SelectItemsQuery, List<SelectListItem>>
    {

        OrgUnitOfWork _orgUnitOfWork;
        public SelectItemsQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public async Task<List<SelectListItem>> Handle(SelectItemsQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string sql = "";
            List<SelectListItem> selectData = new List<SelectListItem>();

            #region 1-机构
            if (request.Type == 1)
            {
                if (!string.IsNullOrEmpty(request.OtherCondition) && request.OtherCondition.Contains("1-"))//按机构分类查询机构
                {
                    var i = Convert.ToInt32(request.OtherCondition.Split('-').Last());
                    var i1 = i > 100 && i < 200 ? i % 100 : i;

                    sql = $@"select Distinct try_convert(varchar(36),org.ID) as [Value],org.name as [Text]  from [dbo].[Organization] as org
                           left join (SELECT id, value AS type FROM [Organization] CROSS APPLY OPENJSON(types))TT on org.id=TT.id
                           left join (select id,value as type from [Organization] CROSS APPLY OPENJSON(subjects))t2 on org.id=t2.id
                           left join (select id,value as type from [Organization] CROSS APPLY OPENJSON(GoodthingTypes))t3 on org.id=t3.id
                           where org.IsValid=1 and org.status={(int)OrganizationStatusEnum.Ok} and (TT.type={i} or TT.type={i1} or t2.type={i} or t3.type={i}) 
                           order by Text ;";
                }
                else
                {
                    sql = $" select try_convert(varchar(36),ID) as [Value],name as [Text]  from [dbo].[Organization] where IsValid=1 and status={(int)OrganizationStatusEnum.Ok}   order by Text ;";

                }
                selectData = _orgUnitOfWork.DbConnection.Query<SelectListItem>(sql).ToList();
            }
            #endregion

            #region 2-所有小专题集合
            else if (request.Type == 2)
            {

                sql = $" select try_convert(varchar(36),ID) as [Value],title as [Text]  from [dbo].[Special] where IsValid=1  and status={(int)SpecialStatusEnum.Ok} and type={(int)SpecialTypeEnum.SmallSpecial}   order by sort ;";
                selectData = _orgUnitOfWork.DbConnection.Query<SelectListItem>(sql).ToList();
            }
            #endregion

            #region 3-课程,机构下的所有课程
            else if (request.Type == 3)
            {
                var dy = new DynamicParameters()
                .Set("Id", request.Id)//机构Id
                .Set("cstatus", (int)CourseStatusEnum.Ok)
                .Set("ostatus", (int)OrganizationStatusEnum.Ok)
                //.Set("type",CourseTypeEnum.Course.ToInt())
                ;

                sql = $@" select try_convert(varchar(36),c.ID) as [Value],c.title as [Text]   from [dbo].[Course] c left join [dbo].[Organization] o on c.orgid=o.id 
                          where c.IsValid=1 and c.status=@cstatus and o.IsValid=1 and o.status=@ostatus and o.id=@Id  order by Text  ;";
                selectData = _orgUnitOfWork.DbConnection.Query<SelectListItem>(sql, dy).ToList();
            }
            #endregion

            #region 4-指定大专题下的小专题
            else if (request.Type == 4 && request.BigSpecialId != default)
            {
                sql = @$"select try_convert(varchar(36),s.ID) as [Value],s.title as [Text],'true' as Selected  from [dbo].[SpecialSeries] ss 
                          left join [dbo].[Special] s on ss.smallspecial=s.id 
                          where ss.IsValid=1 and ss.bigspecial='{request.BigSpecialId}' and s.IsValid=1  and s.status={(int)SpecialStatusEnum.Ok} and s.type={(int)SpecialTypeEnum.SmallSpecial} order by s.sort ; ";
                selectData = _orgUnitOfWork.DbConnection.Query<SelectListItem>(sql).ToList();
            }
            #endregion

            #region 5-未绑定活动的小专题(只要ActivityExtend有专题的记录，都当作该专题绑定了活动，不管记录状态如何)
            else if (request.Type == 5)
            {
                string where = "";
                if (request.ActivityId != default)
                    where += $" and act.id<>'{request.ActivityId}' ";
                sql = @$" select try_convert(varchar(36),ID) as [Value],title as [Text]  from [dbo].[Special] 
                          where IsValid=1  and status={(int)SpecialStatusEnum.Ok} and type={(int)SpecialTypeEnum.SmallSpecial}  
                          and id not in (
                          select  actE.contentid from  [dbo].[Activity] act
                          left join [dbo].[ActivityExtend] actE  on actE.activityid=act.id and actE.[type]={ActivityExtendType.Special.ToInt()}                          
                          where actE.[type]={ActivityExtendType.Special.ToInt()}and act.IsValid=1 --and act.status={ActivityStatus.Ok.ToInt()} 
                          {where}
                          )
                          order by sort ;";
                selectData = _orgUnitOfWork.DbConnection.Query<SelectListItem>(sql).ToList();
            }
            #endregion

            #region 6--所有上架的大专题
            else if (request.Type == 6)
            {
                sql = $" select try_convert(varchar(36),ID) as [Value],title as [Text]  from [dbo].[Special] where IsValid=1  and status={SpecialStatusEnum.Ok.ToInt()} and type={SpecialTypeEnum.BigSpecial.ToInt()}   order by sort ;";
                selectData = _orgUnitOfWork.DbConnection.Query<SelectListItem>(sql).ToList();
            }
            #endregion

            #region 7-所有未绑定大专题的小专题集合,编辑则排查当前大专题
            else if (request.Type == 7)
            {
                string where = "";

                if (request.BigSpecialId != default)
                    where += $" and bigspecial<>'{request.BigSpecialId}' ";

                sql = $@" select try_convert(varchar(36),ID) as [Value],title as [Text]  from [dbo].[Special] as spe 
                      where spe.IsValid=1  
                      and spe.status={(int)SpecialStatusEnum.Ok} and spe.type={(int)SpecialTypeEnum.SmallSpecial}   
                      and id not in (SELECT smallspecial FROM [Organization].[dbo].[SpecialSeries] where IsValid=1 {where} )
                      order by spe.sort ;";
                selectData = _orgUnitOfWork.DbConnection.Query<SelectListItem>(sql).ToList();
            }
            #endregion

            #region 8-KeyValue表
            else if (request.Type == 8)
            {
                //课程科目分类type=1;好物分类type=14
                sql = $@" select [key]  as [Value],[name] as [Text]  from KeyValue where isvalid=1 and type=@type  ;";
                selectData = _orgUnitOfWork.DbConnection.Query<SelectListItem>(sql, new DynamicParameters().Set("type", Convert.ToInt32(request.OtherCondition))).ToList();
            }
            #endregion

            #region 9-供应商列表
            else if (request.Type == 9)
            {
                sql = "SELECT try_convert(varchar(36),ID) AS [Value],Name AS [Text] FROM  dbo.Supplier WHERE IsValid=1";
                selectData = _orgUnitOfWork.DbConnection.Query<SelectListItem>(sql).ToList();
            }
            #endregion

            #region 10-供应商的地址s
            else if (request.Type == 10)
            {
                if (request.SupplierId == default) throw new CustomResponseException("供应商id为空");

                sql = "select try_convert(varchar(36),ID) as [Value], ReturnAddress as [Text] from [dbo].[SupplierAddress] where IsVaild=1 and SupplierId=@SupplierId order by sort ";
                selectData = _orgUnitOfWork.Query<SelectListItem>(sql, new { request.SupplierId }).ToList();
            }
            #endregion 10-供应商的地址s 

            return selectData;
        }
    }
}
