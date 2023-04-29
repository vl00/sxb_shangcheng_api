using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class MiniRwInviteActivityCoursesQryResult
    {
        public IEnumerable<MiniRwInviteActivityCourseDto> Courses { get; set; } = default!;
    }

    public class MiniRwInviteActivityCourseDto
    {
        /// <summary>课程id</summary>
        public Guid Id { get; set; }
        /// <summary>课程短id</summary>
        public string Id_s { get; set; }
        /// <summary>
        /// 是否认证（true：认证；false：未认证）
        /// </summary>
        public bool Authentication => OrgInfo?.Authentication ?? false;
        /// <summary>1=网课 2=好物</summary>
        public string Type { get; set; }
        /// <summary>标题</summary>
        public string Title { get; set; }
        /// <summary>课程副标题</summary>
        public string Subtitle { get; set; }
        /// <summary>
        /// 课程banner图片地址
        /// </summary>
        public string Banner { get; set; }
        /// <summary>现在价格</summary>
        public decimal? Price { get; set; }
        /// <summary>原始价格</summary>
        public decimal? OrigPrice { get; set; }        

        /// <summary>品牌信息</summary>
        public PcOrgItemDto0 OrgInfo { get; set; } = default!;

        /// <summary>
		/// 1 = 被发展人购买资格------>付费机会制
        /// <br/>2 = 发展人购买资格积分-------->推广积分制
        /// <br/>3 = 平台积分制
		/// </summary> 
		public byte ExchangeType { get; set; }
        /// <summary>积分</summary>
        public int ExchangePoint { get; set; }
        /// <summary>开始时间. 可null</summary> 
		public DateTime? ExchangeStartTime { get; set; }
        /// <summary>结束时间. 可null</summary> 
        public DateTime? ExchangeEndTime { get; set; }
        /// <summary>关键词s</summary> 
        public string[] ExchangeKeywords { get; set; } = default!;
    }
}
