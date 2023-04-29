using iSchool.Organization.Appliaction.ViewModels.Coupon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Queries
{
    public interface IGoodsQueries
    {
        Task<GoodsInfo> GetGoodsInfoAsync(Guid goodsId);

        Task<SKUInfo> GetSKUInfoAsync(Guid skuId);

        Task<IEnumerable<SKUInfo>> GetSKUInfosAsync(IEnumerable<Guid> skuIds);

        Task<IEnumerable<Goods>> SearchGoods(IEnumerable<Guid> skuIds
            , IEnumerable<Guid> brandIds
            , IEnumerable<int> goodTypes
            , string searchText = null
            , int offset = 0
            , int limit = 20);

        /// <summary>
        /// 获取优惠专区的内容
        /// </summary>
        /// <returns></returns>
        Task<DiscountAreaPglist> GetDiscountAreaContent(int index = 1,
            string couresName = "", int pageIndex = 1, int pageSize = 10);
    }

}
