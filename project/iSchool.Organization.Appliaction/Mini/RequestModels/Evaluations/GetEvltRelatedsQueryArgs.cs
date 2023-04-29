using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 获取种草关联主体s
    /// </summary>
    public class GetEvltRelatedsQueryArgs : IRequest<List<GetEvltRelatedsResItem>>
    {
        public Guid[] EvltIds { get; set; } = default!;

        public bool AllowIosNodisplay { get; set; } = true;
    }

#nullable disable
}
