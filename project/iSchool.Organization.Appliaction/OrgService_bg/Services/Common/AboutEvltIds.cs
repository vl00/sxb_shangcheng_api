using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Common
{
    /// <summary>
    /// 评测相关Id(专题、机构、课程)
    /// </summary>
    public class AboutEvltIds
    {

        /// <summary>
        /// 专题Id
        /// </summary>
        public Guid? SpecialId { get; set; }

        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid? OrgId { get; set; }

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid? CourseId { get; set; }

    }
}
