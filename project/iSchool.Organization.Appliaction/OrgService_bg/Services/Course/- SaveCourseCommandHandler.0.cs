using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Organization;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    public class SaveCourseCommand0Handler : IRequestHandler<SaveCourseCommand0, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;
        IConfiguration _config;
        SmLogUserOperation _smLogUserOperation;

        public SaveCourseCommand0Handler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IConfiguration _config, SmLogUserOperation smLogUserOperation,
            IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
            this._config = _config;
            this._smLogUserOperation = smLogUserOperation;
        }

        public async Task<ResponseResult> Handle(SaveCourseCommand0 request, CancellationToken cancellationToken)
        {
            var canNewUserReward = (request.NewUserRewardValue ?? -1) > 0 && request.NewUserRewardType > 0;
            var str_freightBacklist = request.FreightBacklist == null ? null : request.FreightBacklist.ToJsonString();

            if ((request.Banner.IsNullOrEmpty() || request.Banner == "[]") && (request.Videos.IsNullOrEmpty() || request.Videos == "[]"))
            {
                return ResponseResult.Failed("必须录入一个视频或商品banner图");
            }
            //
            // 商品分类
            if (!string.IsNullOrEmpty(request.CommodityTypes))
            {
                var ctys = request.CommodityTypes.ToObject<int[]>().Distinct();
                foreach (var cty in ctys)
                {
                    try
                    {
                        var d = await _mediator.Send(new OrgService_bg.RequestModels.BgMallFenleisLoadQuery { Code = cty, ExpandMode = 1 });
                        if (d?.Selected_d3?.Code != cty) throw new Exception("不是3级分类");
                    }
                    catch (Exception ex)
                    {
                        throw new CustomResponseException("保存失败." + ex.Message);
                    }
                }
                request.CommodityTypes = ctys.ToJsonString();
            }


            // find oldata
            var course0 = default(Domain.Course);
            var skus0 = default(Domain.CourseGoods[]);
            if (!request.IsAdd)
            {
                var sql = "select * from Course where Id=@Id";
                course0 = await _orgUnitOfWork.QueryFirstOrDefaultAsync<Domain.Course>(sql, new { request.Id });

                sql = "select * from CourseGoods where IsValid=1 and courseid=@Id";
                skus0 = (await _orgUnitOfWork.QueryAsync<Domain.CourseGoods>(sql, new { request.Id })).AsArray();
            }

            try
            {
                var oldCount = _orgUnitOfWork.DbConnection.Query<int>(
                    $"SELECT count(1) FROM [Organization].[dbo].[Course] where IsValid=1 and orgid=@OrgId and title=@Title and id<>@Id"
                    , new { request.OrgId, request.Title, request.Id }
                ).FirstOrDefault();
                if (oldCount > 0)
                {
                    return ResponseResult.Failed("同一个机构下，不允许课程名称重复，请重新填写！");
                }
                //课程价格-百分比-价格与返现金额校验
                if (request.CashbackType == CashbackTypeEnum.Percent.ToInt() && request.CashbackValue > 100)
                {
                    return ResponseResult.Failed("分销自购返现比例不能超过100%，请重新填写！");
                }

                #region 大课相关校验
                string insertBigs = "";
                var intDy = new DynamicParameters();
                intDy.Set("time", DateTime.Now)
                     .Set("opuser", request.UserId)
                     .Set("Courseid", request.Id);
                for (int i = 0; i < request.BigCourseList?.Count; i++)
                {
                    var bigC = request.BigCourseList[i];
                    //1、大课名称重复校验
                    var isExitsSameName = _orgUnitOfWork.DbConnection.Query<int>(" select count(1) from [dbo].[BigCourse] where IsValid=1 and Title=@title  and Courseid<>@courseId "
                        , new DynamicParameters().Set("title", bigC.Title).Set("courseId", request.Id)).FirstOrDefault() > 0 ? true : false;
                    if (isExitsSameName)
                    {
                        return ResponseResult.Failed($"大课{i + 1}的名称不允许重复，请重新填写！");
                    }


                    if (!string.IsNullOrEmpty(bigC.Title))//课程不为空
                    {
                        //2、大课价格与返现金额校验
                        if (bigC.CashbackType == CashbackTypeEnum.Percent.ToInt() && bigC.CashbackValue > 100)
                            return ResponseResult.Failed($"大课{i + 1}的返现比例不能超过100%，请重新填写！");
                        else if (bigC.CashbackType == CashbackTypeEnum.Yuan.ToInt() && bigC.CashbackValue > bigC.Price)
                            return ResponseResult.Failed($"大课{i + 1}的返现金额不能超过大课价格，请重新填写！");

                        //组装sql
                        intDy.Set($"time{i}", DateTime.Now);
                        insertBigs += $@"insert into [dbo].[BigCourse] 
(Id, Courseid, Title, Price, StartTime, EndTime, Condition, CashbackType, CashbackValue,PJCashbackType, PJCashbackValue,[IsBonusRate], [HeadFxUserExclusiveValue], [HeadFxUserExclusiveType],[Desc], CreateTime, Creator, ModifyDateTime, Modifier, IsValid)
values(NEWID(), @Courseid, @Title{i}, @Price{i}, @StartTime{i}, @EndTime{i},@Condition{i}, @CashbackType{i}, @CashbackValue{i},@PJCashbackType{i}, @PJCashbackValue{i},@IsBonusRate{i},@HeadFxUserExclusiveValue{i},@HeadFxUserExclusiveType{i},@desc{i}, @time{i}, @opuser, @time{i}, @opuser, 1) ;";
                        var desc = string.IsNullOrEmpty(bigC.Desc) ? "" : bigC.Desc;
                        intDy
                        .Set($"Title{i}", bigC.Title)
                        .Set($"Price{i}", bigC.Price)
                        .Set($"StartTime{i}", bigC.StartTime)
                        .Set($"EndTime{i}", bigC.EndTime)
                        .Set($"Condition{i}", bigC.Condition)
                        .Set($"CashbackType{i}", bigC.CashbackType)
                        .Set($"CashbackValue{i}", bigC.CashbackValue)
                        .Set($"PJCashbackType{i}", bigC.PJCashbackType)
                        .Set($"PJCashbackValue{i}", bigC.PJCashbackValue)
                        .Set($"IsBonusRate{i}", bigC.IsBonusRate)
                        .Set($"HeadFxUserExclusiveValue{i}", bigC.HeadFxUserExclusiveValue)
                        .Set($"HeadFxUserExclusiveType{i}", bigC.HeadFxUserExclusiveType)
                        .Set($"Desc{i}", desc)
                       ;
                    }

                }

                #endregion

                decimal? lowestPrice = null; // 课程价格取其所有商品中的最低价
                decimal? lowestOrigPrice = null; // 所有商品中的最低价对应的原价

                var dy = new DynamicParameters();

                string updateGoodsSql = "";
                //更新商品的价格、原来价格、显示状态、数量(库存+销量)               
                if (request.UpdateGoodsInfos?.Any() == true)
                {
                    var _sql = @"select g.id as goodsId,p.id from CourseGoods g 
                        join CourseGoodsPropItem i on i.goodsid=g.id 
                        join CoursePropertyItem p on p.id=i.PropItemId and p.IsValid=1
                        where g.id in @goodsids ";
                    var lsPItems = await _orgUnitOfWork.QueryAsync<(Guid GoodsId, Guid PItemId)>(_sql, new { goodsids = request.UpdateGoodsInfos.Select(_ => _.GoodsId) });

                    for (var i = 0; i < request.UpdateGoodsInfos.Count; i++)
                    {
                        var goods = request.UpdateGoodsInfos[i];

                        request.Stock += goods.Stock;
                        var origPrice = goods.OrigPrice == null ? "origPrice=null" : $"origPrice={goods.OrigPrice}";
                        var supplierId = goods.SupplierId == null ? "SupplierId=null" : $"SupplierId='{goods.SupplierId}'";
                        var str_supplieAddressId = goods.SupplieAddressId == null ? "SupplieAddressId=null" : $"SupplieAddressId='{goods.SupplieAddressId}'";
                        updateGoodsSql +=
                            $" update CourseGoods set {supplierId},{str_supplieAddressId}, Price={goods.Price},{origPrice},LimitedBuyNum={goods.LimitedBuyNum},Show={goods.Show},Count={goods.Stock}+Sellcount,[Cover]=@goods_cover_{i},[articleNo]=@goods_articleNo_{i},[costprice]=@goods_costprice_{i} "
                            + $"where IsValid=1 and Id='{goods.GoodsId}'; "
                            ;

                        dy.Set($"goods_articleNo_{i}", goods.ArticleNo);
                        dy.Set($"goods_costprice_{i}", goods.Costprice);
                        dy.Set($"goods_cover_{i}", goods.Cover);
                        dy.Set($"goods_PItemId_{i}", lsPItems.FirstOrDefault(_ => _.GoodsId == goods.GoodsId).PItemId);
                    }
                }
                //课程价格-元-价格与返现金额校验
                if (request.CashbackType == CashbackTypeEnum.Yuan.ToInt() && request.CashbackValue > lowestPrice)
                {
                    return ResponseResult.Failed("分销自购返现金额不能超过商品最低价格，请重新填写！");
                }


                // lowestPrice and lowestOrigPrice
                {
                    // 所有商品中的最低价 
                    var ls0 = request.UpdateGoodsInfos.Where(_ => _.Show == 1);
                    lowestPrice = ls0.Any() ? ls0.Min(_ => _.Price) : 0;

                    // 所有商品中的最低价对应的原价
                    var ls1 = request.UpdateGoodsInfos.Where(_ => _.Price == lowestPrice && _.OrigPrice != null && _.Show == 1);
                    lowestOrigPrice = ls1.Any() ? ls1.Min(_ => _.OrigPrice) : null;
                }

                dy.Set("courseid", request.Id)
                   .Set("contenttype", (int)AutoOnlineOrOffContentType.Course)
                   .Set("orgid", request.OrgId)
                   .Set("name", request.Name)
                   .Set("banner", request.Banner != "[]" ? request.Banner : request.VideoCovers)
                   .Set("banner_s", request.Banner_s != "[]" ? request.Banner_s : request.VideoCovers)
                   .Set("title", request.Title)
                   .Set("subtitle", request.SubTitle)
                   .Set("mode", request.Mode)
                   .Set("Subjects", request.Type == CourseTypeEnum.Course.ToInt() ? request.Subjects : null)
                   .Set("GoodthingTypes", request.Type == CourseTypeEnum.Goodthing.ToInt() ? request.GoodthingTypes : null)
                   .Set("CommodityTypes", request.CommodityTypes)
                   .Set("Type", request.Type)
                   .Set("duration", request.Duration)
                   .Set("price", lowestPrice)//课程价格为商品的最低价格
                   .Set("OrigPrice", lowestOrigPrice)
                   .Set("detail", request.Detail)
                   .Set("subject", request.Subject)
                   .Set("count", request.Stock)//课程表数量，新增时数量=库存
                   .Set("time", DateTime.Now)
                   .Set("UserId", request.UserId)
                   .Set("IsValid", true)
                   .Set("minage", request.MinAge)
                   .Set("maxage", request.MaxAge)
                   .Set("IsInvisibleOnline", request.IsInvisibleOnline)
                   .Set("IsExplosions", request.IsExplosions)
                   .Set("IsSystemCourse", request.Type == CourseTypeEnum.Goodthing.ToInt() ? null : request.IsSystemCourse)
                   .Set("VideoCovers", request.VideoCovers)
                   .Set("Videos", request.Videos)
                   .Set("CanNewUserReward", canNewUserReward)
                   .Set("NewUserExclusive", request.NewUserExclusive)
                   .Set("LimitedTimeOffer", request.LimitedTimeOffer)
                   .Set("SetTop", request.SetTop)
                   .Set("BlackList", str_freightBacklist)
                   ;

                var delCacheKeys = new List<string>()
                    {
                         $"org:course:courseid:{request.Id}"
                        ,$"org:organization:orgid:{request.OrgId}"
                        ,$"org:course:courseid:{request.Id}:*"//更新课程时间需要清除该缓存
                        ,$"org:organization:orgid:{request.OrgId}:*"
                        , "org:course:skuid:*"
                        ,"org:courses:*"
                        ,"org:evlt:info:*"//评测详情有机构信息、课程信息、专题信息
                        ,"org:*:relatedEvlts:*"//pc评测详情-相关评测s、pc课程详情-相关评测s、pc机构详情-相关评测s
                        //,$"org:course:courseid:{request.Id}:*:relatedcourses"//pc课程详情-机构(相关)课程s
                        ,$"org:organization:orgid:*:relatedcourses"//pc机构详情-机构(相关)课程s,
                        ,"org:course:excellentcourses"//首页精选课程
                        , "org:course:*"
                    };

                string sql = "";//课程            
                string onSql = "";//自动上架            
                string offSql = "";//自动下架

                #region 新增课程
                if (request.IsAdd)//新增如果设置自动上下架时间，则需要插入自动山下架表
                {
                    sql = $@" insert into  [dbo].[Course] ([id], [orgid], [name], [banner],[banner_s], [title], [subtitle], [status], [mode],[Subjects]
,[GoodthingTypes],[CommodityTypes],[Type],[duration], [price], [OrigPrice], [detail], [subject] ,[CreateTime],[Creator],[IsValid], [minage], [maxage],[LastOnShelfTime]
,[LastOffShelfTime],[count],[IsInvisibleOnline],[IsExplosions],[IsSystemCourse],[VideoCovers],[Videos],[CanNewUserReward],[NewUserExclusive],[LimitedTimeOffer],[SetTop],[BlackList])
values(@courseid, @orgid, @name, @banner,@banner_s, @title, @subtitle, @status, @mode,@Subjects,@GoodthingTypes,@CommodityTypes,@Type,@duration, 
@price, @OrigPrice, @detail, @subject,@time,@UserId,@IsValid, @minage, @maxage,@LastOnShelfTime,@LastOffShelfTime,@count
,@IsInvisibleOnline,@IsExplosions,@IsSystemCourse,@VideoCovers,@Videos,@CanNewUserReward,@NewUserExclusive,@LimitedTimeOffer,@SetTop,@BlackList)
;";


                    #region 自动上架/立即上架
                    DateTime? lastOnTime = null;
                    if (request.LastOnShelfTime == null)//立即上架
                    {
                        dy.Add("@status", (int)CourseStatusEnum.Ok);//课程状态上架
                        delCacheKeys.Add($"org:organization:orgid:{request.OrgId}:*:counts:cource");//pc单个机构计数-课程数量
                        onSql = "";
                    }
                    else//自动上架
                    {
                        dy.Add("@status", (int)CourseStatusEnum.Fail);//课程状态下架
                        lastOnTime = request.LastOnShelfTime;
                        onSql += $@" update [dbo].[AutoOnlineOrOff] set IsValid=0 where contentid=@courseid and planstatus=@onstatus;
                            insert into [dbo].[AutoOnlineOrOff]([id], [contentid], [contenttype], [planstatus], [plantime], [CreateTime])
                            values(NEWID(), @courseid, @contenttype, @onstatus, @ontime,GETDATE())  ;";
                        dy.Add("@onstatus", (int)CourseStatusEnum.Ok);
                        dy.Add("@ontime", request.LastOnShelfTime);
                    }
                    #endregion

                    #region 自动下架/不自动下架
                    DateTime? lastOffTime = null;
                    if (request.LastOffShelfTime != null)//自动下架
                    {
                        lastOffTime = ((DateTime)request.LastOffShelfTime).AddHours(23).AddMinutes(59).AddSeconds(59);
                        offSql += $@" update [dbo].[AutoOnlineOrOff] set IsValid=0 where contentid=@courseid and planstatus=@offstatus;
                            insert into [dbo].[AutoOnlineOrOff]([id], [contentid], [contenttype], [planstatus], [plantime], [CreateTime])
                            values(NEWID(), @courseid, @contenttype, @offstatus, @offtime,GETDATE())  ;";
                        dy.Add("@offstatus", (int)CourseStatusEnum.Fail);
                        dy.Add("@offtime", lastOffTime);
                    }
                    else//不自动下架
                    {
                        offSql = "";
                    }

                    #endregion


                    dy.Add("@LastOnShelfTime", lastOnTime);
                    dy.Add("@LastOffShelfTime", lastOffTime);
                }
                #endregion

                #region 修改课程
                else//编辑
                {
                    // 库存后续更新
                    // count=@stock,
                    sql = $@"update [dbo].[Course] set orgid=@orgid, [name]=@name, banner=@banner,banner_s=@banner_s, title=@title, subtitle=@subtitle, mode=@mode,duration=@duration
 ,price=@price, OrigPrice=@OrigPrice, detail=@detail, [subject]=@subject, ModifyDateTime=@time,Modifier=@UserId,minage=@minage, maxage=@maxage,count=({request.Stock}+sellcount)
,Subjects=@Subjects,GoodthingTypes=@GoodthingTypes,[CommodityTypes]=@CommodityTypes,Type=@Type,IsInvisibleOnline=@IsInvisibleOnline,IsExplosions=@IsExplosions,IsSystemCourse=@IsSystemCourse,LastOffShelfTime=null,LastOnShelfTime=null
,VideoCovers=@VideoCovers,Videos=@Videos,[CanNewUserReward]=@CanNewUserReward,[NewUserExclusive]=@NewUserExclusive,[LimitedTimeOffer]=@LimitedTimeOffer,[SetTop]=@SetTop,[BlackList]=@BlackList
  where id=@courseid
 ;";
                    //-- 佣金锁定期类型为具体日期, 修改才能设置自动下架时间
                    //* 2021-11开始 上下架跟佣金没关系了

                    // 自动上架
                    DateTime? lastOnTime = null;
                    {
                        onSql += $" update [dbo].[AutoOnlineOrOff] set IsValid=0 where contentid=@courseid and planstatus={(int)CourseStatusEnum.Ok}; ";
                        if (request.AutoOnShelfDate_Edit != null)
                        {
                            lastOnTime = request.AutoOnShelfDate_Edit.Value.Date;
                            onSql += $@" 
                                insert into [dbo].[AutoOnlineOrOff]([id], [contentid], [contenttype], [planstatus], [plantime], [CreateTime])
                                    values(NEWID(), @courseid, @contenttype, {(int)CourseStatusEnum.Ok},@_lastOnTime, GETDATE()) ;
                            ";
                            dy.Add("@_lastOnTime", lastOnTime);
                            sql += " update [dbo].[Course] set LastOnShelfTime=@_lastOnTime where id=@courseid ; ";
                        }
                    }

                    // 自动下架历史记录全部update isvalid=0，并重新设置自动下架时间
                    DateTime? lastOffTime = null;
                    {
                        offSql += $@" update [dbo].[AutoOnlineOrOff] set IsValid=0 where contentid=@courseid and planstatus=@offstatus; ";
                        dy.Add("@offstatus", (int)CourseStatusEnum.Fail);
                        //if (request.NolimitType == NolimitTypeEnum.ExactDate.ToInt() && request.AutoOffShelfDate_Edit != null)//自动下架
                        if (request.AutoOffShelfDate_Edit != null)//自动下架
                        {
                            lastOffTime = ((DateTime)request.AutoOffShelfDate_Edit).AddHours(23).AddMinutes(59).AddSeconds(59);
                            offSql += $@" insert into [dbo].[AutoOnlineOrOff]([id], [contentid], [contenttype], [planstatus], [plantime], [CreateTime])
                         values(NEWID(), @courseid, @contenttype, @offstatus, @offtime,GETDATE())  ;";
                            dy.Add("@offtime", lastOffTime);
                            sql += @$" update [dbo].[Course] set LastOffShelfTime=@offtime where id=@courseid ;";
                        }
                    }

                    delCacheKeys.Add(CacheKeys.CourseExtend.FormatWith(request.No));
                }
                #endregion

                #region 购前须知，先全部清除，再重新插入
                string noticesSql = " update [dbo].[CourseNotices] set IsValid=0 where CourseId=@courseid and IsValid=1;";
                if (request.ListNotices?.Any() == true)
                {
                    for (int i = 0; i < request.ListNotices.Count; i++)
                    {
                        noticesSql += $@" Insert into [dbo].[CourseNotices](Id, CourseId, Title, Content, IsValid, Sort)
 values(NEWID(), @courseid, @Title_{i}, @Content_{i}, 1, @Sort_{i}); ";
                        dy.Set($"Title_{i}", request.ListNotices[i].Title);
                        dy.Set($"Content_{i}", request.ListNotices[i].Content);
                        dy.Set($"Sort_{i}", i);
                    }
                    delCacheKeys.Add(CacheKeys.CourseExtend.FormatWith("*"));
                }
                #endregion

                #region 运费
                var sql_freight = "\n update [dbo].[CourseFreight] set IsValid=0 where CourseId=@courseid and IsValid=1;" + "\n";
                if (request.Freights?.Length > 0)
                {
                    for (var i = 0; i < request.Freights.Length; i++)
                    {
                        sql_freight += $"insert CourseFreight(id,CourseId,Type,Name,Citys,Cost,CreateTime,Creator,ModifyDateTime,Modifier,IsValid) "
                            + $"values(newid(),@courseid,@frType{i},@frName{i},@frCitys{i},@frCost{i},getdate(),@UserId,getdate(),@UserId,1)"
                            + "\n";
                        dy.Set($"frType{i}", request.Freights[i].Type);
                        dy.Set($"frName{i}", request.Freights[i].Names?.ToJsonString());
                        dy.Set($"frCitys{i}", request.Freights[i].Citys?.ToJsonString());
                        dy.Set($"frCost{i}", request.Freights[i].Cost);
                    }
                }
                #endregion 运费

                _orgUnitOfWork.BeginTransaction();
                var count = _orgUnitOfWork.DbConnection.Execute(sql + onSql + offSql + updateGoodsSql + noticesSql + sql_freight, dy, _orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.CommitChanges();

                if (count >= 1)
                {
                    #region 新增/编辑课程成功之后，更新机构表中的科目
                    if (request.IsAdd)//新增
                    {
                        await _mediator.Send(new UpdateOrgSubjectByCourseCommand() { CourseId = request.Id, NewSubject = request.Subject, OperationType = 1, OrgId = request.OrgId, OldOrgId = null, OldSubject = null });
                    }
                    else//编辑
                    {
                        await _mediator.Send(new UpdateOrgSubjectByCourseCommand() { CourseId = request.Id, NewSubject = request.Subject, OperationType = 2, OrgId = request.OrgId, OldOrgId = request.OldOrgId, OldSubject = request.OldSubject });
                    }
                    #endregion

                    #region 更新体验课关联大课信息的信息                    
                    //先把体验课关联的大课全部update isvalid=0,再插入一条新记录
                    string updateBig = $@" update [dbo].[BigCourse] set IsValid=0,ModifyDateTime=@time,Modifier=@opuser where Courseid=@Courseid  and IsValid=1  ;";

                    _orgUnitOfWork.BeginTransaction();
                    _orgUnitOfWork.DbConnection.Execute(updateBig + insertBigs, intDy, _orgUnitOfWork.DbTransaction);
                    _orgUnitOfWork.CommitChanges();

                    #endregion

                    #region 课程分销规则
                    //先把课程历史分销的规则全部update isvalid=0,,再插入一条新记录
                    string updateDrp = $@" update [dbo].[CourseDrpInfo] set IsValid=0,ModifyDateTime=@time,Modifier=@opuser where IsValid=1 and Courseid=@Courseid ";
                    string insertDrp = $@"Insert into [dbo].[CourseDrpInfo]
(Id, Courseid, CashbackType, CashbackValue,PJCashbackType, PJCashbackValue, [IsBonusRate], [HeadFxUserExclusiveValue], [HeadFxUserExclusiveType], NolimitType, NolimitAfterDate, NolimitAfterBuyInDays, NewUserRewardType,NewUserRewardValue, CreateTime, Creator, ModifyDateTime, Modifier, IsValid,ReceivingAfterDays)
values(NEWID(),@Courseid, @CashbackType, @CashbackValue,@PJCashbackType, @PJCashbackValue, @IsBonusRate, @HeadFxUserExclusiveValue, @HeadFxUserExclusiveType, @NolimitType, @NolimitAfterDate, @NolimitAfterBuyInDays, @NewUserRewardType,@NewUserRewardValue, @time, @opuser, @time, @opuser,1,@ReceivingAfterDays)";
                    var drpDy = new DynamicParameters()
                        .Set("Courseid", request.Id)
                        .Set("CashbackType", request.CashbackType)
                        .Set("CashbackValue", request.CashbackValue)
                        .Set("IsBonusRate", request.CashbackValue > 0 ? 1 : request.IsBonusRate)//自购返现数值大于0，则工资系数计算自动勾选
                        //
                        //!! 2021-12-27 屏蔽 上线独享 和 平级佣金
                        //.Set("PJCashbackType", request.PJCashbackType)
                        //.Set("PJCashbackValue", request.PJCashbackValue)
                        //.Set("HeadFxUserExclusiveValue", request.HeadFxUserExclusiveValue)
                        //.Set("HeadFxUserExclusiveType", request.HeadFxUserExclusiveType)
                        .Set("PJCashbackType", 0)
                        .Set("PJCashbackValue", 0)
                        .Set("HeadFxUserExclusiveValue", 0)
                        .Set("HeadFxUserExclusiveType", 0)
                        //
                        .Set("NolimitType", request.NolimitType)
                        .Set("NolimitAfterDate", request.NolimitType == NolimitTypeEnum.ExactDate.ToInt() ? request.NolimitAfterDate : null)
                        .Set("NolimitAfterBuyInDays", request.NolimitType == NolimitTypeEnum.NDaysLater.ToInt() ? request.NolimitAfterBuyInDays : null)
                        .Set("ReceivingAfterDays", request.ReceivingAfterDays)
                        .Set("NewUserRewardType", (!canNewUserReward ? (int?)null : request.NewUserRewardType))
                        .Set("NewUserRewardValue", (!canNewUserReward ? (decimal?)null : request.NewUserRewardValue))
                        .Set("time", DateTime.Now)
                        .Set("opuser", request.UserId);
                    _orgUnitOfWork.BeginTransaction();
                    _orgUnitOfWork.DbConnection.Execute(updateDrp + insertDrp, drpDy, _orgUnitOfWork.DbTransaction);
                    _orgUnitOfWork.CommitChanges();
                    #endregion

                    // （暂时新增和编辑都逐个更新库存）更新库存
                    if (request.UpdateGoodsInfos?.Any() == true)
                    {
                        foreach (var goods in request.UpdateGoodsInfos)
                        {
                            await _mediator.Send(new CourseGoodsStockRequest
                            {
                                BgSetStock = new BgSetGoodsStockCommand { Id = goods.GoodsId, StockCount = goods.Stock }
                            });
                        }
                    }

                    #region 清除API那边相关的缓存
                    await _mediator.Send(new ClearRedisCacheCmd { Keys = delCacheKeys, ExecSec = 120, WaitSec = 3 });
                    #endregion

                    // add user log
                    {
                        _smLogUserOperation.SetUserId(request.UserId)
                            .SetClass(nameof(SaveCourseCommand0))
                            .SetMethod(request.IsAdd ? "add" : "update")
                            .SetParams("_", request)
                            .SetParams("courseid", request.Id)                       
                            .SetTime(DateTime.Now);

                        if (!request.IsAdd)
                        {
                            _smLogUserOperation.SetOldata("course", course0);
                            _smLogUserOperation.SetOldata("coursegoods", skus0);
                        }
                    }

                    return ResponseResult.Success("操作成功");
                }
                else
                {
                    return ResponseResult.Failed("操作失败");
                }
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.SafeRollback();
                return ResponseResult.Failed($"系统错误：【{ex.Message}】");
            }

        }
    }
}
