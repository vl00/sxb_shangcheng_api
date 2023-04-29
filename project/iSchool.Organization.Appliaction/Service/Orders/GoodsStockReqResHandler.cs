using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public abstract class GoodsStockReqResHandler<TGoodsStockRequest, TGoodsStockResponse> //: IRequestHandler<GoodsStockRequest, GoodsStockResponse>
		where TGoodsStockRequest : GoodsStockRequest, new()
		where TGoodsStockResponse : GoodsStockResponse, new()
    {
        private readonly CSRedisClient _redis;
        private readonly IServiceProvider _services;
		
        protected GoodsStockReqResHandler(CSRedisClient redis, IServiceProvider services)
        {
            _redis = redis;
            _services = services;
        }

        /// <summary>
        /// 获取库存的cache-key
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        protected abstract string OnGetCacheKey(Guid id);

        /// <summary>
        /// load库存from db to cache
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        protected abstract Task<int?> OnLoadGoodsStock(Guid id);

        /// <summary>
        /// save库存from db to cache
        /// </summary>
        /// <param name="id">商品id</param>
        /// <param name="stock1">现在cache中的库存</param>
        /// <param name="stock0">对比原库存</param>
        /// <returns></returns>
        protected abstract Task<bool> OnSaveGoodsStock(Guid id, int stock1, int? stock0);

        /// <summary>
        /// 后台修改库存
        /// </summary>
        /// <param name="id">商品id</param>
        /// <param name="stockcount">新的库存数</param>
        /// <returns></returns>
        protected abstract Task OnBgResetGoodsStock(Guid id, int stockcount);


        public virtual async Task<TGoodsStockResponse> Handle(TGoodsStockRequest req, CancellationToken cancellationToken = default)
        {
            var res = new TGoodsStockResponse();
            if (req.StockCmd != null)
                res.StockResult = await OnHandle(req.StockCmd);
            else if (req.AddStock != null)
                res.AddStockResult = await OnHandle(req.AddStock);
            else if (req.GetStock != null)
                res.GetStockResult = await OnHandle(req.GetStock);
            else if (req.SyncSetStock != null)
                res.SyncSetResult = await OnHandle(req.SyncSetStock);
            else if (req.BgSetStock != null)
                res.BgSetStockIsOk = await OnHandle_BgSet(req.BgSetStock);
            return res;
        }

        /**
         *   -3:库存未初始化
         *   -2:库存不足
         *   -1:不限库存
         *   大于等于0:剩余库存（扣减之后剩余的库存）
         */
        const string lua_stock_substract = @"
            local ttl = redis.call('ttl', KEYS[1])
            if (ttl > -2) then
                local stock = tonumber(redis.call('hget', KEYS[1], 'stock1'))
                local num = tonumber(ARGV[1])
                if (ttl > -1) and (ttl < 30) then
                    redis.call('expire', KEYS[1], ARGV[2])
                end
                if (stock == -1) then
                    return -1
                end
                if (stock >= num) then
                    return redis.call('hincrby', KEYS[1], 'stock1', 0 - num)
                end
                return -2
            end
            return -3
        ";

        const string lua_stock_load = @"
            if (ARGV[3] == '0') then
                redis.call('hsetnx', KEYS[1], 'stock1', ARGV[1])
                redis.call('hsetnx', KEYS[1], 'stock0', ARGV[1])
                redis.call('expire', KEYS[1], ARGV[2])
            else
                redis.call('hset', KEYS[1], 'stock1', ARGV[1])
                redis.call('hset', KEYS[1], 'stock0', ARGV[1])
                redis.call('expire', KEYS[1], ARGV[2])
            end
        ";

        /// <summary>
        /// 扣减库存
        /// </summary>
        private async Task<int> OnHandle(GoodsStockCommand cmd)
        {
            var k = OnGetCacheKey(cmd.Id);
            var r = Convert.ToInt32(await _redis.EvalAsync(lua_stock_substract, k, cmd.Num, 60 * 60 * 24));
            if (r == -3)
            {
                var stock = await OnLoadGoodsStock(cmd.Id);
                stock ??= 0;
                await _redis.EvalAsync(lua_stock_load, k, stock, 60 * 60 * 24, 0);

                r = Convert.ToInt32(await _redis.EvalAsync(lua_stock_substract, k, cmd.Num, 60 * 60 * 24));
            }
            return r;
        }

        /// <summary>
        /// 添加(归还)库存
        /// </summary>
        private async Task<int> OnHandle(AddGoodsStockCommand cmd)
        {
            if (cmd.FromDBIfNotExists)
                return await OnHandle(new GoodsStockCommand { Id = cmd.Id, Num = -1 * cmd.Num });

            // 获取库存的cache-key
            var k = OnGetCacheKey(cmd.Id);
            var i = await _redis.HIncrByAsync(k, "stock1", cmd.Num);
            return (int)i;
        }

        /// <summary>
        /// 获取库存
        /// </summary>
        private async Task<int?> OnHandle(GetGoodsStockQuery query)
        {
            var k = OnGetCacheKey(query.Id);
            var stock = await _redis.HGetAsync<int?>(k, "stock1");
            if (stock == null && query.FromDBIfNotExists)
            {
                stock = await OnLoadGoodsStock(query.Id);
                stock ??= 0;
                await _redis.EvalAsync(lua_stock_load, k, stock, 60 * 60 * 24, 0);
            }
            return stock;
        }


        /// <summary>
        /// cache同步库存到db
        /// </summary>
        private async Task<int?> OnHandle(SyncSetGoodsStockCommand cmd)
        {
            var stockInCache = await OnHandle(new GoodsStockCommand
            {
                Id = cmd.Id,
                Num = cmd.AddNum * -1,
            });
            if (stockInCache == -1)
            {
                return stockInCache;
            }

            var lckfay = _services.GetService<iSchool.Infras.Locks.ILock1Factory>();
            await using var lck1 = await lckfay.LockAsync($"org:lck:60-15:sync_goods_stock:{cmd.Id}", 1000 * 60);
            if (!lck1.IsAvailable) return null;

            var k = OnGetCacheKey(cmd.Id);
            var dict = await _redis.HGetAllAsync<int>(k);
            var stock0 = dict.GetValueEx("stock0", -1);
            var stock1 = dict.GetValueEx("stock1", -1);
            if (stock0 == -1)
            {
                return stockInCache;
            }

            if (await OnSaveGoodsStock(cmd.Id, stock1, stock0))
            {
                await _redis.HSetAsync(k, "stock0", stock1);
            }

            return stockInCache;
        }

        /// <summary>
        /// 后台设置库存
        /// </summary>
        private async Task<bool> OnHandle_BgSet(BgSetGoodsStockCommand cmd)
        {
            var lckfay = _services.GetService<iSchool.Infras.Locks.ILock1Factory>();
            await using var lck1 = await lckfay.LockAsync($"org:lck:60-15:sync_goods_stock:{cmd.Id}", 1000 * 60, 200);
            if (!lck1.IsAvailable) return false;

            await OnBgResetGoodsStock(cmd.Id, cmd.StockCount);

            var k = OnGetCacheKey(cmd.Id);
            await _redis.EvalAsync(lua_stock_load, k, cmd.StockCount, 60 * 60 * 24, 1);
            return true;
        }

    }
}
