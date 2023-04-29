using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.EvaluationCrawler
{
    /// <summary>
    /// 抓取评测详情请求实体
    /// </summary>
    public class CaptureEvaluationDetailsQuery:IRequest<CaptureEvaluationDto>
    {
        public Guid Id { get; set; }
    }
}
