using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Lables;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 评测卡片业务
    /// </summary>
    public class EvalLableByUrlQueryHandler : IRequestHandler<EvalLableByUrlQuery, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;

        public EvalLableByUrlQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork; 
        }
        
        public async Task<ResponseResult> Handle(EvalLableByUrlQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string id_s = request.EvalDetailUrl.Substring(request.EvalDetailUrl.LastIndexOf("/")+1);
            var no = UrlShortIdUtil.Base322Long(id_s) ;
            var evalSql = $" select * from Evaluation where IsValid=1  and no={no} and status={EvaluationStatusEnum.Ok.ToInt()} ";
            var eval = _orgUnitOfWork.QueryFirstOrDefault<Evaluation>(evalSql);
            if (eval == null)
            {
                return ResponseResult.Failed("评测不存在");
            }
            var detailSql = $" select content from [dbo].[EvaluationItem] where IsValid=1 and evaluationid='{eval.Id}' ";
            var listcontent = _orgUnitOfWork.Query<string>(detailSql)?.ToList();

            var response = new EvalDetailsLable();
            response.Id = eval.Id;
            response.Id_s = id_s;
            response.Title = eval.Title;
            response.Content = string.Join("", listcontent);
            response.CoverUrl = eval.Cover;
            return ResponseResult.Success(response);
        }

    }


    


}
