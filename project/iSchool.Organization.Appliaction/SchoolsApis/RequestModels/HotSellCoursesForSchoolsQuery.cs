using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class HotSellCoursesOrgsForSchoolsQuery : IRequest<HotSellCoursesOrgsForSchoolsQryResult>
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
    }
}

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class HotSellCoursesOrgsForSchoolsQryResult
    {
        public DateTime? Time { get; set; }

        /// <summary>热卖课程</summary>
        public IEnumerable<PcCourseItemDto2> HotSellCourses { get; set; } = default!;
        /// <summary>推荐机构</summary>
        public IEnumerable<PcOrgItemDto> RecommendOrgs { get; set; } = default!;
    }

    public class PcCourseItemDto2
    {
        /// <summary>课程Id</summary>
        public Guid Id { get; set; }
        /// <summary>课程短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>机构名</summary>
        public string? OrgName { get; set; }
        /// <summary>课程标题</summary>
        public string? Title { get; set; }
        /// <summary>课程副标题</summary>
        public string? Subtitle { get; set; }
        ///// <summary>课程科目</summary>
        //public int Subject { get; set; }

        /// <summary>课程banner图片地址</summary>
        public string? Banner { get; set; }
        /// <summary>现在价格</summary>
        public decimal? Price { get; set; }
        /// <summary>原始价格</summary>
        public decimal? OrigPrice { get; set; }

        /// <summary>是否认证（true：认证；false：未认证）</summary>
        public bool Authentication { get; set; }

        public int Sellcount { get; set; }

        /// <summary>是否爆款</summary>
        public bool IsExplosions { get; set; }

        /// <summary>pc的url</summary>
        public string? PcUrl { get; set; }
        /// <summary>m站的url</summary>
        public string? MUrl { get; set; }
        /// <summary>微信小程序二维码</summary>
        public string? MpQrcode { get; set; }

        /// <summary>标签</summary>
        public List<string>? Tags { get; set; }
    }

    public class PcCourseItemDto3
    {
        /// <summary>课程Id</summary>
        public Guid Id { get; set; }
        /// <summary>课程短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>机构名</summary>
        public string? OrgName { get; set; }
        /// <summary>课程标题</summary>
        public string? Title { get; set; }
        /// <summary>课程副标题</summary>
        public string? Subtitle { get; set; }        
        /// <summary>课程banner图片地址</summary>
        public string? Banner { get; set; }
        /// <summary>现在价格</summary>
        public decimal? Price { get; set; }
        /// <summary>原始价格</summary>
        public decimal? OrigPrice { get; set; }

        /// <summary>是否认证（true：认证；false：未认证）</summary>
        public bool Authentication { get; set; }

        public int Sellcount { get; set; }

        /// <summary>是否爆款</summary>
        public bool IsExplosions { get; set; }

        /// <summary>标签</summary>
        public List<string>? Tags { get; set; }

        /// <summary>pc的url</summary>
        public string? PcUrl { get; set; }
        /// <summary>m站的url</summary>
        public string? MUrl { get; set; }
        /// <summary>微信小程序二维码</summary>
        public string? MpQrcode { get; set; }
        /// <summary>h5跳微信小程序url</summary>
        public string? H5ToMpUrl { get; set; }

        /// <summary>true=有效 false=无效</summary>
        public bool IsOnline { get; set; } = true;
    }
}

#nullable disable
