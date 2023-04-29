using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    public class ClearEvltCachesCommand: IRequest
    {
        /// <summary>评测id</summary>
        public Guid Id { get; set; } = default!;
        /// <summary>
        /// 编辑类型(1:新增；2:修改；3:官赞修改；4:不影响评论的修改)       
        /// </summary>
        public int Type { get; set; }
           
    }
}
