using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 分销活动回复微信公众号图片
    /// </summary>
    public class HdDrpFxReplyGzhPicCommand : IRequest<bool>
    {
        public string OpenId { get; set; } = default!;

        public int PicIndex { get; set; }
    }

#nullable disable
}
