using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 评测添加到主题
    /// </summary>
    public class AddEvaluationToSpecialsCommand : IRequest<AddEvaluationToSpecialsCommandResult>
    {
        /// <summary>
        /// 评测id
        /// </summary>
        public Guid EvltId { get; set; }
        /// <summary>
        /// 专题id
        /// </summary>
        public Guid SpecialId { get; set; }
    }
}
