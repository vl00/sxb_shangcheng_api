using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 购买课程-下单
    /// </summary>
    [Obsolete("1.9-")]
    public class CourseWxCreateOrderCommand : IRequest<CourseWxCreateOrderCmdResult>
    {
        ///// <summary>课程id.(已弃用)</summary>
        //[Obsolete]
        //public Guid CourseId { get; set; }

        /// <summary>商品id.必传</summary>
        [Required]
        public Guid GoodsId { get; set; }

        /// <summary>
        /// 当前页面课程价格.必传<br/>
        /// 用于验证购买的时候课程有无被后台修改.
        /// </summary>
        [Required]
        public decimal Price { get; set; } = 0;

        /// <summary>购买数量.默认为1</summary>
        [Required]
        public int BuyAmount { get; set; } = 1;

        /// <summary>地址.必须传</summary>
        [Required]
        public RecvAddressDto AddressDto { get; set; } = default!;

        /// <summary>孩子年龄?</summary>
        public string? Age { get; set; }
        /// <summary>
        /// 孩子归档信息ids. 用于新小程序.<br/>
        /// 如没数据,传`undefined`
        /// </summary>
        public Guid[]? ChildrenInfoIds { get; set; }

        /// <summary>
        /// 上课号码. 新小程序购买必传         
        /// </summary>
        public string? BeginClassMobile { get; set; }

        /// <summary>openid</summary>
        public string? OpenId { get; set; } = null!;

        /// <summary>
        /// 分销上一级user id或user code <br/>
        /// 没有传 null
        /// </summary>
        public string? FxHeaducode { get; set; }

        /// <summary>
        /// 0=h5jsapi <br/>
        /// 1=小程序 <br/>
        /// 2=h5支付 <br/>
        /// </summary>
        public int IsWechatMiniProgram { get; set; } = 0;
        /// <summary>
        /// appid主要用于小程序支付, 其他情况不管或传null
        /// </summary>
        public string? AppId { get; set; }

        /// <summary>
        /// 版本(此参数后台使用)
        /// </summary>
        public string? Ver { get; set; }

        /// <summary>订单备注</summary>
        public string? Remark { get; set; }

        /// <summary>来源1</summary>
        public Source1Class? Source1 { get; set; }
        /// <summary>来源1</summary>
        public class Source1Class
        {
            /// <summary>学部id</summary>
            public Guid? Eid { get; set; }            
            /// <summary>来源url</summary>
            public string? Surl { get; set; }
        }

        /// <summary>fw</summary>
        public string? Fw { get; set; }
    }

    public class CourseWxCreateOrderCommand_v3 : IRequest<Res2Result<CourseWxCreateOrderCmdResult_v4>>
    {
        /// <summary>商品id.必传</summary>
        [Required]
        public Guid GoodsId { get; set; }
        /// <summary>
        /// 当前页面课程价格.必传<br/>
        /// 用于验证购买的时候课程有无被后台修改.
        /// </summary>
        [Required]
        public decimal Price { get; set; } = 0;
        /// <summary>购买数量.默认为1</summary>
        [Required]
        public int BuyAmount { get; set; } = 1;

        /// <summary>地址.必须传</summary>
        [Required]
        public RecvAddressDto AddressDto { get; set; } = default!;

        /// <summary>孩子年龄?</summary>
        public string? Age { get; set; }
        /// <summary>
        /// 孩子归档信息ids. 用于新小程序.<br/>
        /// 如没数据,传`undefined`
        /// </summary>
        public Guid[]? ChildrenInfoIds { get; set; }

        /// <summary>
        /// 上课号码. 新小程序购买必传         
        /// </summary>
        public string? BeginClassMobile { get; set; }

        /// <summary>openid</summary>
        public string? OpenId { get; set; } = null!;

        /// <summary>
        /// 分销上一级user id或user code <br/>
        /// 没有传 null
        /// </summary>
        public string? FxHeaducode { get; set; }

        /// <summary>
        /// 0=h5jsapi <br/>
        /// 1=小程序 <br/>
        /// 2=h5支付 <br/>
        /// </summary>
        public int IsWechatMiniProgram { get; set; } = 0;
        /// <summary>
        /// appid主要用于小程序支付, 其他情况不管或传null
        /// </summary>
        public string? AppId { get; set; }

        /// <summary>
        /// 版本(此参数后台使用)
        /// </summary>
        public string? Ver { get; set; }

        /// <summary>订单备注</summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 前端传入的json对象`{}`额外数据.可null <br/>
        /// 例如 `{ fw: '', eid: '', surl: '' }`
        /// </summary>
        public JObject? Jo { get; set; }
    }

#nullable disable
}
