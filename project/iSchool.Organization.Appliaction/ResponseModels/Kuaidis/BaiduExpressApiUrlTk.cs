using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 百度快递单查询接口url tokenV2 BAIDUID
    /// </summary>
    public class BaiduExpressApiUrlTk
    {
        public string ApiUrl { get; set; } = default!;

        public string BaiduID { get; set; } = default!;

        public DateTime? Exp { get; set; }

        public BaiduExpressApiUrlTk() { }

        public BaiduExpressApiUrlTk(string apiUrl, string baiduID, DateTime? exp = null)
        {
            ApiUrl = apiUrl;
            BaiduID = baiduID;
            Exp = exp;
        }

        public DateTime? CreateTime { get; set; }
    }

#nullable disable
}
