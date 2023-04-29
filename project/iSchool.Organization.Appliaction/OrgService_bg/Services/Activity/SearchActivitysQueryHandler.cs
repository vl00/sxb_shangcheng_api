using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 查询机构列表
    /// </summary>
    public class SearchActivitysQueryHandler : IRequestHandler<SearchActivitysQuery, PagedList<ActivityItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public SearchActivitysQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<PagedList<ActivityItem>> Handle(SearchActivitysQuery request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters();
            dy.Add("@skipCount", (request.PageIndex-1)*request.PageSize);
            dy.Add("@pageSize", request.PageSize);
            string where ="";
            
            //活动名称
            if (!string.IsNullOrEmpty(request.Title))
            {
                where += $"   and act.title like '%{request.Title}%'  ";
            }
            ////关联专题
            //if (request.SpecialId!=null && request.SpecialId!=default)
            //{
            //    where += $"    and act.id in (select activityid from [dbo].[ActivityExtend] where contentid=@SpecialId and [type]={ActivityExtendType.Special.ToInt()} )  ";
            //    dy.Set("SpecialId", request.SpecialId);
            //}

            //关联专题(多选)
            if ( !string.IsNullOrEmpty(request.SpecialIds))
            {
                where += $"    and act.id in (select activityid from [dbo].[ActivityExtend] where contentid in ('{request.SpecialIds.Replace(",","','")}') and [type]={ActivityExtendType.Special.ToInt()} )  ";
                dy.Set("SpecialId", request.SpecialId);
            }

            //活动状态
            if (request.Status!=null && Enum.IsDefined(typeof(ActivityStatus),request.Status))
            {   
                if (request.Status == ActivityStatus.Ok.ToInt())//上架则增加附加条件--活动有效期内
                {
                    where += $"   and act.[status]=@status and act.endtime>'{DateTime.Now}' ";
                    dy.Set("status", request.Status);
                }
                else if (request.Status == ActivityStatus.Fail.ToInt())//下架增加多一种情况：状态是上架并且活动时间是过期的
                {
                    where += $" and ( act.[status]=@status or (act.[status]={ActivityStatus.Ok.ToInt()} and act.endtime<'{DateTime.Now}' ) ) ";
                    dy.Set("status", request.Status);
                }
            }
            string listSql = $@" 
                                select *,budget-ExpenditureAmount as RemainingAmount from (
                                select ROW_NUMBER() over(order by act.createtime desc) as [rownum], act.[id],act.[title]

                                ,(SELECT DISTINCT STUFF((SELECT  ',' +spe.title from dbo.ActivityExtend as  actExt 
                                left join dbo.Special as spe on actExt.contentid=spe.id and spe.IsValid=1
                                where spe.status={SpecialStatusEnum.Ok.ToInt()} and actExt.activityid=act.id and spe.IsValid=1
                                FOR XML PATH('')), 1, 1, '') )as [SpecialTitles]
                                
                                ,(select price  from dbo.ActivityRule   where [activityid]=act.id and [type]={ActivityRuleType.SingleBonus.ToInt()} and IsValid=1) as [price] 
                                
                                ,format(act.[starttime],'yyyy-MM-dd HH:mm') as [starttime],format(act.[endtime],'yyyy-MM-dd HH:mm') as [endtime]
                                
                                ,(select SUM(aemo.money) as expenditure from [dbo].[ActivityEvalMoneyOrder] as  aemo
                                 where aemo.[orderstatus]%3=0 and aemo.activityid=act.id) as ExpenditureAmount

                                ,act.[budget],act.[status]
                                
                                ,(select count(distinct evlt.id) from dbo.ActivityExtend as  actExt  
                                left join dbo.Special as spe on actExt.contentid=spe.id and spe.IsValid=1 
                                left join [dbo].[SpecialBind] speb on spe.id=speb.specialid and speb.IsValid=1
                                left join [dbo].[Evaluation] evlt on speb.evaluationid=evlt.id and evlt.IsValid=1 
                                where  spe.status={SpecialStatusEnum.Ok.ToInt()} and evlt.status={EvaluationStatusEnum.Ok.ToInt()} and actExt.activityid=act.id) as [ActEvltCount]
                                
                                ,act.acode
                                from dbo.Activity as act
                                where act.IsValid=1 and act.type={ActivityType.Hd2.ToInt()} {where}
                                order by rownum OFFSET @skipCount ROWS FETCH NEXT @pageSize ROWS ONLY
) as TT
                        ;";
            string countSql = $@" 
                               select count(1)
                               from dbo.Activity as act
                               where act.IsValid=1  and act.type={ActivityType.Hd2.ToInt()} {where}       
                             ;";
            var totalItemCount = _orgUnitOfWork.DbConnection.Query<int>(countSql,dy).FirstOrDefault();
            var data = _orgUnitOfWork.DbConnection.Query<ActivityItem>(listSql, dy).ToPagedList(request.PageSize,request.PageIndex, totalItemCount);
            var list = data.CurrentPageItems?.ToList();
            if (list?.Any() == true)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].ActivityUrl = request.ActivityUrl.FormatWith(list[i].ACode);
                }
            }            
            data.CurrentPageItems = list;
            return Task.FromResult(data);
        }
    }
}
