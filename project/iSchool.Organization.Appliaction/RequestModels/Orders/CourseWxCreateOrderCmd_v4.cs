using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
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
    /// 
    /// </summary>
    public class CourseWxCreateOrderCmd_v4 : IRequest<Res2Result<CourseWxCreateOrderCmdResult_v4>>
    {
        /// <summary>商品s</summary>
        [Required]
        public GoodsItem4Order[] Goods { get; set; } = default!;

        /// <summary>
        /// 备注s <br/>
        /// 例子：`{ "供应商1 id": "备注1", "供应商2 id": "备注2", ... }`
        /// </summary>
        public Dictionary<Guid, string>? Remarks { get; set; } = default!;

        /// <summary>地址.必须传</summary>
        [Required]
        public RecvAddressDto AddressDto { get; set; } = default!;

        /// <summary>
        /// 孩子归档信息ids. 用于新小程序.<br/>
        /// 如没数据,传`undefined`
        /// </summary>
        public Guid[]? ChildrenInfoIds { get; set; }

        /// <summary>
        /// 上课号码. 新小程序购买网课必传         
        /// </summary>
        public string? BeginClassMobile { get; set; }

        /// <summary>
        /// 分销上一级user id或user code <br/>
        /// 没有传null
        /// </summary>
        public string? FxHeaducode { get; set; }

        /// <summary>openid</summary>
        public string? OpenId { get; set; } = null!;
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
        /// 供应商信息s <br/>
        /// 例子：`{ "供应商1 id": {}, "供应商2 id": {}, ... }`
        /// </summary>
        public Dictionary<Guid, OrgItem4Order>? Orgs { get; set; } = default!;

        /// <summary>版本(此参数后台使用)</summary>     
        [JsonIgnore]
        public string? Ver { get; set; } = default!;

        /// <summary>
        /// 表示只创建订单而不调用预支付.用于跳转到新小程序中支付
        /// </summary>
        [JsonIgnore]
        public bool IsOnlyCreateOrder { get; set; } = false;

        /// <summary>
        /// 前端传入的json对象`{}`额外数据.可null <br/>
        /// 例如 `{ fw: '', eid: '', surl: '' }`
        /// </summary>
        public JObject? Jo { get; set; }

        /// <summary>
        /// 优惠券ID
        /// </summary>
        public Guid? CouponReceiveId { get; set; }

        /// <summary>
        /// 是否使用积分支付
        /// </summary>
        public int? PayPoints { get; set; }


    }

    public class GoodsItem4Order
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
        public int BuyCount { get; set; } = 1;

        /// <summary>
        /// 前端传入到购物车商品项的json对象`{}`额外数据.可null <br/>
        /// 例如 `{ fw: '', eid: '', surl: '' }`
        /// </summary>
        public JObject? Jo { get; set; }
    }

    public class OrgItem4Order
    {
        /// <summary>运费</summary>
        public decimal Freight { get; set; }
        
        // 配送方式
    }

#nullable disable
}
