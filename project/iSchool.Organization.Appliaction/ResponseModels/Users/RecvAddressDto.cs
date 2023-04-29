using iSchool.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 收货地址dto
    /// </summary>
    public partial class RecvAddressDto
    {
		/// <summary>
		/// 地址信息id.<br/>
		/// 当文档出现作为body参数传入api时,如果没有请去掉此参数或传null.
		/// </summary>
		public Guid? Id { get; set; }

		/// <summary>收货人名字</summary> 
		public string RecvUsername { get; set; } = default!;
		/// <summary>收货人手机</summary> 
		public string RecvMobile { get; set; } = default!;
		/// <summary>收货地址</summary> 
		public string Address { get; set; } = default!;

		private bool? _IsDefault;
		/// <summary>是否默认</summary> 
		public bool? IsDefault
		{
			get => _IsDefault ?? false;
			set => _IsDefault = value;
		}

		/// <summary>邮编</summary> 
		public string? Postalcode { get; set; }
		/// <summary>省</summary> 
		public string? Province { get; set; }
		/// <summary>市</summary> 
		public string? City { get; set; }
		/// <summary>区</summary> 
		public string? Area { get; set; }

	}

#nullable disable
}
