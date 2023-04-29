using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 专题base info
    /// </summary>
    public class GetSpecialInfoQuery : IRequest<Special>
    {        
        public long No { get; set; }
        public Guid SpecialId { get; set; }
    }
}
