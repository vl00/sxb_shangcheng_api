using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 根据商品id查询商品简易信息
    /// </summary>
    public class CourseGoodsSimpleInfoByIdQuery : IRequest<CourseGoodsSimpleInfoDto?>
    {        
        /// <summary>商品id</summary>
        public Guid GoodsId { get; set; }
        /// <summary>是否要cache</summary>
        public bool UseCache { get; set; } = true;
        /// <summary>是否允许查无效的商品</summary>
        public bool AllowNotValid { get; set; } = false;

        public bool NeedCourse { get; set; } = false;
    }

    public class CourseGoodsSimpleInfoByPropItemsQuery : IRequest<ApiCourseGoodsSimpleInfoDto>
    {
        /// <summary>属性项id数组</summary>
        public Guid[] PropItemIds { get; set; } = default!;
        /// <summary>课程id</summary>
        public Guid CourseId { get; set; }
    }

    /// <summary>
    /// 课程商品结算info
    /// </summary>
    public class CourseGoodsSettleInfoQuery : IRequest<CourseGoodsSettleInfoQryResult>
    {
        /// <summary>商品id</summary>
        public Guid Id { get; set; }
        /// <summary>购买数量</summary>
        [Obsolete]
        public int BuyAmount
        {
            get => BuyCount;
            set => BuyCount = value;
        }
        /// <summary>购买数量（跟 BuyAmount 一样）</summary>
        public int BuyCount { get; set; } = 1;

        [Newtonsoft.Json.JsonIgnore]
        public bool UseQrcode { get; set; } = true;

        [Newtonsoft.Json.JsonIgnore]
        public bool AllowNotValid { get; set; } = false;
    }

    /// <summary>
    /// 多课程商品结算info
    /// </summary>
    public class CourseMultiGoodsSettleInfosQuery : IRequest<CourseMultiGoodsSettleInfosQryResult>
    {
        public CourseMultiGoodsSettleInfos_Sku[] Goods { get; set; } = default!;

        [Newtonsoft.Json.JsonIgnore]
        public bool UseQrcode { get; set; } = true;

        [Newtonsoft.Json.JsonIgnore]
        public bool AllowNotValid { get; set; } = false;

        public Guid? UserId { get; set; }
    }

    public class CourseMultiGoodsSettleInfos_Sku
    {
        /// <summary>商品id</summary>
        public Guid Id { get; set; }
        /// <summary>购买数量</summary>
        public int BuyCount { get; set; } = 1;
    }

#nullable disable
}
