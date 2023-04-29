using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>课程商品</summary>
	public class CourseGoodsStockRequest : GoodsStockRequest, IRequest<CourseGoodsStockResponse>
	{ }

#nullable enable

    public abstract class GoodsStockRequest //: IRequest<GoodsStockResponse>
    {         
		/// <inheritdoc cref="GoodsStockCommand"/>
        public GoodsStockCommand? StockCmd { get; set; }       
        /// <inheritdoc cref="AddGoodsStockCommand"/>
        public AddGoodsStockCommand? AddStock { get; set; }
        /// <inheritdoc cref="GetGoodsStockQuery"/>
        public GetGoodsStockQuery? GetStock { get; set; }

        /// <inheritdoc cref="SyncSetGoodsStockCommand"/>
        public SyncSetGoodsStockCommand? SyncSetStock { get; set; }

        /// <inheritdoc cref="BgSetGoodsStockCommand"/>
        public BgSetGoodsStockCommand? BgSetStock { get; set; }
    }

	/// <summary>
    /// 扣减库存(会从db加载到cache)
    /// </summary>
    public class GoodsStockCommand
    {
        /// <summary>商品id</summary>
        public Guid Id { get; set; }
        public int Num { get; set; }
    }

	/// <summary>
    /// 归还库存
    /// </summary>
    public class AddGoodsStockCommand
    {
        /// <summary>商品id</summary>
        public Guid Id { get; set; }
        public int Num { get; set; }
        public bool FromDBIfNotExists { get; set; }
    }

	/// <summary>
    /// load库存
    /// </summary>
    public class GetGoodsStockQuery
    {
        /// <summary>商品id</summary>
        public Guid Id { get; set; }
        public bool FromDBIfNotExists { get; set; }
    }

	/// <summary>
    /// 库存从cache同步save到db
    /// </summary>
    public class SyncSetGoodsStockCommand
    {
        /// <summary>商品id</summary>
        public Guid Id { get; set; }
        public int AddNum { get; set; }
    }

	/// <summary>
    /// 后台改变库存
    /// </summary>
    public class BgSetGoodsStockCommand
    {
        /// <summary>商品id</summary>
        public Guid Id { get; set; }
        public int StockCount { get; set; }
    }

#nullable disable
}
