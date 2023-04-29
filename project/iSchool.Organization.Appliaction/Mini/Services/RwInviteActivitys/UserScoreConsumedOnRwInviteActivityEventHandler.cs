using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.CommonHelper;
using System.Net.Http;
using System.Diagnostics;

namespace iSchool.Organization.Appliaction.Services
{
    public class UserScoreConsumedOnRwInviteActivityEventHandler : INotificationHandler<UserScoreConsumedOnRwInviteActivityEvent>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        IHttpClientFactory _httpClientFactory;
        NLog.ILogger _log;

        public UserScoreConsumedOnRwInviteActivityEventHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IHttpClientFactory httpClientFactory, NLog.ILogger log,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._httpClientFactory = httpClientFactory;
            this._log = log;
        }

        public async Task Handle(UserScoreConsumedOnRwInviteActivityEvent e, CancellationToken cancellation)
        {
            string sql = null;
            try
            {
                var rwgids = await _redis.SMembersAsync<Guid>(CacheKeys.RwInviteActivity_InvisibleOnlineCourses);
                if (rwgids.Length < 1) throw new CustomResponseException("no rwgids in cache", 404);

                //
                // rw活动目前是直接购买
                var goodsInfo = e.GoodsInfo;
                if (e.GoodsInfo == null)
                {
                    try { goodsInfo = await _mediator.Send(new CourseGoodsSimpleInfoByIdQuery { GoodsId = e.GoodsId, AllowNotValid = true, NeedCourse = true }); } catch { }
                    if (goodsInfo == null)
                        throw new CustomResponseException("非法操作", Consts.Err.CourseGoodsOffline);
                }
                e.GoodsId = goodsInfo.Id;
                e.GoodsInfo = goodsInfo;

                if (!rwgids.Contains(goodsInfo.CourseId))
                {
                    _log.Info(_log.GetNLogMsg(nameof(UserScoreConsumedOnRwInviteActivityEventHandler))
                        .SetUserId(e.UnionID_dto?.UserId ?? e.UserId)
                        .SetParams(e)
                        .SetLevel("info")
                        .SetContent("不是rw活动"));
                    return;
                }

                //
                // unionID_dto
                var unionID_dto = e.UnionID_dto;
                if (unionID_dto == null)
                {
                    unionID_dto = await _mediator.Send(new GetUserSxbUnionIDQuery { UserId = e.UserId });
                    if (unionID_dto == null)
                        throw new CustomResponseException("用户没UnionID", Consts.Err.OrderCreate_UserHasNoUnionID);
                }
                e.UserId = unionID_dto.UserId;
                e.UnionID_dto = unionID_dto;

                //
                // courseExchangeType
                sql = "select top 1 * from CourseExchange where CourseId=@CourseId and IsValid=1";
                var curr_courseExchange = await _orgUnitOfWork.QueryFirstOrDefaultAsync<CourseExchange>(sql, new { goodsInfo.CourseId });
                if (curr_courseExchange == null) throw new CustomResponseException("无积分配置", Consts.Err.OrderCreate_CourseExchangeIsNull);

                var courseExchangeType = (CourseExchangeTypeEnum)curr_courseExchange.Type;

                sql = $"select * from CourseExchange where CourseId in @CourseIds and IsValid=1 and {"Creator=@Creator".If(curr_courseExchange.Creator != null)}";
                var ces = (await _orgUnitOfWork.QueryAsync<CourseExchange>(sql, new { CourseIds = rwgids, curr_courseExchange.Creator })).AsList();
                if (ces.Count == 0) throw new CustomResponseException("无积分配置", Consts.Err.OrderCreate_CourseExchangeIsNull);

                ces.RemoveAll(_ => _.Type != curr_courseExchange.Type);
                if (ces.Count == 0) throw new CustomResponseException($"没有{courseExchangeType.GetDesc()}的积分配置", Consts.Err.OrderCreate_CourseExchangeIsNull);

                ces.RemoveAll(courseExchange => (courseExchange.StartTime != null && DateTime.Now < courseExchange.StartTime)
                    || (courseExchange.EndTime != null && courseExchange.EndTime <= DateTime.Now));
                if (ces.Count == 0) throw new CustomResponseException($"积分配置都过期了", Consts.Err.OrderCreate_CourseExchangeIsNull);

                // get score
                var score = (await _mediator.Send(new UserScoreOnRwInviteActivityArgs { UnionID = unionID_dto.UnionID }
                    .SetCourseExchangeType(courseExchangeType)
                    .Consume(0)
                    )).GetResult<double>();
                score = score < 0 ? 0 : score;

                //
                // wx客服消息
                //
                var strmsg = $@"
恭喜您已经兑换{goodsInfo._Course.Title}商品
（如未支付，积分将在30分钟后归还）
当前剩余{score}积分[太阳]

";
                if (score > 0)
                {
                    var strmsg1 = "";
                    foreach (var courseExchange in ces)
                    {
                        if (courseExchange.Point.HasValue && courseExchange.Point.Value > score) continue;
                        // 够扣积分的才显示
                        try
                        {
                            var course = await _mediator.Send(new CourseBaseInfoQuery { CourseId = courseExchange.CourseId });
                            strmsg1 += $"\n{course.Title} 关键词：【{courseExchange.Keywords?.ToObject<string[]>()?.FirstOrDefault()}】";
                        }
                        catch { }
                    }
                    if (strmsg1 != "") strmsg += "可兑换" + strmsg1 + "\n";
                }
                strmsg += @"
如需兑换其他产品，可继续邀请好友哟~
PS：同一产品可多次兑换[加油]
";
                strmsg = strmsg.Trim();
                //
                var openid = await _orgUnitOfWork.QueryFirstOrDefaultAsync<string>($@"
                    select top 1 openID from [iSchoolUser].[dbo].[openid_weixin] where valid=1 and userID=@UserId; 
                ", new { e.UserId });
                if (string.IsNullOrEmpty(openid))
                {
                    throw new CustomResponseException($"用户({e.UserId})没openid,可能没关注公众号.", 404);
                }
                //
                var gzhAppName = _config["AppSettings:SxbWxGzhAppName"];
                var accessTokenInfo = await _mediator.Send(new GetWxGzhAccessTokenQuery { GzhAppName = gzhAppName });
                var msgUrl = "https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token=" + accessTokenInfo.Token;
                //
                using var http = _httpClientFactory.CreateClient(string.Empty);
                //
                // 正式要跟正式公众号48小时有交互才能发送成功, 否则会报 45015 'response out of time limit or subscription is canceled rid'
                var r = await new HttpApiInvocation(HttpMethod.Post, msgUrl, _log)
                    .SetApiDesc("rw活动积分消耗提醒-wx客服消息")
                    .SetBodyByJson(new
                    {
                        touser = openid,
                        msgtype = "text",
                        text = new { content = strmsg },
                    })
                    .SetResBodyParser(json =>
                    {
                        var jtk = JToken.Parse(json);
                        var code = (int?)jtk["errcode"] ?? 0;
                        if (code == 0) return ResponseResult<bool>.Success(true);
                        var r = ResponseResult<bool>.Failed(jtk["errmsg"]?.ToString());
                        r.status = (ResponseCode)code;
                        return r;
                    })
                    .InvokeByAsync<bool>(http);

                Debugger.Break();
            }
            catch (CustomResponseException ex)
            {
                _log.Error(_log.GetNLogMsg(nameof(UserScoreConsumedOnRwInviteActivityEventHandler))
                    .SetUserId(e.UnionID_dto?.UserId ?? e.UserId)
                    .SetParams(e)
                    .SetLevel("错误")
                    .SetContent("rw活动积分消耗提醒-wx客服消息")
                    .SetError(ex, ex.Message, ex.ErrorCode));
            }
        }

    }
}
