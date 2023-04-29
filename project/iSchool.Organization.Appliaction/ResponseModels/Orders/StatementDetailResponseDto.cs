using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Orders
{
    public class StatementDetailResponseDto
    {
        public string UserNick { get; set; }
        public string UserHeadImg { get; set; }
        public long PayTime { get; set; }
        public decimal? PayAmount { get; set; }
        public string OrderStatusDec { get; set; }

        public string CourseTitle { get; set; }
        public string CourseCover { get; set; }
        public string CourseProp { get; set; }
        public decimal? Bonus { get; set; }
    }
}
