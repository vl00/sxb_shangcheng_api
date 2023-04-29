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
    /// <summary>
    /// 素材库中 获取商品图片视频s
    /// </summary>
    public class GetCourseMedias4MeterialQuery : IRequest<GetCourseMedias4MeterialQyResult>
    {
        public Guid CourseId { get; set; }
    }
}
