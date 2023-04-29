using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
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

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 查询机构列表
    /// </summary>
    public class SearchSpecialsQueryHandler : IRequestHandler<SearchSpecialsQuery, PagedList<SpecialItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public SearchSpecialsQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<PagedList<SpecialItem>> Handle(SearchSpecialsQuery request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters();
            dy.Add("@skipCount", (request.PageIndex-1)*request.PageSize);
            dy.Add("@pageSize", request.PageSize);
            string where ="";
            //专题名称
            if (!string.IsNullOrEmpty(request.Title))
            {
                where += $"   and sp.title like '%{request.Title}%'  ";
            }
            //分享标题
            if (!string.IsNullOrEmpty(request.ShareTitle))
            {
                where += $"   and sp.sharetitle like '%{request.ShareTitle}%'  ";
            }
            //分享副标题
            if (!string.IsNullOrEmpty(request.ShareSubTitle))
            {
                where += $"   and sp.sharesubtitle like '%{request.ShareSubTitle}%'  ";
            }
            //专题状态
            if (request.Status != null)
            {
                if (Enum.IsDefined(typeof(SpecialStatusEnum), request.Status))
                {
                    where += $"   and sp.status=@status  ";
                    dy.Add("@status", (SpecialStatusEnum)request.Status);
                }
            }
            string listSql = $@" 
                                SELECT  ROW_NUMBER()over(order by sp.CreateTime desc,sp.sort) as rownum, sp.id,sp.title,evlt.evltcount,sp.status
                                ,sp.sharetitle,sp.sharesubtitle,banner,subtitle,sp.type,sp.no
                                FROM [dbo].[Special] as sp
                                left join 
                                (
                                SELECT count(distinct evlt.id) as evltcount ,spb.specialid FROM [dbo].[SpecialBind] as spb
                                left join [dbo].[Evaluation] as evlt on spb.evaluationid=evlt.id and  spb.IsValid=1 
                                where  evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} group by spb.specialid
                                union
                                SELECT count(distinct evlt.id) as evltcount ,ss.bigspecial as specialid FROM [dbo].[SpecialSeries] ss 								
								left join [dbo].[SpecialBind] as spb on ss.smallspecial=spb.specialid and spb.IsValid=1
                                left join [dbo].[Evaluation] as evlt on spb.evaluationid=evlt.id and  evlt.IsValid=1 
                                where  evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} and ss.IsValid=1  group by ss.bigspecial
                                )as evlt on sp.id=evlt.specialid and sp.IsValid=1
                                where sp.IsValid=1 {where}
                                order by rownum OFFSET @skipCount ROWS FETCH NEXT @pageSize ROWS ONLY
                        ;";
            string countSql = $@" 
                               SELECT  count(1)
                                FROM [dbo].[Special] as sp                            
                                where sp.IsValid=1 {where}       
                             ;";
            var totalItemCount = _orgUnitOfWork.DbConnection.Query<int>(countSql,dy).FirstOrDefault();
            var data = _orgUnitOfWork.DbConnection.Query<SpecialItem>(listSql, dy).ToPagedList(request.PageSize,request.PageIndex, totalItemCount);
            var list = data.CurrentPageItems.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].SpecialUrl = request.SpecialBaseUrl.FormatWith(UrlShortIdUtil.Long2Base32(list[i].No));
            }
            data.CurrentPageItems = list;
            return Task.FromResult(data);
        }
    }
}
