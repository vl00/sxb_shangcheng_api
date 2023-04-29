using iSchool.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 
    /// </summary>
    public partial class UserSxbUnionIDDto
    {
		public Guid UserId { get; set; }

        public string UnionID { get; set; } = default!;

        public string NickName { get; set; } = default!;
    }

#nullable disable
}
