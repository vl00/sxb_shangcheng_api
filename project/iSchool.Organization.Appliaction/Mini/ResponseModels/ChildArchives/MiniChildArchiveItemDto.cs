using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{

    /// <summary>
    /// 孩子档案
    /// </summary>
    public class MiniChildArchiveItemDto
    {
        /// <summary>
        /// id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string HeadImg { get; set; }

        /// <summary>
        /// 性别 男：1  女：0
        /// </summary>
        public int Sex { get; set; }


        /// <summary>
        /// 别名
        /// </summary>
        public string NikeName { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>

        public DateTime? BirthDate { get; set; }


        /// <summary>
        /// 孩子年龄--单位月
        /// </summary>
        public int ChildrenAge { get; set; }

        /// <summary>
        /// 科目
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Subjs { get; set; } = default!;
        /// <summary>
        /// 用户ID
        /// </summary>

        public Guid UserId { get; set; }
    }
}
