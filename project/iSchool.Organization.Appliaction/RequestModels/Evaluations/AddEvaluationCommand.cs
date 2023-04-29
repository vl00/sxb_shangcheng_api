#nullable enable

using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 添加评测. 机构和课程可选可填
    /// </summary>
    public class AddEvaluationCommand : IRequest<EvaluationAddedResult>
    {
        /// <summary>活动(推广)码</summary>
        public string? Promocode { get; set; }

        /// <summary>
        /// 评测ID
        /// </summary>
        public Guid? EvaluationId { get; set; }
        /// <summary>
        /// 专题id.可null.如果由专题页进入添加评测则要带
        /// </summary>
        public Guid? SpecialId { get; set; }

        /// <summary>
        /// 选择的机构id.如果机构是自定义则为null.
        /// </summary>
        public Guid? OrgId { get; set; }
        /// <summary>
        /// 自定义的机构名称.如果机构是自定义则不为null.
        /// </summary>
        public string? OrgName { get; set; }
        /// <summary>
        /// 选择或自定义的课程.没数据为空对象
        /// </summary>
        public AddEvaluationCommand_CourseEntity Course { get; set; } = default!;
        /// <summary>
        ///  1=自由模式 2=专业模式<br/>
        ///  各个模式是互斥的
        /// </summary>
        public int Mode { get; set; }
        /// <summary>
        /// 评测内容.自由模式不为null,其他模式为null
        /// </summary>
        public EvltContent1? Ctt1 { get; set; }
        /// <summary>
        /// 评测内容.专业模式不为null,其他模式为null
        /// </summary>
        public EvltContent2? Ctt2 { get; set; }
    }

    /// <summary>
    /// 里面字段可为null
    /// </summary>
    public class AddEvaluationCommand_CourseEntity
    {
        /// <summary>
        /// 选择的课程id,如果课程是自定义的为null
        /// </summary>
        public Guid? CourseId { get; set; }
        /// <summary>
        /// 自定义的课程名称
        /// </summary>
        public string? CourseName { get; set; }
        /// <summary>
        /// 自定义的课程--科目
        /// </summary> 
        public int? Subject { get; set; }
        /// <summary>
        /// 自定义的课程--年龄段
        /// </summary> 
        public byte? Age { get; set; }
        /// <summary>
        /// 自定义的课程--上课方式(可多选)
        /// </summary> 
        public int[]? Mode { get; set; }
        /// <summary>
        /// 自定义的课程--上课时长
        /// </summary>
        public int? Duration { get; set; }
        /// <summary>
        /// 自定义的课程--开课时间
        /// </summary> 
        public DateTime? Opentime { get; set; }
        /// <summary>
        /// 自定义的课程--周期
        /// </summary> 
        public string? Cycle { get; set; }
        /// <summary>
        /// 自定义的课程--现在价格(2位小数)
        /// </summary> 
        public decimal? Price { get; set; }
    }

    /// <summary>
    /// 评测内容.选择不同的模式,内容不同.
    /// </summary>
    public interface IEvltContent 
    {
        /// <summary>
        /// 标题
        /// </summary>
        string Title { get; set; }
    }
    /// <summary>
    /// 评测内容--自由模式
    /// </summary>
    public class EvltContent1 : IEvltContent
    {
        /// <summary>
        /// 主键，编辑时使用
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;
        /// <summary>
        /// 内容
        /// </summary>
        public string? Content { get; set; }
        /// <summary>
        /// 图片地址s.没数据为空数组
        /// </summary>
        public string[] Pictures { get; set; } = default!;
        /// <summary>
        /// 缩略图地址s.没数据为空数组
        /// </summary>
        public string[] Thumbnails { get; set; } = default!;
    }
    /// <summary>
    /// 评测内容--专业模式
    /// </summary>
    public class EvltContent2 : IEvltContent
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;
        /// <summary>
        /// 维度内容.没数据为空数组
        /// </summary>
        public EvltContent2Step[] Steps = default!;
    }
    /// <summary>
    /// 每个维度的内容.如全没填都要传空对象{}
    /// </summary>
    public class EvltContent2Step
    {  /// <summary>
       /// 主键，编辑时使用
       /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// 文字内容
        /// </summary>
        public string? Content { get; set; }
        /// <summary>
        /// 每个维度（含普通模式）的图片s, 原型上的最多10张指一个模式下所以维度共10张<br/>
        /// 够10张后,后续维度界面不显示上传图片的块 <br/>
        /// 没数据为空数组
        /// </summary>
        public string[] Pictures { get; set; } = default!;
        /// <summary>
        /// 缩略图地址s.没数据为空数组
        /// </summary>
        public string[] Thumbnails { get; set; } = default!;
    }
}

#nullable disable