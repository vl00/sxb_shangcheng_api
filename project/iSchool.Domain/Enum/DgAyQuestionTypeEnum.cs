using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>
	/// 学位分析器-题型
	/// </summary>
	public enum DgAyQuestionTypeEnum
	{
		/// <summary>
		/// 单选
		/// </summary>
		[Description("单选")]
		Ty1 = 1,

		/// <summary>
		/// 多选
		/// </summary>
		[Description("多选")]
		Ty2 = 2,

		/// <summary>
		/// 单选计分
		/// </summary>
		[Description("单选计分")]
		Ty3 = 3,

		/// <summary>
		/// 下拉计分
		/// </summary>
		[Description("下拉计分")]
		Ty4 = 4,

		/// <summary>
		/// 地址选择
		/// </summary>
		[Description("地址选择")]
		Ty5 = 5,

		/// <summary>
		/// 地图定位
		/// </summary>
		[Description("地图定位")]
		Ty6 = 6,

		/// <summary>
		/// 地区单选
		/// </summary>
		[Description("地区单选")]
		Ty7 = 7,
	}
}
