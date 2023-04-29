#nullable enable

using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// mini添加评测
    /// </summary>
    public class MiniAddEvaluationCommand : IRequest<EvaluationAddedResult>
    {
        /// <summary>评测ID.可null.用于修改</summary>
        public Guid? EvaluationId { get; set; }

        /// <summary>1=自由模式 </summary>
        public int Mode { get; set; } = 1;

        /// <summary>
        /// 评测内容.自由模式不为null,其他模式为null
        /// </summary>
        public MiniEvltContent1? Ctt1 { get; set; }
        /// <summary>
        /// 评测内容--自由模式
        /// </summary>
        public class MiniEvltContent1 : IEvltContent
        {
            ///// <summary>
            ///// 主键，编辑时使用
            ///// </summary>
            //public Guid? Id { get; set; }
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

            /// <summary>视频地址.可null.</summary>
            public string? VideoUrl { get; set; }
            /// <summary>视频封面图.可null.</summary>
            public string? VideoCoverUrl { get; set; }
        }

        /// <summary>
        /// 关联主体mode 1=课程 2=品牌 3=其他
        /// </summary>
        public int RelatedMode { get; set; } = 3;
        /// <summary>关联的课程s Id. 可null.</summary>
        public Guid[]? RelatedCourseIds { get; set; }
        /// <summary>关联的品牌s Id. 可null.</summary>
        public Guid[]? RelatedOrgIds { get; set; }
    }


    

}

#nullable disable