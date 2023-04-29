using iSchool.Organization.Appliaction.Mini.ResponseModels.Courses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Mini.RequestModels.Courses
{
   public class MiniCoursePointsGoodsQuery:IRequest<IEnumerable<MiniCoursePointsGoods>>
    {
        public int Offset { get; set; } = 0;

        public int Limit { get; set; } = 20;
    }
}
