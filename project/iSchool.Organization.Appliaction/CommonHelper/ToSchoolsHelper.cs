using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    public class ToSchoolsHelper
    {
        /// <summary>
        /// 根据学校年级enum获取最小最大年龄
        /// </summary>
        public static (int MinAge, int MaxAge) GetAgesBySchoolGrade(iSchool.Domain.Enum.SchoolGrade? grade)
        {
            return grade switch
            {
                null => (0, 0),
                (iSchool.Domain.Enum.SchoolGrade)0 => (0, 0),
                iSchool.Domain.Enum.SchoolGrade.Kindergarten => (3, 6),
                iSchool.Domain.Enum.SchoolGrade.PrimarySchool => (6, 12),
                iSchool.Domain.Enum.SchoolGrade.JuniorMiddleSchool => (12, 15),
                iSchool.Domain.Enum.SchoolGrade.SeniorMiddleSchool => (0, 3), //(15, 18),
                _ => (3, 6),
            };
        }
    }
}
