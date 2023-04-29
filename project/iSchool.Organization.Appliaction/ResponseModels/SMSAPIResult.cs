using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 短信返回实体类
    /// </summary>
    public class SMSAPIResult
    {
        public Sendstatu[] sendStatus { get; set; }
        public int statu { get; set; }
        public string message { get; set; }
        public class Sendstatu
        {
            public string phoneNumber { get; set; }
            public string message { get; set; }
            public string code { get; set; }
        }
    }
}
