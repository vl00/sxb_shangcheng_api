using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class MiniDelEvaluationCommand : IRequest<bool>
    {
        /// <summary>
        /// 评测id
        /// </summary>
        public Guid Id { get; set; }

    }
}
