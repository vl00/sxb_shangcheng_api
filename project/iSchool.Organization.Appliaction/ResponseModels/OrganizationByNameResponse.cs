using iSchool.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    

    /// <summary>
    /// 根据机构名称(品牌)，获取机构列表
    /// </summary>
    public class OrganizationByNameResponse
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfoResult PageInfo { get; set; }

        /// <summary>
        /// 机构列表
        /// </summary>
        public List<OrganizationDataOfName> OrganizationDatas { get; set; }
    }

    /// <summary>
    /// 机构信息
    /// </summary>
    public class OrganizationDataOfName
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid Id { get; set; }

        //private string no;
        /// <summary>
        /// 机构短Id
        /// </summary>
        public string No { get; set; }
        
        /// <summary>
        /// 机构名称
        /// </summary>
        public string Name { get; set; }

    }
}
