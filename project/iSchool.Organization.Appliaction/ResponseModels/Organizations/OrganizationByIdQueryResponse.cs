using iSchool.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{    
    /// <summary>
    /// 根据机构Id查询条件，返回的机构实体类
    /// </summary>
    public class OrganizationByIdQueryResponse
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid Id { get; set; }               
        
        /// <summary>
        /// 短Id
        /// </summary>
        public string No { get; set; }
        
        /// <summary>
        /// 机构底图Url
        /// </summary>
        public string OrgBaseMap { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 机构Logo
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 是否认证（true：认证；false：未认证）
        /// </summary>
        public bool Authentication { get; set; }

        /// <summary>
        /// 机构简介
        /// </summary>
        public string Intro { get; set; }

        /// <summary>
        /// 相关课程
        /// </summary>        
        public List<RelatedCoursesDto> RelatedCourses { get; set; }

        /// <summary>
        /// 相关评测
        /// </summary>
        public List<EvaluationItemDto> RelatedEvaluations { get; set; }

        /// <summary>
        /// 评测推荐（无课程无详情，则展示评测推荐）
        /// </summary>
        public List<EvaluationItemDto> RecommendEvaluations { get; set; }

        /// <summary>商品数</summary>
        public int GoodsCount { get; set; }
    }

    /// <summary>
    /// 机构-相关课程OrganizationRelatedCourses
    /// </summary>
    public class RelatedCourses
    {
        
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }
       
        /// <summary>
        /// 课程短Id
        /// </summary>
        public string CNO { get; set; }
        
        /// <summary>
        /// 课程名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程banner图片地址
        /// </summary>
        public string Banner { get; set; }

        /// <summary>
        /// 现在价格（认证则显示，否则不显示）
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 原始价格
        /// </summary>
        public decimal OrigPrice { get; set; }

        /// <summary>
        /// 库存（认证则显示，否则不显示）
        /// </summary>
        public int? Stock { get; set; }
       
    }

    /// <summary>
    /// 机构-相关课程OrganizationRelatedCourses
    /// </summary>
    public class RelatedCoursesDto
    {

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }
                
        /// <summary>
        /// 课程短Id
        /// </summary>
        public string CNO { get; set; }
       
        /// <summary>
        /// 课程名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程banner图片地址
        /// </summary>
        public List<string> Banner { get; set; } = new List<string>();

        /// <summary>
        /// 现在价格（认证则显示，否则不显示）
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 原始价格
        /// </summary>
        public decimal OrigPrice { get; set; }

        /// <summary>
        /// 库存（认证则显示，否则不显示）
        /// </summary>
        public int? Stock { get; set; }

    }
}
