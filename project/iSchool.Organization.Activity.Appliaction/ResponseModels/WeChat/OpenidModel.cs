using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Activity.Appliaction.ResponseModels.WeChat
{
    public class OpenidModel
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// openID
        /// </summary>
        public string OpenID { get; set; }
    }
}
