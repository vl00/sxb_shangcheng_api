using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Modles
{
    public class PVUV4Wechat
    {
        public GropyByData _id { get; set; }
        public int pv { get; set; }
        public int uv { get; set; }
    }
    public class GropyByData
    {
        public string courseid { get; set; }
        public string eid { get; set; }
        public string day { get; set; }
        public string surl { get; set; }

        public Guid EidUUid
        {
            get
            {
                var guid = Guid.Empty;
                Guid.TryParse(eid, out guid);
                return guid;
            }

        }
    }
}
