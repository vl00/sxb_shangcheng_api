using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using iSchool.Organization.Appliaction.ViewModels;
using MediatR;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 编辑评测页面展示
    /// </summary>
    public class EvaluationUpdateShow:IRequest<EvalUpdateShowDto>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid Id { get; set; }
    }
}
