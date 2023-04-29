using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ViewModels;
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

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 查询机构列表
    /// </summary>
    public class SearchOrgListQueryHandler : IRequestHandler<SearchOrgListQuery, OrgListDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public SearchOrgListQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<OrgListDto> Handle(SearchOrgListQuery request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters();
            dy.Add("@SkipCount", (request.PageIndex-1)*request.PageSize);

            string where ="";

            //年龄段
            if (request.AgeGroup != null)
            {
                if(Enum.IsDefined(typeof(AgeGroup), request.AgeGroup))
                {
                    var ages_str= EnumUtil.GetDesc((AgeGroup)request.AgeGroup).Split('-');
                    var minAge = Convert.ToInt32(ages_str[0]);
                    var maxAge = Convert.ToInt32(ages_str[1]);
                    where += @$" and (
                                        (o.minage>=@minAge and o.maxage<=@maxAge)
                                        or (o.minage<=@minAge and o.maxage>=@minage)
                                        or (o.minage<=@maxage and o.maxage>=@maxage)
                                 ) and o.maxage>0  ";
                    dy.Add("@minAge", minAge);
                    dy.Add("@maxAge", maxAge);
                }
            }
            //教学模式/上课方式
            if (request.TeachMode != null)
            {
                if (Enum.IsDefined(typeof(TeachModeEnum), request.TeachMode))
                {
                    where += @"  and MM.mode=@Mode  ";
                    dy.Add("@Mode", request.TeachMode);
                }
            }

            //是否合作
            if (request.Authentication != null)
            {
                where += @"   and o.authentication=@Authentication  ";
                dy.Add("@Authentication", request.Authentication);
            }

            //机构名称模糊查询
            if (!string.IsNullOrEmpty(request.Name))
            {
                where += $@"   and o.Name like '%{request.Name}%' ";
            }

            dy.Add("@coursestatus", CourseStatusEnum.Ok);
            dy.Add("@type", CourseTypeEnum.Course.ToInt());
            string sql = $@" 
                            select top {request.PageSize} * from 
                            (
                                 select ROW_NUMBER() over(order by CreateTime desc) as rownum,* from 
                                 (
                                    select distinct o.name as orgname,o.id as orgId,o.no as Id_s
                                    ,o.authentication,o.types ,o.GoodthingTypes,o.logo
                                    ,o.ages as AgeRange,o.minage,o.maxage
                                    ,o.modes as Mode,o.CreateTime,o.status
                                    ,(select count(1) from [dbo].[Course] where orgid=o.id and IsValid=1 and status=@coursestatus and type=@type) as CourseCount
                                    from   [dbo].[Organization] o 
                                    left join [dbo].[Course] c on o.id=c.orgid and c.IsValid=1
                                    --left join (SELECT id, value AS age FROM [Organization]CROSS APPLY OPENJSON(ages)) AA on o.id=AA.id
                                    left join (SELECT id, value AS mode FROM [Organization]CROSS APPLY OPENJSON(modes)) MM on o.id=MM.id
                                    where o.IsValid=1 {where}
                                )t1
                            )TT
                            Where rownum>@SkipCount order by rownum 
                        ;";
            string pageSql = $@" 
                               select COUNT(1) AS pagecount,{request.PageIndex} AS PageIndex,{ request.PageSize} AS PageSize from 
							   (
									select  distinct o.id
									from   [dbo].[Organization] o 
									left join [dbo].[Course] c on o.id=c.orgid and c.IsValid=1
									--left join (SELECT id, value AS age FROM [Organization]CROSS APPLY OPENJSON(ages)) AA on o.id=AA.id
									left join (SELECT id, value AS mode FROM [Organization]CROSS APPLY OPENJSON(modes)) MM on o.id=MM.id
									where o.IsValid=1   {where}
								)T                                
                             ;";
            var data = _orgUnitOfWork.DbConnection.Query<OrgListDto>(pageSql, dy).FirstOrDefault();            
            data.list = new List<OrgItem>();
            data.list = _orgUnitOfWork.DbConnection.Query<OrgItem>(sql, dy).ToList();
            foreach (var item in data.list)
            {
                var orgTypes = new List<string>();
                if (!string.IsNullOrEmpty(item.GoodthingTypes))//好物分类
                {
                    var types = JsonSerializationHelper.JSONToObject<List<int>>(item.GoodthingTypes);
                    foreach (var t in types)
                    {
                        orgTypes.Add(((GoodthingCfyEnum)t).GetDesc());
                    }

                }
                if (!string.IsNullOrEmpty(item.Types))//品牌分类
                {
                    var types = JsonSerializationHelper.JSONToObject<List<int>>(item.Types);
                    foreach (var t in types)
                    {
                        orgTypes.Add(((OrgCfyEnum)t).GetDesc());
                    }
                    
                }

                item.Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(item.Id_s));

                item.OrgType = string.Join(",", orgTypes);
            }
            return Task.FromResult(data);
        }
    }
}
