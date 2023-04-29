using iSchool.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    public class WxPayResult
    {
        public string AppId { get; set; }
        public string TimeStamp { get; set; }
        public string NonceStr { get; set; }
        public string SignType { get; set; }
        public string PaySign { get; set; }
        public string Package { get; set; }
        public string H5_url { get; set; }
    }
}
