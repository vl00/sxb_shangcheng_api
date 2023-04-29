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
    /// 某个大专题的小专题集合
    /// </summary>
    public class GetSpecialsQuery : IRequest<IEnumerable<SmallSpecialItem>>
    {      
        /// <summary>
        /// 大专题No
        /// </summary>
        public long No { get; set; }

        /// <summary>
        /// 大专题长Id
        /// </summary>
        public Guid SpecialId { get; set; }
    }
}
