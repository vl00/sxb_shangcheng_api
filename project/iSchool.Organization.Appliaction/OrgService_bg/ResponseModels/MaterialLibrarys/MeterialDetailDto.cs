using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ResponseModels
{
    public class MeterialDetailDto
    {
		public Guid Id { get; set; }
		/// <summary>课程id</summary> 
		public Guid CourseId { get; set; }
		/// <summary>课程title</summary> 
		public string CourseTitle { get; set; }
		/// <summary>标题</summary> 
		public string Title { get; set; }
		/// <summary>内容</summary> 
		public string Content { get; set; }

		/// <summary>图片</summary> 
		public string[] Pictures { get; set; }
		/// <summary>缩略图</summary> 
		public string[] Thumbnails { get; set; }

		/// <summary>视频</summary> 
		public string Video { get; set; }

		/// <summary>视频封面</summary> 
		public string VideoCover { get; set; }

		/// <summary>
		/// 状态   0 下架  1上架
		/// </summary> 
		public byte Status { get; set; }

		/// <summary>下载数</summary> 
		public int DownloadTime { get; set; }


	}
}
