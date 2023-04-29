using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{
	
	/// <summary>
	/// 释放订单占用的兑换码
	/// </summary>
	public class ReleaseOccupiedCodeCommand : IRequest<ResponseResult>
	{
		/// <summary>
		/// 课程id
		/// </summary> 
		public Guid CourseId { get; set; }
	}
}
