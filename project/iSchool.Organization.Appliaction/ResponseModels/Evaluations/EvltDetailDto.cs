#nullable enable

using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 评测详情
    /// </summary>
    public class EvltDetailDto : ISeoTDKInfo
    {
        /// <summary>
        /// 评测id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 评测短id
        /// </summary>
        public string Id_s { get; set; } = default!;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;
        /// <summary>
        /// 是否置顶/精华
        /// </summary>
        public bool Stick { get; set; }
        /// <summary>
        /// 封面图
        /// </summary>
        public string Cover { get; set; } = default!;
        /// <summary>
        /// 作者id
        /// </summary>
        public Guid AuthorId { get; set; }
        /// <summary>
        /// 作者名
        /// </summary>
        public string AuthorName { get; set; } = default!;
        /// <summary>
        /// 作者头像
        /// </summary>
        public string? AuthorHeadImg { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Parse("1986-06-01");
        /// <summary>
        /// 编辑时间(可为null)
        /// </summary>
        public DateTime? Mtime { get; set; }
        /// <summary>
        /// 服务器的当前时间
        /// </summary>
        public DateTime Now { get; set; } //= DateTime.Now;
        /// <summary>
        /// 专题id
        /// </summary>
        public Guid? SpecialId { get; set; }
        /// <summary>
        /// 专题短id
        /// </summary>
        public string? SpecialId_s { get; set; }
        /// <summary>
        /// 专题名称
        /// </summary>
        public string? SpecialName { get; set; }

        /// <summary>
        /// 1=自由模式 2=专业模式
        /// </summary>
        public byte Mode { get; set; }

        /// <summary>
        /// 分享的内容 正文 50 字左右
        /// </summary>
        public string? SharedContent { get; set; }
        /// <summary>
        /// 内容s. 没内容为空数组.
        /// </summary>
        public EvaluationContentDto?[] Contents { get; set; } = default!;
        /// <summary>
        /// 评论 前20条. 没内容为空数组.
        /// </summary>
        public EvaluationCommentDto[] Comments { get; set; } = default!;
        /// <summary>
        /// 投票. 没内容为null
        /// </summary>
        public EvaluationVoteDto? Vote { get; set; }
        /// <summary>
        /// 课程part. 没内容为空对象.
        /// </summary>
        public EvaluationCoursePartDto CoursePart { get; set; } = default!;

        /// <summary>
        /// 收藏数
        /// </summary>
        public int CollectionCount { get; set; } = 0;
        /// <summary>
        /// 评论数+回复数
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// 评论数，不包含回复
        /// </summary>
        public int FirstCommentCount { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int LikeCount { get; set; }

        /// <summary>
        /// 是否是我曾经点赞过的
        /// </summary>
        public bool IsLikeByMe { get; set; }
        /// <summary>
        /// 是否是我曾经收藏过的
        /// </summary>
        public bool IsCollectByMe { get; set; }
        /// <summary>
        /// 是否我自己发布的评测
        /// </summary>
        public bool IsSelf { get; set; }
        /// <summary>
        /// seo  decription
        /// </summary>
        public string Tdk_d { get; set; } = default!;
        /// <summary>
        /// 能否编辑.不能编辑应该隐藏编辑按钮<br/>
        /// 例如活动审核成功后几日内不能编辑等等
        /// </summary>
        public bool Editable { get; set; } = true;
        /// <summary>不能编辑时,需要弹出的qrcode</summary>
        public string? EditdisableQrcode { get; set; }
    }

    /// <summary>
    /// 评测内容项
    /// </summary>
    public class EvaluationContentDto
    {
        /// <summary>
        /// 评测内容项id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 文字内容
        /// </summary>
        public string? Content { get; set; }
        /// <summary>
        /// 每个维度（含普通模式）的图片s, 原型上的最多10张指一个模式下所以维度共10张<br/>
        /// 够10张后,后续维度界面不显示上传图片的块 <br/>
        /// 没内容为空数组
        /// </summary>
        public string[] Pictures { get; set; } = default!;
        /// <summary>
        /// 缩略图.没内容为空数组
        /// </summary>
        public string[] Thumbnails { get; set; } = default!;

        ////只显示内容 不需要显示问题
        //public string Question { get; set; }

        /// <summary>视频地址</summary>
        public string? VideoUrl { get; set; }
        /// <summary>视频封面图</summary>
        public string? VideoCoverUrl { get; set; }
    }

    /// <summary>
    /// 评测投票
    /// </summary>
    public class EvaluationVoteDto
    {
        /// <summary>
        /// 投票id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 投票类型 保留字段 <br/>
        /// 1单选 2多选
        /// </summary>
        public byte Type { get; set; } = 1;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;
        /// <summary>
        /// 详情|补充内容.可null
        /// </summary>
        public string? Detail { get; set; }
        /// <summary>
        /// 结束时间.可null
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 投票选项s.没内容为空数组
        /// </summary>
        public EvaluationVoteItemDto[] Items { get; set; } = default!;
        /// <summary>
        /// 是否是我参与过的
        /// </summary>
        public bool IsVotedByMe { get; set; }
        /// <summary>
        /// false=不显示票数 <br/>
        /// true=显示票数
        /// </summary>
        public bool CanShowCount { get; set; }
    }
    /// <summary>
    /// 评测投票项
    /// </summary>
    public class EvaluationVoteItemDto
    {
        /// <summary>
        /// 投票选项id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 包含评测+投票+投票项的关系,用于用户选择此项时传递给后台
        /// </summary>
        public string Token { get; set; } = default!;
        /// <summary>
        /// 票数
        /// </summary>
        public int? Count { get; set; }
        /// <summary>
        /// 是否是我投的票
        /// </summary>
        public bool IsVoteByMe { get; set; }
        /// <summary>
        /// 投票项内容 
        /// </summary>
        public string Content { get; set; } = default!;
    }

    /// <summary>
    /// 评测课程part
    /// </summary>
    public class EvaluationCoursePartDto
    {
        /// <summary>机构id, 用户自己填的为null</summary>
        public Guid? OrgId { get; set; }
        /// <summary>机构短id</summary>
        public string? OrgId_s { get; set; }
        /// <summary>机构名</summary>
        public string OrgName { get; set; } = default!;
        /// <summary>机构logo</summary>
        public string? OrgLogo { get; set; }
        /// <summary>机构副标题1</summary>
        public string? OrgDesc { get; set; }
        /// <summary>机构副标题2</summary>
        public string? OrgSubdesc { get; set; }

        /// <summary>机构是否验证</summary>
        public bool OrgIsAuthenticated { get; set; }
        ////课程是否验证
        //public bool CourseIsAuthenticated { get; set; }

        /// <summary>课程id, 用户自己填的为null</summary>
        public Guid? CourseId { get; set; }
        /// <summary>课程短id</summary>
        public string? CourseId_s { get; set; }
        /// <summary>课程名称|标题</summary>
        public string? CourseName { get; set; }
        /// <summary>课程副标题</summary>
        public string? CourseSubtitle { get; set; }
        /// <summary>课程图片</summary>
        public string[]? CourseBanner { get; set; }
        /// <summary>课程价格</summary>
        public decimal? Price { get; set; }
        /// <summary>年龄段</summary>
        public string? Age { get; set; }
        /// <summary>上课方式.没内容为null</summary>
        public string[]? Mode { get; set; }
        /// <summary>上课时间</summary>
        public DateTime? OpenTime { get; set; }
        /// <summary>上课时长</summary>
        public string? Duration { get; set; }
        /// <summary>周期</summary>
        public string? Cycle { get; set; }
        /// <summary>科目</summary>
        public string? Subject { get; set; }
        /// <summary>科目(数值)</summary>
        public int Subj { get; set; }

        /// <summary>
        /// 科目(原数值)
        /// </summary>
        [JsonIgnore]
        public int? Subj0
        {
            get 
            {
                if (Subject == null) return null;
                var em = EnumUtil.GetDescs<SubjectEnum>().FirstOrDefault(_ => _.Desc == Subject);
                if (em.Desc == null) return null;
                return em.Value.ToInt();
            }
        }
    }
}

#nullable disable