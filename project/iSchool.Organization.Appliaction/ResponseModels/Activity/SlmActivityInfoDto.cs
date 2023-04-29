using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

	/// <summary>
	/// 用于前端显示
	/// </summary>
	public class SlmActivityInfoDto
	{
		[JsonIgnore]
		public Activity? Data { get; set; }

		/// <summary>活动id</summary> 		
		public Guid? Id => Data?.Id;
		/// <summary>活动号</summary> 		
		public string? Acode => Data?.Acode;

		/// <summary>resolved的推广编号</summary>		
		public string? PromoNo { get; set; }

		private int? _status;
		/// <summary>
		/// 根据此值判断活动状态.<br/>
		/// ```
		/// <summary>正常|审核成功|上架|期间</summary>
		/// [Description("正常")]
		/// Ok = 1,
		/// /// <summary>失败|审核失败|下架</summary>
		/// [Description("已下架")]
		/// Fail = 2,
		/// /// <summary>未开始</summary>
		/// [Description("未开始")]
		/// NotStarted = 3,
		/// /// <summary>已过期</summary>
		/// [Description("已过期")]
		/// Expired = 4,
		/// /// <summary>不存在</summary>
		/// [Description("不存在")]
		/// NotExsits = 5,
		/// /// <summary>已删除</summary>
		/// [Description("已删除")]
		/// Deleted = 6,
		/// /// <summary>到达每日上限</summary>
		/// [Description("到达每日上限")]
		/// DayLimited = 7,
		/// ```
		/// </summary> 
		public int Status
		{
			get => _status ??= (int)HdDataInfoDto.GetFrStatus(Data);
			set => _status = value;
		}

		/// <summary>活动标题</summary> 
		public string? Title => Data?.Title;
		/// <summary></summary> 
		public string? Logo => Data?.Logo;
		/// <summary>开始时间</summary> 
		public DateTime? Starttime => Data?.Starttime;
		/// <summary>结束时间</summary> 
		public DateTime? Endtime => Data?.Endtime;
		/// <summary>描述</summary> 
		public string? Desc => Data?.Desc;

		/// <summary>?当活动不是正常时,需要在前端显示的二维码</summary>
		public string? Qrcode { get; set; }
		/// <summary>用于到达上限后需要显示的评测数</summary> 
		public int? EvltCount { get; set; }

		/// <summary>
		/// 账号状态<br/> 
		/// ```
		/// /// <summary>账号正常</summary>
		/// [Description("账号正常")]
		/// Normal = 0,
		/// /// <summary>手机号异常</summary>
		/// [Description("手机号异常")]
		/// MobileExcp = 1,
		/// ```
		/// </summary>
		public int Ustatus { get; set; }
	} 

#nullable disable
}
