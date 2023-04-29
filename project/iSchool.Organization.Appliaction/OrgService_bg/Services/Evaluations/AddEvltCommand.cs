using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.ComponentModel;



namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    public class AddEvltCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 缩略图片
        /// </summary>
        public string ThumUrl { get; set; }

        /// <summary>
        /// 专题
        /// </summary>
        public Guid? Specialid { get; set; }

        /// <summary>
        /// 机构(list集合)
        /// </summary>
        public List<Guid> ListOrgId { get; set; }

        /// <summary>
        /// 课程(list集合)
        /// </summary>
        public List<Guid> ListCourseId { get; set; }

        #region CourseId==null,自定义课程需要维护的字段，更新到评测绑定表

        ///// <summary>
        ///// 自定义课程名称
        ///// </summary>
        //public string CourseName { get; set; }

        ///// <summary>
        ///// 上课时长
        ///// </summary>
        //public int? Duration { get; set; }

        ///// <summary>
        ///// 上课方式(json格式)
        ///// </summary>
        //public string Mode { get; set; }

        ///// <summary>
        ///// 科目分类
        ///// </summary>
        //public int? Subject { get; set; }

        ///// <summary>
        ///// 年龄段
        ///// </summary>
        //public int? Age { get; set; }

        ///// <summary>
        ///// 课程周期
        ///// </summary>
        //[Description("Cycle")]
        //public string Cycle { get; set; }

        ///// <summary>
        ///// 课程价格
        ///// </summary>
        //[Description("Price")]
        //public decimal? Price { get; set; } 


        #endregion

        /// <summary>
        /// 评论  json格式
        /// </summary>
        public string Comments { get; set; }


        /// <summary>
        /// 是否加精
        /// </summary>
        public bool IsStick { get; set; } = false;

        /// <summary>
        /// 是否紧急发布，(true:紧急发布；默认false)
        /// </summary>
        public bool IsUrgent { get; set; } = false;

        /// <summary>
        /// 操作者
        /// </summary>
        public Guid? Creator { get; set; }
    }
}
