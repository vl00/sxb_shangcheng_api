using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Queries.Models
{
    public class OffsetRequest<TBody> where TBody:class
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        public TBody Body { get; set; }
    }
}
