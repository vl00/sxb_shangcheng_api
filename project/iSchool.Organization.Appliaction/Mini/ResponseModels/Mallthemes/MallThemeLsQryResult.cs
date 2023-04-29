using iSchool.Infrastructure.Dapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

	public class MallThemeLsQryResult
	{
		public PagedList<MallThemeDto> Pages { get; set; } = default!;
	}

    /// <summary>
    /// 商城主题dto
    /// </summary>
    public class MallThemeDto
    {
		/// <summary>主题id</summary> 
		public Guid Id { get; set; }
		/// <summary>主题短id</summary> 
		public string Id_s { get; set; } = default!;

		/// <summary>主题名称</summary> 
		public string Name { get; set; } = default!;
		/// <summary>主题logo</summary> 
		public string Logo { get; set; } = default!;

		/// <summary>m列表图片</summary> 
		public string? MListPicture { get; set; }
		/// <summary>pc列表图片</summary> 
		public string? PcListPicture { get; set; }

		/// <summary>开始时间</summary> 
		public DateTime? StartTime { get; set; }
		/// <summary>结束时间</summary> 
		public DateTime? EndTime { get; set; }

		/// <summary>是否本期</summary> 
		public bool IsCurrent { get; set; }
		/// <summary>全局排序no</summary> 
		public long No { get; set; } = 0;

		/// <summary>开始时间描述</summary>
		public string? StartTimeDesc
		{
			get
			{
				if (StartTime == null) return null;
				var str = StartTime.Value.ToString("MMM", CultureInfo.CreateSpecificCulture("en-GB"));
				str = str.Length <= 3 ? str : str[..3];
				str = $"{StartTime.Value.Day}.{str}";
				return str;
			}
		}


	}

#nullable disable
}
