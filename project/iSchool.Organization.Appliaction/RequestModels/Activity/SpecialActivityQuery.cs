using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 根据专题id查找对应的活动s<br/>
    /// 目前一个专题对应一个活动,一个活动对应多个主题
    /// </summary>
    public class SpecialActivityQuery : IRequest<HdDataInfoDto[]?>
    {
        /// <summary>专题id</summary>
        public Guid SpecialId { get; set; }
    }

#nullable disable
}
