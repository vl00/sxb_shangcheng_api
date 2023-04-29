using iSchool.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Mini.ResponseModels.MaterialLibrary
{
    public class MiniMaterialLibraryItemDto
    {
        /// <summary>
        /// 素材ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>课程id</summary>
        public Guid CourseId { get; set; }

        /// <summary>标题</summary>
        public string Title { get; set; } = default!;

        /// <summary>创建时间</summary>
        public string  CreateTime { get; set; } = DateTime.Parse("1986-06-01").UnixTicks();



        /// <summary>内容</summary>
        public string Content { get; set; }

        /// <summary>评测图(原图)</summary>
        public IEnumerable<string> Imgs { get; set; }
        /// <summary>评测图(缩略图)</summary>
        public IEnumerable<string> Imgs_s { get; set; }
        /// <summary>视频地址</summary>
        public string VideoUrl { get; set; }
        /// <summary>视频封面图</summary>
        public string VideoCoverUrl { get; set; }

        /// <summary>作者id</summary>
        public Guid AuthorId { get; set; }
        /// <summary>作者名</summary>
        public string AuthorName { get; set; }
        /// <summary>作者头像</summary>
        public string AuthorHeadImg { get; set; }

        /// <summary>下载数</summary>
        public int DownloadTime { get; set; }


    }

    public class MaterialLibraryDataDB
    {
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid Id { get; set; }

        /// <summary>
        /// Desc:课程id
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid CourseId { get; set; }

        /// <summary>
        /// Desc:标题
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Title { get; set; }

        /// <summary>
        /// Desc:内容
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Content { get; set; }

        /// <summary>
        /// Desc:图片
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string pictures { get; set; }

        /// <summary>
        /// Desc:缩略图
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string thumbnails { get; set; }

        /// <summary>
        /// Desc:视频
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string video { get; set; }

        /// <summary>
        /// Desc:视频封面
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string videoCover { get; set; }

       

        /// <summary>
        /// Desc:下载数
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int DownloadTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid Creator { get; set; }


      
    }

}
