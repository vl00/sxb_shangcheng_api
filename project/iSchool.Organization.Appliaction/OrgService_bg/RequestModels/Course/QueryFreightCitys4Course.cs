using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.RequestModels
{
#nullable enable

    public class QueryFreightCitys4Course : IRequest<(int Code, string Name)[]>
    {        
    }

#nullable disable
}
