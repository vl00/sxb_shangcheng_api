using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 课程详情
    /// </summary>
    public class CourseDetailsByIdQueryHandler : IRequestHandler<CourseDetailsByIdQuery, ResponseResult>
    {
        IUserInfo _userInfo;
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient cSRedis;
        const int time = 60 * 60;//cache timeout
        UserUnitOfWork userUnitOfWork;
        private readonly IMediator _mediator;

        public CourseDetailsByIdQueryHandler(IOrgUnitOfWork unitOfWork, IUserUnitOfWork userUnitOfWork
            , IHttpClientFactory httpClientFactory
            , IUserInfo userInfo
            , CSRedisClient cSRedis,
            IMediator mediator)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.userUnitOfWork = (UserUnitOfWork)userUnitOfWork;
            _userInfo = userInfo;
            this.cSRedis = cSRedis;
            this._mediator = mediator;
        }

        public async Task<ResponseResult> Handle(CourseDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            #region get guid id
            string id_key = CacheKeys.courseidbyno.FormatWith(request.No);
            request.CourseId = cSRedis.Get<Guid>(id_key);
            if (request.CourseId == default)
            {
                string id_sql = $" select id from [dbo].[Course] where IsValid=1 and no=@no and status=1 ;";
                request.CourseId = _orgUnitOfWork.Query<Guid>(id_sql, new DynamicParameters().Set("no", request.No)).FirstOrDefault();

                cSRedis.Set(id_key, request.CourseId, 60 * 60 * 24 * 365);
            }

            #endregion
            string key = string.Format(CacheKeys.CourseDetails, request.CourseId);
            var data = cSRedis.Get<CourseDetailsResponse>(key);
            if (data != null)
            {
                //var userSql = "select top 1 *  from [userinfo] where id=@UserId and mobile is not null";
                //var user = userUnitOfWork.QueryFirstOrDefault<userinfo>(userSql, new { _userInfo.UserId });
                ////返回手机号码
                //data.LoginUserMobile = user?.Mobile;
            }
            else
            {
                var dy = new DynamicParameters();
                dy.Add("@CourseId", request.CourseId);
                dy.Add("@No", request.No);
                #region 课程详情查询
                string sql = $@" 
                   select c.CanNewUserReward,fx.NewUserRewardValue,fx.NewUserRewardType, c.videos,c.videocovers,c.type,c.subjects,c.subject,c.price,c.Origprice,c.sellcount
                    ,(SELECT MIN(Origprice) FROM CourseGoods WHERE 
                    Courseid = c.id AND SHOW = 1 AND IsValid = 1
                    AND EXISTS(SELECT 1 FROM CourseGoodsExchange WHERE CourseGoods.Id = CourseGoodsExchange.GoodId AND CourseGoodsExchange.Show = 1 AND CourseGoodsExchange.IsValid = 1) ) PointsMinOrigprice
                    ,c.id,c.no,c.isPointExchange,c.name as cName,c.banner,c.title,c.subtitle,c.Detail,c.subscribe,c.LastOffShelfTime,c.NewUserExclusive,c.LimitedTimeOffer,c.IsInvisibleOnline,
                    org.id as orgId,org.logo,org.name as orgName,org.authentication,org.no as OrgNoId,org.[desc],org.subdesc,c.BlackList
                   from [dbo].[Course] c    left join [dbo].[CourseDrpInfo] fx on c.id=fx.Courseid  left join [dbo].[Organization] org on c.orgid=org.id  and c.IsValid=1
                   where org.IsValid=1 and c.status=1 and org.status=1  and c.id=@CourseId and c.no=@No  ";


                var dBData = _orgUnitOfWork.Query<CourseDetailsDB>(sql, dy).FirstOrDefault();

                if (dBData == null) throw new CustomResponseException("课程不存在！", 404);
                else
                {
                    data = new CourseDetailsResponse()
                    {
                        Type = dBData.Type,
                        Title = dBData.Title,
                        Authentication = dBData.Authentication,
                        Banner = dBData.Banner == null ? null : JsonSerializationHelper.JSONToObject<List<string>>(dBData.Banner),
                        CName = dBData.CName,
                        Detail = dBData.Detail,
                        Id = dBData.Id,
                        IsPointExchange = dBData.IsPointExchange,
                        Logo = dBData.Logo,
                        OrgName = dBData.OrgName,
                        Subtitle = dBData.Subtitle,
                        OrgId = dBData.OrgId,
                        OrgNoId = UrlShortIdUtil.Long2Base32(Convert.ToInt64(dBData.OrgNoId)),
                        Desc = dBData.Desc,
                        SubDesc = dBData.SubDesc,
                        Price = dBData.Price,
                        Origprice = dBData.Origprice,
                        PointsMinOrigprice = dBData.PointsMinOrigprice,
                        Subject = dBData.Subject ?? SubjectEnum.Other.ToInt(),
                        Video = dBData.Videos == null ? null : JsonSerializationHelper.JSONToObject<List<string>>(dBData.Videos),
                        VideoCovers = dBData.VideoCovers == null ? null : JsonSerializationHelper.JSONToObject<List<string>>(dBData.VideoCovers),
                        CanNewUserReward = dBData.CanNewUserReward,
                        NewUserRewardAmount = Math.Round((dBData.NewUserRewardType == (byte)CashbackTypeEnum.Percent ? (dBData.NewUserRewardValue * dBData.Price / 100m) :
                            dBData.NewUserRewardValue), 2, MidpointRounding.ToZero),
                        NewUserExclusive = dBData.NewUserExclusive,
                        LimitedTimeOffer = dBData.LimitedTimeOffer,
                        LastOffShelfTime = dBData.LastOffShelfTime,
                        IsInvisibleOnline = dBData.IsInvisibleOnline ?? false,
                        FreightBlackList = dBData.BlackList?.ToObject<int[]>()?.Select(_ => new NameCodeDto<int> { Code = _ }).ToList() ?? new List<NameCodeDto<int>>(),
                    };
                    data.SubjectDesc = EnumUtil.GetDesc((SubjectEnum)data.Subject);
                    data.Subjects = dBData.Subjects?.ToObject<int[]>() is int[] subjs && subjs.Length > 0 ? subjs : new[] { SubjectEnum.Other.ToInt() };
                    data.SubjectDescs = data.Subjects.Select(subj => EnumUtil.GetDesc((SubjectEnum)subj)).ToArray();
                    data.Id_s = UrlShortIdUtil.Long2Base32(dBData.No);
                }

                #endregion

                #region 优质评测
                string evalIdsql = $@"select id as evaluationid,stick,viewcount,EvltNoId from(
                                    select * from(select top 1 1 as rownum, e.id,e.no as EvltNoId,stick,viewcount from  [dbo].[Evaluation] e left join [dbo].[EvaluationBind] eb  on e.id=eb.evaluationid  and eb.IsValid=1 and e.IsValid=1 where eb.courseid=@CourseId  and e.status=1 and e.IsValid=1  order by CreateTime desc,stick desc)T1
                                    UNION 
                                    select * from(select  top 1 2 as rownum, e.id,e.no as EvltNoId,0 as stick,viewcount from  [dbo].[Evaluation] e left join [dbo].[EvaluationBind] eb  on e.id=eb.evaluationid  and eb.IsValid=1 and e.IsValid=1 where eb.courseid=@CourseId  and e.status=1 and e.IsValid=1  order by viewcount desc)T2
                                    )TT order by rownum;";

                var listEvaluationInfoByIdDB = _orgUnitOfWork.Query<EvaluationInfoByIdDB>(evalIdsql, dy).ToList();
                if (listEvaluationInfoByIdDB != null && listEvaluationInfoByIdDB.Count > 0)
                {
                    Guid? evlId = null;
                    string evltNo = "";
                    foreach (var item in listEvaluationInfoByIdDB)
                    {
                        if (item.Stick)
                        {
                            evlId = item.EvaluationId;
                            evltNo = UrlShortIdUtil.Long2Base32(Convert.ToInt64(item.EvltNoId));
                            break;
                        }
                        else if (item.ViewCount > 0)
                        {
                            evlId = item.EvaluationId;
                            evltNo = UrlShortIdUtil.Long2Base32(Convert.ToInt64(item.EvltNoId));
                            break;
                        }
                    }
                    if (evlId != null)
                    {
                        dy.Add("@EvaluationId", evlId);
                        string evalsql = $@" select item.evaluationid,item.content,item.thumbnails,item.pictures from [dbo].[EvaluationItem] item where evaluationid=@EvaluationId  and IsValid=1 ";
                        var ret = _orgUnitOfWork.Query<EvaluationInfoDB>(evalsql, dy).FirstOrDefault();
                        if (ret != null)
                        {
                            EvaluationInfo info = new EvaluationInfo();
                            info.EvaluationId = ret.EvaluationId;
                            info.Content = ret.Content;
                            info.EvltNoId = evltNo;
                            info.Pictures = string.IsNullOrEmpty(ret.Pictures) ? null : JsonSerializationHelper.JSONToObject<List<string>>(ret.Pictures);
                            info.Thumbnails = string.IsNullOrEmpty(ret.Thumbnails) ? null : JsonSerializationHelper.JSONToObject<List<string>>(ret.Thumbnails);
                            data.EvaluationInfo = info;
                        }
                    }

                }
                #endregion


                if (_userInfo.IsAuthenticated)//已登录，则查看收藏状态
                {
                    var u_c_status_key = CacheKeys.MyCollectionCourse.FormatWith(_userInfo.UserId, request.CourseId);
                    var collectionStatus = cSRedis.Get<bool?>(u_c_status_key);
                    if (collectionStatus == null)
                    {
                        string c_sql = $" select count(1) from [dbo].[Collection] where dataID=@CourseId and userID=@UserId and IsValid=1 ";
                        var c_count = _orgUnitOfWork.Query<int>(c_sql, new DynamicParameters()
                            .Set("CourseId", request.CourseId)
                            .Set("UserId", _userInfo.UserId)).FirstOrDefault();
                        data.IsCurrentUserCollection = c_count > 0 ? true : false;
                        cSRedis.Set(u_c_status_key, data.IsCurrentUserCollection, time);
                    }
                    else
                    {
                        data.IsCurrentUserCollection = (bool)collectionStatus;
                    }


                }

                cSRedis.Set(key, data, time);

                //var userSql = "select top 1 *  from [userinfo] where id=@UserId and mobile is not null";
                //var user = userUnitOfWork.QueryFirstOrDefault<userinfo>(userSql, new { _userInfo.UserId });
                ////返回手机号码
                //data.LoginUserMobile = user?.Mobile;

            }

            if (data.LastOffShelfTime != null)
            {
                data.LastOffShelfTime = DateTime.Now >= data.LastOffShelfTime ? null : data.LastOffShelfTime;
            }

            // seo
            var description = HtmlHelper.NoHTML(data.Detail);
            data.Tdk_d = description.Length > 160 ? description[0..160] : description;

            // IsHeadFxUser
            if (_userInfo.IsAuthenticated)
            {
                // call api
                data.IsHeadFxUser = (await _mediator.Send(new CheckIsFxHeadQuery { UserId = _userInfo.UserId })).IsHead;

                if (data.IsHeadFxUser)
                {
                    var drpfx = await _mediator.Send(new GetCourseDrpFxInfoQuery { CourseId = data.Id });
                    if (drpfx.RewardList?.Any() != true && drpfx.BigCourseInfo == null)
                        data.IsHeadFxUser = false;
                }

                //用户添加浏览历史
                var logKey = CacheKeys.CourseVisitLog.FormatWith(_userInfo.UserId);
                var log = await cSRedis.GetAsync<List<CourseVisitLog>>(logKey);
                if (null == log)
                {
                    var val = new List<CourseVisitLog>();
                    val.Add(new CourseVisitLog() { CourseId = request.CourseId, AddTime = DateTime.Now });
                    cSRedis.Set(logKey, val);
                }
                else
                {
                    log.Add(new CourseVisitLog() { CourseId = request.CourseId, AddTime = DateTime.Now });
                    log = log.OrderByDescending(x => x.AddTime).ToList();
                    //只保留5条
                    if (log.Count >= 5)
                    {
                        log = log.Take(5).ToList();

                    }

                    cSRedis.Set(logKey, log);


                }
            }

            if (request.AllowRecordPV)
            {
                // 增加PV
                AsyncUtils.StartNew(new PVisitEvent { CttId = data.Id, UserId = _userInfo.UserId, Now = DateTime.Now, CttType = PVisitCttTypeEnum.Course });
            }

            // 是否rw微信群拉新买隐形上架商品活动的商品
            if (data.IsInvisibleOnline)
            {
                data.IsRwInviteActivity = await cSRedis.SIsMemberAsync(CacheKeys.RwInviteActivity_InvisibleOnlineCourses, data.Id.ToString());
            }

            // 购物车数量
            if (_userInfo.IsAuthenticated)
            {
                data.CartCount = (int)(await cSRedis.HLenAsync(CacheKeys.ShoppingCart.FormatWith(_userInfo.UserId)));
            }

            //运费
            var fsql = "select * from CourseFreight where CourseId=@CourseId and IsValid=1 ORDER BY Type";
            var freights = await _orgUnitOfWork.QueryAsync<CourseFreight>(fsql, new { CourseId = request.CourseId });
            if (null != freights)
            {
                var freightList = new List<CFreightM>();
                foreach (var item in freights)
                {
                   var  addM= new CFreightM()
                    {
                        Area = EnumUtil.GetDesc((FreightAreaTypeEnum)item.Type),
                        Cost = item.Cost.Value
                    };
                    if (null != item.Citys)
                        addM.CityCode = JsonConvert.DeserializeObject<List<string>>(item.Citys);
                    if (null != item.Name)
                        addM.CityName = JsonConvert.DeserializeObject<List<string>>(item.Name);
                    if (item.Type == (int)FreightAreaTypeEnum.RemoteAreas)
                    {
                        addM.CityName = new List<string>() { "内蒙古", "辽宁", "吉林", "黑龙江", "西藏",
                        "甘肃","青海","宁夏","新疆"
                        };
                    }
                    freightList.Add(addM);
                }
                data.FreightList = freightList;
            }

            for (var __ = data.FreightBlackList?.Count > 0; __; __ = !__)
            {
                if (data.FreightBlackList.Any(_ => _.Code == 0))
                {
                    data.FreightBlackList = new List<NameCodeDto<int>> { new NameCodeDto<int> { Code = 0, Name = "全国" } };
                    continue;
                }

                var ls = _orgUnitOfWork.Query<(int Id, string Name)>("select id,name from CityArea where IsValid=1 and id in @ids ",
                    new { ids = data.FreightBlackList.Select(_ => _.Code).ToArray() });

                foreach (var fo in data.FreightBlackList)
                {
                    if (!ls.TryGetOne(out var _i, _ => _.Id == fo.Code)) continue;
                    fo.Name = _i.Name;
                }
            }

            return ResponseResult.Success(data);
        }

    }





}
