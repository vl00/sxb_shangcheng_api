using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{


    public class MiniChildArchiveListQuery : IRequest<List<MiniChildArchiveItemDto>>
    {
        public List<Guid> UserIds { get; set; }
    }
}
