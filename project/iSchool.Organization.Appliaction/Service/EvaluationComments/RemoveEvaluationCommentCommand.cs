using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.ComponentModel;



namespace iSchool.Organization.Appliaction.Service
{
    public class RemoveEvaluationCommentCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测评论Id
        /// </summary>
        [Description("Id")]
        public Guid Id { get; set; }

      
    }
}
