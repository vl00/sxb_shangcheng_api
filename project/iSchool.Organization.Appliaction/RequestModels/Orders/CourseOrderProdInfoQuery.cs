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

    public class CourseGoodsPropsQuery : IRequest<CourseGoodsPropsDto>
    {
        public Guid CourseId { get; set; }
        public long CourseNo { get; set; }

        public bool IsFromPoints { get; set; }
    }    

#nullable disable
}
