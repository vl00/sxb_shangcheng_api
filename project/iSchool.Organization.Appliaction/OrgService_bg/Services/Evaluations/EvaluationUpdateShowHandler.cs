using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using MediatR;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 编辑评测页面展示
    /// </summary>
    public class EvaluationUpdateShowHandler : IRequestHandler<EvaluationUpdateShow, EvalUpdateShowDto>
    {

        OrgUnitOfWork _orgUnitOfWork;
        public EvaluationUpdateShowHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<EvalUpdateShowDto> Handle(EvaluationUpdateShow request, CancellationToken cancellationToken)
        {
            #region old
            //string sql1 = $@"  Select  e.id,evltb.id as evaluationBindId,evltb.courseid,speb.id as specialBindId,speb.specialid ,evltb.orgid
            //                  ,e.title as etitle,c.minage,c.maxage,e.stick
            //                  ,case when evltb.courseid is not null then 'true' else 'false' end  ExistingorCustom--true:已有课程；false:自定义课程
            //                  ,case when evltb.courseid is not null then c.duration else evltb.duration end  duration--上课时长
            //                  ,case when evltb.courseid is not null then c.mode else evltb.mode end  mode--上课方式
            //                  ,case when evltb.courseid is not null then c.subject else evltb.subject end  subject--科目分类
            //                  ,case when evltb.courseid is not null then c.age else evltb.age end  age--年龄段
            //                  ,e.mode as evlttype--评测类型：1:自由模式 2:专业模式；图片和正文再根据评测类型和评测Id获取  
            //                  ,evltb.coursename --自定义课程名称
            //                  from [dbo].[Evaluation] e 
            //                  --left join [dbo].[EvaluationItem] item on e.id=item.evaluationid and e.IsValid=1
            //                  --left join [dbo].[EvaluationComment] comment on e.id=comment.evaluationid 
            //                  left join [dbo].[EvaluationBind] evltb on  e.id=evltb.evaluationid and e.IsValid=1 and evltb.IsValid=1
            //                  left join [dbo].[Course] c on evltb.courseid=c.id   and c.IsValid=1 and evltb.IsValid=1
            //                  left join [dbo].[SpecialBind] speb on e.id=speb.evaluationid and e.IsValid=1 and speb.IsValid=1 
            //                  where e.IsValid=1 and e.id=@evaluationid ;"; 
            #endregion

            string sql1 = $@"  
Select  e.id,speb.id as specialBindId,speb.specialid ,e.title as etitle,e.stick
,e.mode as evlttype--评测类型：1:自由模式 2:专业模式；图片和正文再根据评测类型和评测Id获取
from [dbo].[Evaluation] e 
left join [dbo].[SpecialBind] speb on e.id=speb.evaluationid and e.IsValid=1 and speb.IsValid=1 
where e.IsValid=1 and e.id=@evaluationid ;";
            var dto = _orgUnitOfWork.DbConnection.Query<EvalUpdateShowDto>(sql1, new DynamicParameters().Set("evaluationid", request.Id)).FirstOrDefault();
            if (dto != null)
            {
                var dp = new DynamicParameters().Set("Id", request.Id);

                dto.ListEvaluationItems = _orgUnitOfWork.DbConnection.Query<EvaluationItem>(" select * from  [dbo].[EvaluationItem] where IsValid=1 and evaluationid=@Id  order by type",
                dp).ToList();

                dto.ListEvltBind = _orgUnitOfWork.DbConnection.Query<EvaluationBind>(" select * from  [dbo].[EvaluationBind] where IsValid=1 and evaluationid=@Id ",
                    dp).ToList();

                //dto.ListEvaluationComments = _orgUnitOfWork.DbConnection.Query<EvaluationComment>(" select * from  [dbo].[EvaluationComment] where IsValid=1 and evaluationid=@Id ",
                //    new DynamicParameters().Set("Id", request.Id)).ToList();
            }
            
            //
            //if(dto!=null && string.IsNullOrEmpty(dto?.Mode))
            //    dto.Mode = JsonSerializationHelper.Serialize(dto.Mode);

            return Task.FromResult(dto);
        }
    }
}
