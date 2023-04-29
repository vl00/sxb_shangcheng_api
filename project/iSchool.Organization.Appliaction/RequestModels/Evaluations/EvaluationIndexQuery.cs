using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 评测主页
    /// </summary>
    public class EvaluationIndexQuery : IRequest<ResponseModels.EvaluationIndexQueryResult>
    {
        public string Subj { get; set; }
        public string Age { get; set; }
        public int Stick { get; set; }
    }

    public class EvaluationIndexQuery2 : IRequest<ResponseModels.EvaluationIndexQueryResult2>
    {
        public string Subj { get; set; }
        public string Age { get; set; }
        public int PageIndex { get; set; } = 1;
        public int Stick { get; set; }
    }
}
