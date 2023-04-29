using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction
{
    /// <summary>
    /// 
    /// </summary>
    public class BussTknOption
    {
        public string Key { get; set; }
        public string Alg { get; set; } = SecurityAlgorithms.HmacSha256;
        public double? Exp { get; set; }
    }
}
