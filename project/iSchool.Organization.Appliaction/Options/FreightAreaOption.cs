using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction
{
    /// <summary>
    /// 运费地区option <br/> 
    /// "AppSettings:FreightCityAreas"
    /// </summary>
    public class FreightAreaOption
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public string[] AreaNames { get; set; }
        public int[] AreaCodes { get; set; }
    }
}
