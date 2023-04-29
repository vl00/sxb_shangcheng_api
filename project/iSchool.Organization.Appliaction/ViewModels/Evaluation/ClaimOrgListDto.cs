using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 认领机构列表
    /// </summary>
    public class ClaimOrgListDto
    {
        public List<ClaimOrgItem> list { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }

    public class ClaimOrgItem
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int RowNum { get; set; }

        public Guid Id { get; set; }

        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrgId { get; set; }

        /// <summary>
        /// 机构
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string Position { get; set; }
      
        /// <summary>
        /// 认领状态
        /// </summary>
        public int? Status { get; set; }


    }
}
