using iSchool.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 根据分页信息,返回所有机构信息
    /// </summary>
    public class OrganizationAllResponse
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfoResult PageInfo { get; set; }

        /// <summary>
        /// 机构列表
        /// </summary>
        public List<OrganizationData> OrganizationDatas { get; set; }

        /// <summary>活动二维码</summary>
        public string Qrcode { get; set; }
    }

    /// <summary>
    /// 机构信息
    /// </summary>
    public class OrganizationData 
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid Id { get; set; }
                
        /// <summary>
        /// 机构短Id
        /// </summary>
        public string No { get; set; }       

        /// <summary>
        /// 机构名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 机构Logo
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 是否认证（true：认证；false：未认证）
        /// </summary>
        public bool Authentication { get; set; }
    }

}
