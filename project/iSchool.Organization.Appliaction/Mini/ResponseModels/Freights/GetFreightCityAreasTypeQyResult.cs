using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class GetFreightCityAreasTypeQyResult
    {
        public string Name { get; set; } = default!;
        public int Code { get; set; } = -1;

        public FreightAreaTypeEnum Ty { get; set; } = (FreightAreaTypeEnum)(-1);
    }

#nullable disable
}
