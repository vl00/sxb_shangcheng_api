using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    public class CheckGoodSkuCmd : IRequest<ResponseResult<CheckGoodSkuCmdResult>>
    {
        /// <summary>商品s</summary>
        public GoodsItem4Check[] Goods { get; set; } = default!;
        /// <summary>用户</summary>
        public Guid UserId { get; set; }

        /// <summary>是否源于添加购物车</summary>
        public bool IsAddToCart { get; set; } = false;

        public bool IsInlck { get; set; }
    }

    public class GoodsItem4Check
    {
        /// <summary>商品id.必传</summary>
        [Required]
        public Guid GoodsId { get; set; }

        /// <summary>
        /// 当前页面课程价格.<br/>
        /// 用于验证购买的时候课程有无被后台修改.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>购买数量.默认为1</summary>
        [Required]
        public int BuyCount { get; set; } = 1;
    }

#nullable disable
}
