using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 种草详情dto
    /// </summary>
    public class MiniEvltDetailDto : ISeoTDKInfo
    {
        public DateTime Now { get; set; }
        /// <summary>评测id</summary>
        public Guid Id { get; set; }
        /// <summary>评测短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>标题</summary>
        public string Title { get; set; } = default!;
        /// <summary>是否精华</summary>
        public bool Stick { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; } = DateTime.Parse("1986-06-01");
        public byte Mode { get; set; }
        /// <summary>封面</summary>
        public string Cover { get; set; } = default!;

        /// <summary>是否是纯文字(没图)</summary>
        public bool IsPlaintext { get; set; }
        /// <summary>是否有视频</summary>
        public bool HasVideo { get; set; }
        /// <summary>内容</summary>
        public string Content { get; set; }
        /// <summary>分享的内容 正文 50 字左右</summary>
        public string SharedContent { get; set; }

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
        
        /// <summary>分享数</summary>
        public int SharedCount { get; set; }
        /// <summary>点赞数</summary>
        public int LikeCount { get; set; }
        /// <summary>是否是我曾经点赞过的</summary>
        public bool IsLikeByMe { get; set; }
        /// <summary>是否我自己发布的评测</summary>
        public bool IsSelf { get; set; }

        /// <summary>seo  decription</summary>
        public string Tdk_d { get; set; } = default!;

        /// <summary>
        /// 关联主体mode 1=课程 2=品牌 3=其他
        /// </summary>
        public int RelatedMode { get; set; } = 3;
        /// <summary>关联的课程.可null</summary>
        public IEnumerable<MiniEvltRelatedCourseDto> RelatedCourses { get; set; } = null;
        /// <summary>关联的品牌.可null</summary>
        public IEnumerable<MiniEvltRelatedOrgDto> RelatedOrgs { get; set; } = null;
    }

    /// <summary>评测关联的课程</summary>
    public class MiniEvltRelatedCourseDto
    {
        /// <summary>课程id</summary>
        public Guid Id { get; set; }
        /// <summary>课程短id</summary>
        public string Id_s { get; set; } = default;
        /// <summary>课程banner图片地址</summary>
        public string Banner { get; set; }
        /// <summary>课程标题</summary>
        public string Title { get; set; } = default;
        /// <summary>最低价格</summary>
        public double Price { get; set; }      
    }

    /// <summary>评测关联的品牌</summary>
    public class MiniEvltRelatedOrgDto
    {
        /// <summary>机构id</summary>
        public Guid Id { get; set; }
        /// <summary>机构短id</summary>
        public string Id_s { get; set; } = default;
        /// <summary>logo图片地址</summary>
        public string Logo { get; set; }
        /// <summary>机构名</summary>
        public string Name { get; set; } = default;
        /// <summary>机构描述</summary>
        public string Desc { get; set; }
        /// <summary>机构副描述</summary>
        public string Subdesc { get; set; }
        /// <summary>是否认证</summary>
        public bool Authentication { get; set; }
        /// <summary>商品数</summary>
        public int GoodsCount { get; set; }
    }
}
