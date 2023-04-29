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

    /// <summary>
    /// 课程分销-推广奖励信息
    /// </summary>
    public class GetCourseDrpFxInfoQuery : IRequest<GetCourseDrpFxInfoDto>
    {        
        public Guid CourseId { get; set; }
    }

    /// <summary>
    /// spu分销(佣金)信息
    /// </summary>
    public class GetCourseFxSimpleInfoQuery : IRequest<CourseDrpInfo?>
    {
        public Guid CourseId { get; set; }
    }

    /// <summary>
    /// sku分销(佣金)信息
    /// </summary>
    public class GetSkuFxSimpleInfoQuery : IRequest<CourseGoodDrpInfo?>
    {
        public Guid SkuId { get; set; }
    }

#nullable disable
}
