using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>从course详情页里添加到购物车</summary>    
    public class AddCourseToShoppingCartCmd : IRequest<bool>
    {
        /// <summary>用户id</summary>    
        [JsonIgnore] public Guid UserId { get; set; }

        /// <summary>商品id</summary>
        public Guid GoodsId { get; set; }
        /// <summary>
        /// 更新商品数量.(>=1)
        /// </summary>
        public int Count { get; set; }
        /// <summary>商品单价</summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 前端传入的json对象`{}`额外数据.可null <br/>
        /// 例如 `{ fw: '', eid: '', surl: '' }`
        /// </summary>
        public JObject? Jo { get; set; }
    }

    public class UpUserCourseShoppingCartCmd : IRequest<UpUserCourseShoppingCartCmdResult>
    {
        /// <summary>用户id</summary>        
        public Guid UserId { get; set; }
        
        /// <summary>
        /// 操作s. 每种操作各填一种action
        /// </summary>
        public IEnumerable<UpCourseShoppingCartCmdAction> Actions { get; set; } = default!;
    }

    public class UpCourseShoppingCartCmdAction
    {
        public interface IGoodsAction
        {
            /// <summary>商品id</summary>
            Guid GoodsId { get; set; }
        }

        /// <inheritdoc cref="UpCountsAction"/>
        public UpCountsAction? UpCounts { get; set; } = null!;
        /// <summary>
        /// 更新(加减)商品数量
        /// </summary>
        public class UpCountsAction : IGoodsAction
        {
            /// <summary>商品id</summary>
            public Guid GoodsId { get; set; }
            /// <summary>
            /// 更新商品数量.`添加为正数, 扣减为负数.`
            /// </summary>
            public int Count { get; set; } // !=0
            /// <summary>
            /// false 或不传 = count是加减变化的数量 <br/>
            /// true = 直接把count赋值到cart中
            /// </summary>
            public bool Doset { get; set; } = false;
        }

        /// <inheritdoc cref="UpSelectedAction"/>
        public UpSelectedAction? UpSelected { get; set; } = null!;
		/// <summary>
        /// 更新商品是否被选中
        /// </summary>
		public class UpSelectedAction : IGoodsAction
        {
			/// <summary>商品id</summary>
            public Guid GoodsId { get; set; }
            /// <summary>
            /// 更新商品是否被选中.`true=选中, false=取消选中`
            /// </summary>
            public bool Selected { get; set; }
		}

        /// <inheritdoc cref="DelGoodsAction"/>
        public DelGoodsAction? DelGoods { get; set; } = null!;
        /// <summary>
        /// 删除商品(多个删除分开传)
        /// </summary>
        public class DelGoodsAction : IGoodsAction
        {
            /// <summary>商品id</summary>
            public Guid GoodsId { get; set; }
        }

        /// <inheritdoc cref="ClearGoodsAction"/>
        public ClearGoodsAction? ClearGoods { get; set; } = null!;
        /// <summary>
        /// 前端清除购物车.如需此操作,请传`{ "clearGoods": {} }`
        /// </summary>
        public class ClearGoodsAction
        {
        }
    }

#nullable disable
}
