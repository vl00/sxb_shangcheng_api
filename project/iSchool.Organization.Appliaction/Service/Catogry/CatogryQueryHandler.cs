using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.KeyVal;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.KeyValues
{
    /// <summary>
    /// 商品分类
    /// </summary>
    public class CatogryQueryHandler : IRequestHandler<CatogryQuery, ResponseResult>
    {

        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public CatogryQueryHandler(IOrgUnitOfWork orgUnitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _redisClient = redisClient;
        }

        public Task<ResponseResult> Handle(CatogryQuery request, CancellationToken cancellationToken)
        {
            string key = string.Format(CacheKeys.CatogoryItemsForDiffrentRootLevel, request.Root);
            if (3 == request.Root)
            {
                var threeLevldata = _redisClient.Get<LevelThreeCatogoryVm>(key);
                if (threeLevldata != null)
                {
                    return Task.FromResult(ResponseResult.Success(threeLevldata));
                }
                else {

                    var thirdQuerySql = $@" select [Key] as [Key] ,[name] as [Value],sort,attach,Depth from [dbo].[KeyValue] where IsValid=1 and type=@Type and depth=3  order by sort ;";
                    var third = _orgUnitOfWork.DbConnection.Query<LevelThreeCatogoryVm>(thirdQuerySql, new DynamicParameters().Set("Type", request.Type)).ToList();
                    _redisClient.Set(key, third, TimeSpan.FromHours(6));
                    return Task.FromResult(ResponseResult.Success(third));
                }

            }
            var data = _redisClient.Get<CatogoryDto>(key);
            if (data != null)
            {
                return Task.FromResult(ResponseResult.Success(data));
            }
            else
            {
                var r = new CatogoryDto() { };
                if (request.Root == 1)
                {
                    var root = new List<RootCatogoryVm>();
                    //热门推荐
                    var hotQuerySql = $@"SELECT  kv.[Key] as [Key] ,kv.[name] as [Value],kv.sort,kv.attach,kv.Depth  from PopularClassify  pc join KeyValue kv on pc.classifykey=kv.[key] and kv.type=@Type where  pc.isvalid=1 order by pc.sort ";
                    var hot = _orgUnitOfWork.DbConnection.Query<LevelThreeCatogoryVm>(hotQuerySql, new DynamicParameters().Set("Type", request.Type)).ToList();
                    if (hot != null && hot.Count > 0)
                    {
                        //推荐
                        var RootCatogoryVm = new RootCatogoryVm() { Value = "推荐", Attach = "https://cos.sxkid.com/images/miniprogram/miniprogram/category-recommend.svg" };
                        //热门分类
                        var HotCatogoryVm = new LevelSecondCatogoryVm() { Value = "热门分类" };
                        HotCatogoryVm.Children = new List<LevelThreeCatogoryVm>();
                        HotCatogoryVm.Children.AddRange(hot);
                        RootCatogoryVm.Children = new List<LevelSecondCatogoryVm>();
                        RootCatogoryVm.Children.Add(HotCatogoryVm);
                        root.Add(RootCatogoryVm);

                    }
                    string firstQuerySql = $@" select [Key] as [Key] ,[name] as [Value],sort,attach,Depth from [dbo].[KeyValue] where IsValid=1 and type=@Type and depth=1 order by sort ;";
                    var first = _orgUnitOfWork.DbConnection.Query<RootCatogoryVm>(firstQuerySql, new DynamicParameters().Set("Type", request.Type)).ToList();
                    if (first != null && first.Count > 0)
                    {
                        root.AddRange(first);

                    }
                    if (root.Count > 0)
                    {
                        foreach (var item in root)
                        {
                            if (item.Key == 0) continue;//人为插入得推荐
                            bool allSecondNoChild = true;//是否所有二级都没有三级
                            var secondQuerySql = $@" select [Key] as [Key] ,[name] as [Value],sort,attach,Depth from [dbo].[KeyValue] where IsValid=1 and type=@Type and depth=2 and parent=@parent order by sort ;";
                            var second = _orgUnitOfWork.DbConnection.Query<LevelSecondCatogoryVm>(secondQuerySql, new DynamicParameters().Set("Type", request.Type).Set("parent", item.Key)).ToList();
                            item.Children = second;
                            if (second != null && second.Count > 0)
                            {

                                foreach (var itemS in second)
                                {
                                    var thirdQuerySql = $@" select [Key] as [Key] ,[name] as [Value],sort,attach,Depth from [dbo].[KeyValue] where IsValid=1 and type=@Type and depth=3 and parent=@parent order by sort ;";
                                    var third = _orgUnitOfWork.DbConnection.Query<LevelThreeCatogoryVm>(thirdQuerySql, new DynamicParameters().Set("Type", request.Type).Set("parent", itemS.Key)).ToList();
                                    if (allSecondNoChild && third.Count > 0)
                                    {
                                        allSecondNoChild = false;
                                    }
                                    itemS.Children = third;
                                }

                            }
                            item.NotShow = allSecondNoChild;

                        }
                        r.RootCatogoryList = root.Where(x => x.NotShow == false).ToList();
                        r.LastUpdateTime = DateTimeExchange.ToUnixTimestampByMilliseconds(DateTime.Now);
                        _redisClient.Set(key,r,TimeSpan.FromHours(6));
                        return Task.FromResult(ResponseResult.Success(r));

                    }

                    else
                    {
                        return Task.FromResult(ResponseResult.Failed("暂无数据"));
                    }

                }
                else if (request.Root == 2)
                {
                    var rootQuerySql = $@" select [Key] as [Key] ,[name] as [Value],sort,attach,Depth from [dbo].[KeyValue] where IsValid=1 and type=@Type and depth=2 order by parent,sort ;";
                    var root = _orgUnitOfWork.DbConnection.Query<RootCatogoryVm>(rootQuerySql, new DynamicParameters().Set("Type", request.Type)).ToList();
                    if (root != null && root.Count > 0)
                    {
                        foreach (var itemS in root)
                        {
                            var thirdQuerySql = $@" select [Key] as [Key] ,[name] as [Value],sort,attach,Depth from [dbo].[KeyValue] where IsValid=1 and type=@Type and depth=3 and parent=@parent order by sort ;";
                            var third = _orgUnitOfWork.DbConnection.Query<LevelSecondCatogoryVm>(thirdQuerySql, new DynamicParameters().Set("Type", request.Type).Set("parent", itemS.Key)).ToList();
                            itemS.Children = third;
                        }
                        r.RootCatogoryList = root;
                        r.LastUpdateTime = DateTimeExchange.ToUnixTimestampByMilliseconds(DateTime.Now);
                        _redisClient.Set(key, r, TimeSpan.FromHours(6));
                        return Task.FromResult(ResponseResult.Success(r));
                    }
                    else
                    {
                        return Task.FromResult(ResponseResult.Failed("暂无数据"));
                    }
                }
             
                return Task.FromResult(ResponseResult.Failed("非法请求"));


            }
        }
    }
}
