using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.ComponentModel;



namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 编辑评测
    /// </summary>
    public class UpdateEvaluationCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        [Description("Id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Description("Title")]
        public string Title { get; set; }

        /// <summary>
        /// 评测Item集合
        /// </summary>
        public List<EvltItem> ListEvltItems { get; set; }

        /// <summary>
        /// 专题
        /// </summary>
        [Description("Specialid")]
        public Guid? Specialid { get; set; }

        #region old
        ///// <summary>
        ///// 机构
        ///// </summary>
        //[Description("OrgId")]
        //public Guid? OrgId { get; set; }

        ///// <summary>
        ///// 课程
        ///// </summary>
        //[Description("CourseId")]
        //public Guid? CourseId { get; set; }

        //#region CourseId==null,自定义课程需要维护的字段，更新到评测绑定表

        ///// <summary>
        ///// 自定义课程名称
        ///// </summary>
        //public string CourseName { get; set; }

        ///// <summary>
        ///// 上课时长
        ///// </summary>
        //[Description("Duration")]
        //public int? Duration { get; set; }

        ///// <summary>
        ///// 上课方式(json格式)
        ///// </summary>
        //[Description("Mode")]
        //public string Mode { get; set; }

        ///// <summary>
        ///// 科目分类
        ///// </summary>
        //[Description("Subject")]
        //public int? Subject { get; set; }

        ///// <summary>
        ///// 年龄段
        ///// </summary>
        //[Description("Age")]
        //public int? Age { get; set; }      

        //#endregion 
        #endregion

        /// <summary>
        /// 机构(list集合)
        /// </summary>
        public List<Guid> ListOrgId { get; set; }

        /// <summary>
        /// 课程(list集合)
        /// </summary>
        public List<Guid> ListCourseId { get; set; }

        /// <summary>
        /// 是否加精
        /// </summary>
        public bool IsStick { get; set; } = false;

        /// <summary>
        /// 更新者
        /// </summary>
        public Guid? Modifier { get; set; }
    }

    /// <summary>
	/// 评测--内容项
	/// </summary>
    public  class EvltItem
    {

        /// <summary>
        /// 评测Item Id
        /// </summary> 
        public Guid Id { get; set; }      
     
        /// <summary>
        /// 评测内容
        /// </summary> 
        public string Content { get; set; }

        /// <summary>
        /// 原图集合
        /// </summary> 
        public string Pictures { get; set; }

        /// <summary>
        /// 缩略图集合
        /// </summary> 
        public string Thumbnails { get; set; }

        /// <summary>
		/// 视频
		/// </summary> 
		public string Video { get; set; }

        /// <summary>
        /// 视频封面
        /// </summary> 
        public string VideoCover { get; set; }


    }
}
