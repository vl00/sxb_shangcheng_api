using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

	[Obsolete("旧活动")]
    public class ActivitySimpleInfoDto
    {
		/// <summary>活动id</summary> 
		public Guid Id { get; set; }
		/// <summary>活动标题</summary> 
		public string Title { get; set; } = default!;
		public string? Logo { get; set; }
		/// <summary>开始时间</summary> 
		public DateTime? StartTime { get; set; }
		/// <summary>结束时间</summary> 
		public DateTime? EndTime { get; set; }
		/// <summary>类型</summary> 
		public int? Type { get; set; }
		/// <summary>描述</summary> 
		public string? Desc { get; set; }
		public Guid? Creator { get; set; }
		public DateTime? CreateTime { get; set; }
		/// <summary>专题id</summary> 
		public Guid? SpecialId { get; set; }
		/// <summary>专题名</summary> 
		public string? SpecialName { get; set; }
		/// <summary>专题短id</summary> 
		public string? SpecialId_s { get; set; }
		/// <summary>活动号</summary> 
		public string Acode { get; set; } = default!;

		/// <summary>
		/// 检测是否有效
		/// </summary>
		/// <param name="now">null为当前时间</param>
		/// <returns>
		/// -1=活动未开始<br/>
		/// 0=活动期间<br/>
		/// 1=活动过期
		/// </returns>
		public int CheckIfNotValid(DateTime? now = null)
		{
			now ??= DateTime.Now;
			if (StartTime != null && StartTime.Value > now.Value) return -1;
			if (EndTime != null && EndTime.Value <= now.Value) return 1;
			return 0;
		}
	}

	/// <summary>
	/// 用于前端显示
	/// </summary>
	[Obsolete("旧活动")]
	public class ActivityDataDto
	{ 
		/// <summary>活动id</summary> 
		public Guid Id { get; set; }
		/// <summary>活动标题</summary> 
		public string Title { get; set; } = default!;
		public string? Logo { get; set; }
		/// <summary>开始时间</summary> 
		public DateTime? StartTime { get; set; }
		/// <summary>结束时间</summary> 
		public DateTime? EndTime { get; set; }
		/// <summary>专题id</summary> 
		public Guid? SpecialId { get; set; }
		/// <summary>专题名</summary> 
		public string? SpecialName { get; set; }
		/// <summary>专题短id</summary> 
		public string? SpecialId_s { get; set; }
		/// <summary>活动号</summary> 
		public string Acode { get; set; } = default!;
		/// <summary>
		/// 检测是否有效<br/>
		/// -1=活动未开始<br/>
		/// 0=活动期间<br/>
		/// 1=活动过期
		/// </summary>
		public int Astatus { get; set; }
	}

#nullable disable
}
