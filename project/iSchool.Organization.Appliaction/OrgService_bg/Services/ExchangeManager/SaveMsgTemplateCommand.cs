using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{
    /// <summary>
    /// 保存课程短信模板
    /// </summary>
    public class SaveMsgTemplateCommand: IRequest<ResponseResult>
    {
		/// <summary>
		/// 主键(编辑时需要)
		/// </summary> 
		public Guid? Id { get; set; }

		/// <summary>
		/// 变量1
		/// </summary> 
		public string Variable1 { get; set; }

		/// <summary>
		/// 变量2
		/// </summary> 
		public string Variable2 { get; set; }

		/// <summary>
		/// 兑换链接
		/// </summary> 
		public string Url { get; set; }

		/// <summary>
		/// 提示框内容
		/// </summary> 
		public string Msg { get; set; }

		/// <summary>
		/// 课程id
		/// </summary> 
		public Guid CourseId { get; set; }

		/// <summary>
		/// 商品id
		/// </summary> 
		public Guid? GoodId { get; set; }

		/// <summary>
		/// 是否自动发送兑换码
		/// </summary> 
		public bool IsAuto { get; set; }

		/// <summary>
		/// 跳转兑换链接
		/// </summary> 
		public bool IsRedirect { get; set; }

		/// <summary>
		/// 模板的内容
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// 运营商短信模板Id
		/// </summary>
		public string Code { get; set; }
	}
}
