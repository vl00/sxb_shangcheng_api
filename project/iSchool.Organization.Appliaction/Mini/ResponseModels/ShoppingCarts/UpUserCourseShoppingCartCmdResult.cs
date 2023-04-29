using iSchool.Organization.Appliaction.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class UpUserCourseShoppingCartCmdResult
    {
        public int Count { get; set; }

        ///// <summary>
        ///// 返回新增的项.多操作时请注意时间戳是不同的.
        ///// </summary>
        //public List<CourseShoppingCartItem> Addeds { get; set; } = new List<CourseShoppingCartItem>();

        /// <summary>返回删除的项</summary>
        public List<Guid> Deleteds { get; set; } = new List<Guid>();
    }

#nullable disable
}
