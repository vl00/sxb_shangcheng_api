using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class MiniChildArchivesQuery : IRequest<List<MiniChildArchiveItemDto>>
    {

    }
}
