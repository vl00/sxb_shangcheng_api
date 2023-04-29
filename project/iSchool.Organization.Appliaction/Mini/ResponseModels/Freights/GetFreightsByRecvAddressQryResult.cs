using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class GetFreightsByRecvAddressQryResult
    {
        /// <summary>运费s</summary>
        public IEnumerable<OrgFreightDto> Freights { get; set; } = default!;
        /// <summary>不发货地区的skuid</summary>
        public IEnumerable<Guid>? BlacklistSkuIds { get; set; }
    }

    public class OrgFreightDto
    {
        /// <summary>品牌id</summary>
        [Obsolete]
        public Guid OrgId { get; set; }
        /// <summary>运费</summary>
        public decimal Freight { get; set; }
        /// <summary>供应商id</summary> 
		public Guid SupplierId { get; set; }

        /// <summary>spu id</summary>
        public Guid? CourseId { get; set; }
        public Guid? Cfid { get; set; }
    }

#nullable disable
}
