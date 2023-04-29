using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Orders
{
    /// <summary>
    /// 微信退款返回实体
    /// </summary>
    public class RefundResult
    {        
        public bool ApplySucess { get; set; }
        public string AapplyDesc { get; set; }
    }
}
