using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    /// <summary>
    /// 更新科目
    /// </summary>
    public class UpdateSubjectCommand:IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvltId { get; set; }

        public int Subject { get; set; }
    }
}
