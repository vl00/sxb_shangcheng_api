using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Lables
{
    /// <summary>
    /// 机构卡片实体
    /// </summary>
    public class OrgLable
    {
        /// <summary>
        /// 机构长Id
        /// </summary>
        public Guid Id { get; set; }

        public long No { get; set; }

        /// <summary>
        /// 机构短Id
        /// </summary>
        public string Id_s { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 机构Logo
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 机构副标题1
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 机构副标题2
        /// </summary>
        public string Subdesc { get; set; }

        /// <summary>
        /// 机构的评测数
        /// </summary>
        public int EvalCount { get; set; }

        /// <summary>
        /// 机构的课程数
        /// </summary>
        public int CourseCount { get; set; }

    }
    /// <summary>
    /// 机构卡片返回实体
    /// </summary>
    public class OrgsLablesResponse
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfoResult PageInfo { get; set; }

        /// <summary>
        /// 机构卡片列表
        /// </summary>
        public List<OrgLable> ListLables { get; set; }
    }

}
