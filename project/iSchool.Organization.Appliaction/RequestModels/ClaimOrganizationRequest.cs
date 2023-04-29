using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 认领机构请求实体
    /// </summary>
    public class ClaimOrganizationRequest:IRequest<ResponseResult>
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid Orgid { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// 操作人Id
        /// </summary>
        public Guid? Creator { get; set; }

    }
}
