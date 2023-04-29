using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Domain.Enum;
using System.Linq;
using iSchool.Organization.Appliaction.Service.Organization;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 保存属性-选项相关信息
    /// </summary>
    public class SavePropertyInfoCommandHandler : IRequestHandler<SavePropertyInfoCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;

        public SavePropertyInfoCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
        }

        public async Task<ResponseResult> Handle(SavePropertyInfoCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var req_PropAndItemData = request.PropertyInfos;//属性选项相关信息集合


            //一、找到课程下所有商品的选项集合
            var goodsItemIdList = _mediator.Send(new QueryGoodsItemIdsByCourseId() { CourseId = request.CourseId }).Result;

            //二、属性排列组合成N个商品属性选项集合+操作状态


            #region 相关sql语句声明
            //各表删除记录的sql语句
            string delSql = "";

            //属性、选项新增及更新的Sql语句
            string intPropItemSql = "";

            //商品表及商品选项关系表新增sql语句  
            string intGoodsItemsSql = "";
            #endregion


            //2.1、增、删属性情况
            var newProp = req_PropAndItemData.Where(_ => _.Operation == OperationTypeEnum.Add.ToInt())?.ToList();
            var delProp = req_PropAndItemData.Where(_ => _.Operation == OperationTypeEnum.Del.ToInt())?.ToList();
            #region 属性数量变化
            if (newProp?.Any() == true || delProp?.Any() == true)
            {
                //2.1.1、属性表、选项表：增则直接插入记录、删则直接删
                //2.1.2、goods表、goodsitem关系表均需删除所有历史记录再重新插入       

                //删除Sql
                delSql = GetDelPropSql(request.CourseId, delProp, req_PropAndItemData);

                //新增属性、选项(含属性、选项的编辑)
                intPropItemSql = GetAddPropSql(request.CourseId, ref req_PropAndItemData);

                //新增商品及商品选项关系
                intGoodsItemsSql = GetProp_Goods_GoodsItem(request.CourseId, req_PropAndItemData);
            }
            #endregion

            #region 属性数量不变
            else
            {
                //2.2、属性数量无变化情况
                //2.2.1、筛查出所有删除的选项并删除对应选项、gonds表、goodsitem关系中相关记录
                //2.2.2、筛选更新状态的属性、选项，直接更新属性名称，选项的名称及sort
                //2.2.3、选项组合成商品的选项集合，有新增项的商品，则需要新增，否则不需要处理


                //删除：获取各表删除记录的sql语句
                delSql = GetDelPropItemSql(req_PropAndItemData);

                //新增选项(含属性、选项的编辑)
                intPropItemSql = GetPropItemSql(request.CourseId, ref req_PropAndItemData);

                //新增商品及商品选项关系
                intGoodsItemsSql = GetItem_Goods_GoodsItem(request.CourseId, goodsItemIdList, req_PropAndItemData);

            }
            #endregion

            #region 事务
            try
            {
                _orgUnitOfWork.BeginTransaction();

                if (!string.IsNullOrEmpty(delSql))
                    _orgUnitOfWork.DbConnection.Execute(delSql, null, _orgUnitOfWork.DbTransaction);

                if (!string.IsNullOrEmpty(intPropItemSql))
                    _orgUnitOfWork.DbConnection.Execute(intPropItemSql, null, _orgUnitOfWork.DbTransaction);

                if (!string.IsNullOrEmpty(intGoodsItemsSql))
                    _orgUnitOfWork.DbConnection.Execute(intGoodsItemsSql, null, _orgUnitOfWork.DbTransaction);

                _orgUnitOfWork.CommitChanges();

                _ = _redisClient.BatchDelAsync(CacheKeys.CourseGoodsProps.FormatWith(request.CourseId), 10);
                return ResponseResult.Success("操作成功");
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return ResponseResult.Failed($"系统错误：【{ex.Message}】");
            }
            #endregion

        }

        #region Del
        /// <summary>
        /// 【属性个数变化】删除属性方法  
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="delProps">待删除属性</param>
        /// <param name="data">前台传入数据</param>
        /// <returns></returns>
        private string GetDelPropSql(Guid courseId, List<PropertyAndItems> delProps, List<PropertyAndItems> data)
        {
            string updatesql = "";

            //1、删属性及其下全部属性
            {
                var str_delPorpId = string.Join("','", delProps?.Select(_ => _.PropertyId)?.ToList());
                if (!string.IsNullOrEmpty(str_delPorpId))
                {
                    updatesql += $@" update [dbo].[CourseProperty] set IsValid=0  where IsValid=1 and id in ('{str_delPorpId}') ;
                    update [dbo].[CoursePropertyItem] set IsValid=0  where IsValid=1 and Propid in ('{str_delPorpId}')  ;
                    ";
                }

            }

            //2、非删除状态的属性，仅删除其下删除状态的选项
            {
                var updateProDelItemId = new List<Guid>();

                var updateProps = data?.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt());
                foreach (var d in updateProps)
                {
                    var delItemIds = d.ProItems.Where(_ => _.Operation == OperationTypeEnum.Del.ToInt()).Select(_ => _.ItemId).ToList();
                    if (delItemIds?.Any() == true) updateProDelItemId.AddRange(delItemIds);
                }
                if (updateProDelItemId.Any())
                    updatesql += $@" update [dbo].[CoursePropertyItem] set IsValid=0  where IsValid=1 and  id in ('{string.Join("','", updateProDelItemId)}');";
            }

            //3、该课程的CourseGoods表和CourseGoodsPropItem历史记录均作废
            {
                //3.1、删该课程下所有商品在goodsitem关系表中的记录
                updatesql += $@" delete [dbo].[CourseGoodsPropItem] where GoodsId in (select id from [dbo].[CourseGoods] where IsValid=1 and Courseid='{courseId}')  ;";

                //3.2、删该课程相关的goods表中所有记录
                updatesql += $@" update [dbo].[CourseGoods] set IsValid=0 where IsValid=1  and Courseid='{courseId}' ;";
            }

            return updatesql;
        }

        /// <summary>
        /// 删除选项方法
        /// </summary>
        /// <param name="data">前台传入数据</param>
        /// <returns></returns>
        private string GetDelPropItemSql(List<PropertyAndItems> data)
        {
            string updatesql = "";

            var delItemIdsList = new List<Guid>();//所有属性下待删除选项Id集
            foreach (var d in data)
            {
                var itemIds = d.ProItems.Where(_ => _.Operation == OperationTypeEnum.Del.ToInt()).Select(_ => _.ItemId)?.ToList();
                if (itemIds.Any() == true) delItemIdsList.AddRange(itemIds);
            }

            if (delItemIdsList.Any() == true)
            {
                var str_itemIds = string.Join("','", delItemIdsList);

                //1、删选项
                updatesql += $@" update [dbo].[CoursePropertyItem] set IsValid=0  where IsValid=1 and id in ('{str_itemIds}') ;";

                //2、删goods表，涉及删除选项的全部记录均删掉
                updatesql += $@" update [dbo].[CourseGoods] set IsValid=0 where IsValid=1 and id in ( select GoodsId from [dbo].[CourseGoodsPropItem] where PropItemId in ('{str_itemIds}') ) ;";

                //3、删goodsitem关系表，涉及删除选项的全部记录均删掉
                updatesql += $@" delete [dbo].[CourseGoodsPropItem] where PropItemId in ('{str_itemIds}') ;";

            }

            return updatesql;
        }
        #endregion

        #region Add
        /// <summary>
        /// 【属性数量变化】情况
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetAddPropSql(Guid courseId, ref List<PropertyAndItems> data)
        {
            string intSql = "";

            foreach (var d in data)
            {
                //1、新增属性，则其下所有选项均为新增项
                if (d.Operation == OperationTypeEnum.Add.ToInt())
                {
                    var newPropId = Guid.NewGuid();
                    //新增属性sql
                    intSql += @$"Insert into [dbo].[CourseProperty]([Id], [Name], [Courseid], [IsValid], [Sort])
                               values('{newPropId}', '{d.PropertyName}', '{courseId}', 1, {d.Sort})
                               ;";
                    d.PropertyId = newPropId;//回填新Id

                    foreach (var item in d.ProItems)//新增选项sql
                    {
                        var newPropItemId = Guid.NewGuid();
                        item.ItemId = newPropItemId;//回填选项Id,用于插入关系表
                        intSql += @$" Insert into [dbo].[CoursePropertyItem]([Id], [Name], [Courseid], [Propid], [IsValid], [Sort])
                                  values('{newPropItemId}','{item.ItemName}','{courseId}','{newPropId}',1,{item.Sort}) 
                                    ;";
                    }
                }
                //2、查询、编辑属性下的新增选项(并更新其属性的name、sort)
                else if (d.Operation == OperationTypeEnum.Update.ToInt() || d.Operation == OperationTypeEnum.Query.ToInt())
                {
                    intSql += @$"update [dbo].[CourseProperty] set [Name]='{d.PropertyName}',Sort={d.Sort}  where id='{d.PropertyId}'
                              ;";
                    foreach (var item in d.ProItems)
                    {
                        if (item.Operation == OperationTypeEnum.Add.ToInt())//新增选项
                        {
                            var newPropItemId = Guid.NewGuid();
                            item.ItemId = newPropItemId;//回填选项Id,用于插入关系表
                            intSql += @$" Insert into [dbo].[CoursePropertyItem]([Id], [Name], [Courseid], [Propid], [IsValid], [Sort])
                                  values('{newPropItemId}','{item.ItemName}','{courseId}','{d.PropertyId}',1,{item.Sort}) 
                                    ;";
                        }
                        else if (item.Operation == OperationTypeEnum.Update.ToInt() || item.Operation == OperationTypeEnum.Query.ToInt())//更新编辑选项的name、sort
                        {
                            intSql += @$" update [dbo].[CoursePropertyItem] set [Name]='{item.ItemName}',Sort={item.Sort}  where id='{item.ItemId}';";
                        }
                    }
                }
            }

            return intSql;
        }

        /// <summary>
        /// 【属性数量不变化】的情况
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetPropItemSql(Guid courseId, ref List<PropertyAndItems> data)
        {
            string intSql = "";
            //编辑属性下的新增选项(并更新其属性的name、sort)
            foreach (var d in data)
            {
                if (d.Operation == OperationTypeEnum.Update.ToInt() || d.Operation == OperationTypeEnum.Query.ToInt())
                {
                    intSql += @$"update [dbo].[CourseProperty] set [Name]='{d.PropertyName}',Sort={d.Sort}  where id='{d.PropertyId}'
                              ;";
                    foreach (var item in d.ProItems)
                    {
                        if (item.Operation == OperationTypeEnum.Add.ToInt())//新增选项
                        {
                            var newPropItemId = Guid.NewGuid();
                            item.ItemId = newPropItemId;//回填选项Id,用于插入关系表
                            intSql += @$" Insert into [dbo].[CoursePropertyItem]([Id], [Name], [Courseid], [Propid], [IsValid], [Sort])
                                  values('{newPropItemId}','{item.ItemName}','{courseId}','{d.PropertyId}',1,{item.Sort}) 
                                    ;";
                        }
                        else if (item.Operation == OperationTypeEnum.Update.ToInt() || item.Operation == OperationTypeEnum.Query.ToInt())//更新编辑选项name、sort
                        {
                            intSql += @$" update [dbo].[CoursePropertyItem] set [Name]='{item.ItemName}',Sort={item.Sort}  where id='{item.ItemId}';";
                        }
                    }
                }
            }
            return intSql;
        }
        #endregion

        #region goods&goodsItem
        /// <summary>
        /// 【属性个数变化】 goods表和goodsitems关系表的新增记录sql语句
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="data">前端传入数据</param>
        /// <returns></returns>
        private string GetProp_Goods_GoodsItem(Guid courseId, List<PropertyAndItems> data)
        {
            string sql = "";

            //获取除删除外的属性信息
            var d = data.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();//待组合属性

            {
                //一、待组合属性个数=1
                if (d.Count == 1)
                {
                    //待组合选项
                    var items1 = d[0].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    foreach (var i1 in items1)
                    {
                        var goodsId = Guid.NewGuid();
                        sql += $@" Insert into [dbo].[CourseGoods]([Id], [Courseid], [Stock], [Price], [Count], [Sellcount], [Show], [IsValid])
Values('{goodsId}','{courseId}',0,0,0,0,1,1) ;";
                        sql += $@" Insert into [dbo].[CourseGoodsPropItem]([Id], [GoodsId], [PropItemId])
values
 (NEWID(),'{goodsId}','{i1.ItemId}') 
;";
                    }
                }
            }
            {
                //二、待组合属性个数=2
                if (d.Count == 2)
                {
                    //待组合选项
                    var items1 = d[0].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    var items2 = d[1].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    foreach (var i1 in items1)
                    {
                        foreach (var i2 in items2)
                        {
                            var goodsId = Guid.NewGuid();
                            sql += $@" Insert into [dbo].[CourseGoods]([Id], [Courseid], [Stock], [Price], [Count], [Sellcount], [Show], [IsValid])
Values('{goodsId}','{courseId}',0,0,0,0,1,1) ;";
                            sql += $@" Insert into [dbo].[CourseGoodsPropItem]([Id], [GoodsId], [PropItemId])
values
 (NEWID(),'{goodsId}','{i1.ItemId}') 
,(NEWID(),'{goodsId}','{i2.ItemId}') 
;";
                        }
                    }
                }
            }
            {
                //三、待组合属性个数=3
                if (d.Count == 3)
                {
                    //待组合选项
                    var items1 = d[0].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    var items2 = d[1].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    var items3 = d[2].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    foreach (var i1 in items1)
                    {
                        foreach (var i2 in items2)
                        {
                            foreach (var i3 in items3)
                            {
                                var goodsId = Guid.NewGuid();
                                sql += $@" Insert into [dbo].[CourseGoods]([Id], [Courseid], [Stock], [Price], [Count], [Sellcount], [Show], [IsValid])
Values('{goodsId}','{courseId}',0,0,0,0,1,1) ;";
                                sql += $@" Insert into [dbo].[CourseGoodsPropItem]([Id], [GoodsId], [PropItemId])
values
 (NEWID(),'{goodsId}','{i1.ItemId}') 
,(NEWID(),'{goodsId}','{i2.ItemId}') 
,(NEWID(),'{goodsId}','{i3.ItemId}') 
;";
                            }
                        }
                    }
                }
            }
            return sql;
        }

        /// <summary>
        /// 【属性个数不变】 goods表和goodsitems关系表的新增记录sql语句
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <param name="dbData">库中商品信息</param>
        /// <param name="data">前端传入数据</param>
        /// <returns></returns>
        private string GetItem_Goods_GoodsItem(Guid courseId, List<GoodsItemIds> dbData, List<PropertyAndItems> data)
        {
            string sql = "";

            //获取除删除外的属性信息
            var d = data.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();//待组合属性

            {
                //一、待组合属性个数=1
                if (d.Count == 1)
                {
                    //待组合选项
                    var items1 = d[0].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    foreach (var i1 in items1)
                    {
                        if (i1.Operation == OperationTypeEnum.Add.ToInt())//若选项新增，则新增商品
                        {
                            var goodsId = Guid.NewGuid();
                            sql += $@" Insert into [dbo].[CourseGoods]([Id], [Courseid], [Stock], [Price], [Count], [Sellcount], [Show], [IsValid])
                        Values('{goodsId}','{courseId}',0,0,0,0,1,1) ;";
                            sql += $@" Insert into [dbo].[CourseGoodsPropItem]([Id], [GoodsId], [PropItemId])
                        values
                         (NEWID(),'{goodsId}','{i1.ItemId}') 
                        ;";
                        }
                    }
                }
            }
            {
                //二、待组合属性个数=2
                if (d.Count == 2)
                {
                    //待组合选项
                    var items1 = d[0].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    var items2 = d[1].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    foreach (var i1 in items1)
                    {
                        foreach (var i2 in items2)
                        {
                            if (i1.Operation == OperationTypeEnum.Add.ToInt() || i2.Operation == OperationTypeEnum.Add.ToInt())
                            {
                                var goodsId = Guid.NewGuid();
                                sql += $@" Insert into [dbo].[CourseGoods]([Id], [Courseid], [Stock], [Price], [Count], [Sellcount], [Show], [IsValid])
                Values('{goodsId}','{courseId}',0,0,0,0,1,1) ;";
                                sql += $@" Insert into [dbo].[CourseGoodsPropItem]([Id], [GoodsId], [PropItemId])
                values
                 (NEWID(),'{goodsId}','{i1.ItemId}') 
                ,(NEWID(),'{goodsId}','{i2.ItemId}') 
                ;";
                            }
                        }
                    }
                }
            }
            {
                //三、待组合属性个数=3
                if (d.Count == 3)
                {
                    //待组合选项
                    var items1 = d[0].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    var items2 = d[1].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    var items3 = d[2].ProItems.Where(_ => _.Operation != OperationTypeEnum.Del.ToInt()).ToList();
                    foreach (var i1 in items1)
                    {
                        foreach (var i2 in items2)
                        {
                            foreach (var i3 in items3)
                            {
                                if (i1.Operation == OperationTypeEnum.Add.ToInt()
                                    || i2.Operation == OperationTypeEnum.Add.ToInt()
                                    || i3.Operation == OperationTypeEnum.Add.ToInt())
                                {
                                    var goodsId = Guid.NewGuid();
                                    sql += $@" Insert into [dbo].[CourseGoods]([Id], [Courseid], [Stock], [Price], [Count], [Sellcount], [Show], [IsValid])
                Values('{goodsId}','{courseId}',0,0,0,0,1,1) ;";
                                    sql += $@" Insert into [dbo].[CourseGoodsPropItem]([Id], [GoodsId], [PropItemId])
                values
                 (NEWID(),'{goodsId}','{i1.ItemId}') 
                ,(NEWID(),'{goodsId}','{i2.ItemId}') 
                ,(NEWID(),'{goodsId}','{i3.ItemId}') 
                ;";
                                }
                            }
                        }
                    }
                }
            }
            return sql;
        }


        #endregion
    }
}

