using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.RequestModels
{
    public class GetCourses4MeterialQuery : IRequest<GetCourses4MeterialQryResult>
    {
        public string Txt { get; set; }
    }

}
