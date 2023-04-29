using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>
	/// 学位分析-政策文件类型
	/// </summary>
	public enum DgAySchPcyFileTypeEnum
	{
		/// <summary>
		/// 对口
		/// </summary>
		[Description("对口入学")]
		Cp = 1,
		/// <summary>
		/// 统筹
		/// </summary>
		[Description("统筹")]
		Ov = 2,
		/// <summary>
		/// 积分入学
		/// </summary>
		[Description("积分入学")]
		Jf = 3,

		/// <summary>
		/// 对口直升
		/// </summary>
		[Description("对口直升")]
		CpHeli = 4,
		/// <summary>
		/// 电脑派位
		/// </summary>
		[Description("电脑派位")]
		CpPcAssign = 5,
	}
}
