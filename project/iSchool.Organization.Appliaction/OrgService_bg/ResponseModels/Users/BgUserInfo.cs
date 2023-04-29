using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ResponseModels
{
    public class BgUserInfo
    {
        /// <summary>用户id</summary>
        public Guid Id { get; set; }
        /// <summary>用户名</summary>
        public string Displayname { get; set; }
    }
}
