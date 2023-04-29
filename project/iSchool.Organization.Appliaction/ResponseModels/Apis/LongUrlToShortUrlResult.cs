using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Apis
{

    public class LongUrlToShortUrlResult
    {
        public string data { get; set; }
        public bool success { get; set; }
        public int status { get; set; }
        public string erroMsg { get; set; }
    }

}
