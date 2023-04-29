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

    public class CourseGoodsPropsSmTableQuery : IRequest<CourseGoodsPropsSmTableItemDto[]>
    {
        public Guid CourseId { get; set; }
    }    

#nullable disable
}
