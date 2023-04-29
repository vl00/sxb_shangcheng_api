using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class MiniOrganizationItemDto
    {
        /// <summary>
        /// id
        /// </summary>
        public Guid Id { get; set; }


        public string Id_s { get; set; }
        /// <summary>
        /// Logo
        /// </summary>

        public string Logo { get; set; }


        public string Name { get; set; }

        /// <summary>
        /// 是否认证
        /// </summary>
        public bool Authentication { get; set; }



    }
}
