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
	/// 新活动数据信息dto
	/// </summary>
    public class HdDataInfoDto
	{		
		public Activity? Data { get; set; }		

		/// <summary>活动id</summary> 		
		public Guid Id => Data?.Id ?? default;
		/// <summary>活动号</summary> 		
		public string? Acode => Data?.Acode;
		/// <summary>活动类型</summary> 		
		public ActivityType Type => (ActivityType)(Data?.Type ?? 0);

		/// <summary>resolved的推广编号</summary>		
		public string? PromoNo { get; set; }

		/// <summary>
		/// !! ActivityFrontStatus.DayLimited 需要后续根据用户每日发评测数判断
		/// </summary>
		/// <returns></returns>
		public ActivityFrontStatus GetFrStatus(DateTime? now = null) => GetFrStatus(this.Data, now);

		/// <summary>
		/// !! ActivityFrontStatus.DayLimited 需要后续根据用户每日发评测数判断
		/// </summary>
		/// <returns></returns>
		public static ActivityFrontStatus GetFrStatus(Activity? activity, DateTime? now = null)
		{
			if (activity == null) return ActivityFrontStatus.NotExsits;
			if (!activity.IsValid) return ActivityFrontStatus.Deleted;
			if (activity.Status == (byte)ActivityStatus.Fail) return ActivityFrontStatus.Fail;
			now ??= DateTime.Now;
			if (activity.Starttime != null && activity.Starttime.Value > now.Value) return ActivityFrontStatus.NotStarted;
			if (activity.Endtime != null && activity.Endtime.Value <= now.Value) return ActivityFrontStatus.Expired;
			return ActivityFrontStatus.Ok;
		}
	}
	
#nullable disable
}
