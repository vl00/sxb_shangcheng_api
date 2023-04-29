using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Lables;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 【短Id集合】评测卡片业务
    /// </summary>
    public class EvaltsLableById_ssQueryHandler : IRequestHandler<EvaltsLableById_ssQuery, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;

        public EvaltsLableById_ssQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork; 
        }
        
        public async Task<ResponseResult> Handle(EvaltsLableById_ssQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string where = "";
            if (request.Id_ss.Any() == false)
            {
                return ResponseResult.Failed("Id集不允许为空集");
            }
            else
            {
                var nos =request.Id_ss.Select(_ => UrlShortIdUtil.Base322Long(_));
                where += $"  and no in ({string.Join(",",nos)})  ";
            }
            
            var evalSql = $" select * from Evaluation where IsValid=1  and status={EvaluationStatusEnum.Ok.ToInt()} {where}";
            var evalts = _orgUnitOfWork.Query<Evaluation>(evalSql);
            if (evalts.Any() == false)
            {
                return ResponseResult.Success("暂无符合条件的数据");
            }
            var list = new List<EvalDetailsLable>();
            foreach (var eval in evalts)
            {
                var detailSql = $" select content from [dbo].[EvaluationItem] where IsValid=1 and evaluationid='{eval.Id}' ";
                var listcontent = _orgUnitOfWork.Query<string>(detailSql)?.ToList();
                var response = new EvalDetailsLable();
                response.Id = eval.Id;
                response.Id_s = UrlShortIdUtil.Long2Base32(eval.No);
                response.Title = eval.Title;
                response.Content = string.Join("", listcontent);
                response.CoverUrl = eval.Cover;
                list.Add(response);
            } 
            return ResponseResult.Success(list);
        }

    }


    


}
