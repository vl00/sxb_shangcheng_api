using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.ComponentModel;



namespace iSchool.Organization.Appliaction.Service
{
    public class RemoveEvaluationCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        [Description("Id")]
        public Guid Id { get; set; }

      
    }
}
