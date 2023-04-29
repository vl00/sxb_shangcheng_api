using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class MiniCourseSearchQuery : IRequest<List<MiniCourseItemDto>>
    {
        public List<Guid> Ids { get; set; }
    }
}
