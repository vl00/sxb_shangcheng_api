using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Mini.ResponseModels.Courses
{
    public class MiniCoursePointsGoods
    {


        public Guid Id { get; set; }

        public string NoStr { get; set; }

        public string Title { get; set; }

        public decimal Origprice { get; set; }

        public List<string> Banner { get; set; }

        public List<string> Banner_s { get; set; }

        public int Points { get; set; }

        public IEnumerable<string> Tags { get; set; }

    }
}
