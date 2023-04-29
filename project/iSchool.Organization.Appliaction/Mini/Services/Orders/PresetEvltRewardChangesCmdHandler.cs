using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class PresetEvltRewardChangesCmdHandler : IRequestHandler<PresetEvltRewardChangesCmd>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient redis;
        IConfiguration _config;
        NLog.ILogger _log;

        public PresetEvltRewardChangesCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IConfiguration config,
            NLog.ILogger log,
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.redis = redis;
            this._config = config;
            this._log = log;
        }

        public async Task<Unit> Handle(PresetEvltRewardChangesCmd cmd, CancellationToken cancellation)
        {
            if (cmd.OrderId == default && cmd.OrderDetail == null) return default;
            var order = cmd.OrderDetail;
            if (order == null)
            {
                order = await _mediator.Send(new OrderDetailQuery { OrderId = cmd.OrderId });
            }

            LogDebug(order.UserId, "find order", cmd);

            var startTime = DateTime.Parse(_config["AppSettings:EvltReward:StartTime"]);
            var endTime = DateTime.Parse(_config["AppSettings:EvltReward:EndTime"]);
            if (order.UserPayTime == null || order.UserPayTime < startTime || order.UserPayTime > endTime)
            {
                return default;
            }

            var ls_EvaluationReward = new List<EvaluationReward>();
            var payTime = order.UserPayTime ?? DateTime.Now;

            // user is 新用户 | 顾问 普通用户
            var userId = order.UserId;              // 下单人userid 
            var user_IsFxAdviser = cmd.IsFxAdviser; // 下单人是否fx顾问
            // 顾问的未使用的推广订单带来的种草机会
            var ls_FxAdviserNotUsedTgEvltRewards = default(List<EvaluationReward>);             
            // 本订单商品里的网课数量
            var c1count = order.Prods.Where(_ => _.ProdType == CourseTypeEnum.Course.ToInt()).Count();
            
            // 是否(课程)新用户
            var user_IsNewbuyer = c1count > 0 ? (await _mediator.Send(new UserIsCourseTypeNewBuyerQuery
            {
                UserId = userId,
                ExcludedOrderIds = new[] { order.OrderId },
                CourseType = CourseTypeEnum.Course,
            })).IsNewBuyer : (bool?)null;

            foreach (var courseOrderProdItemDto in order.Prods.OfType<CourseOrderProdItemDto>())
            {
                var price = (double)courseOrderProdItemDto.Price;
                var course = await _mediator.Send(new CourseBaseInfoQuery { CourseId = courseOrderProdItemDto.Id, AllowNotValid = true });
                if (!course.IsValid)
                {
                    LogError(userId, "购买成功以后计算对应的种草获奖机会时发现course下架", course, null);
                    continue;
                }
                if (course.IsInvisibleOnline == true)
                {
                    // 2021-10-13 沈叔叔要求隐形商品不算种草机会
                    // 
                    LogDebug(userId, "购买成功以后计算对应的种草获奖机会时发现是隐形商品不算机会", new { courseOrderProdItemDto, course });
                    continue;
                }
                // 课程
                if (courseOrderProdItemDto.ProdType == CourseTypeEnum.Course.ToInt())
                {
                    // course.CanEvltReward 自(己)购(买) 受限
                    // 成为别人(顾问)的推广单 不受限

                    // 新用户
                    for (var __ = user_IsNewbuyer == true && course.CanEvltReward && !ls_EvaluationReward.Any(_ => _.IsNewbuy == true); __; __ = !__)
                    {
                        var conditions = _config.GetSection("AppSettings:EvltReward:CourseBonus:newbuyer:bonus").GetChildren();
                        var condition = conditions.FirstOrDefault(_ => MathInterval.Parse(_.Key).Contains(price));
                        if (condition == null) break;

                        // for insert
                        var dto = new EvaluationReward { IsNewbuy = true };
                        ls_EvaluationReward.Add(dto);
                        dto.UserId = userId;
                        /* dto.Id = Guid.NewGuid(); //后续填id */
                        dto.IsValid = true;
                        dto.Used = false;
                        dto.OrgId = courseOrderProdItemDto.OrgInfo.Id;
                        dto.CourseId = courseOrderProdItemDto.Id;
                        dto.GoodsId = courseOrderProdItemDto.GoodsId;
                        dto.OrderId = order.OrderId;
                        dto.Creator = dto.Modifier = userId;
                        dto.CreateTime = payTime.AddSeconds(1);
                        dto.ModifyDateTime = DateTime.Now;
                        //dto.Reward = decimal.Parse(condition.Value);
                    }

                    // 本单maybe成为上级顾问的推广订单
                    for (var __ = cmd.FxHeadUserId != null; __; __ = !__)
                    {
                        var condition = _config.GetSection("AppSettings:EvltReward:CourseBonus:fxhead:subuser").Value;
                        if (!MathInterval.Parse(condition).Contains(price)) break;

                        // for insert
                        for (var i = 0; i < courseOrderProdItemDto.BuyCount; i++)
                        {
                            var dto = new EvaluationReward { IsNewbuy = false };
                            ls_EvaluationReward.Add(dto);
                            dto.UserId = cmd.FxHeadUserId!.Value;
                            /* dto.Id = Guid.NewGuid(); //后续填id */
                            dto.IsValid = true;
                            dto.Used = false;
                            dto.TgOrderId = order.OrderId;
                            dto.TgOrderDetialId = courseOrderProdItemDto.OrderDetailId;
                            dto.Creator = dto.Modifier = userId;
                            dto.CreateTime = payTime.AddSeconds(1);
                            dto.ModifyDateTime = DateTime.Now;
                        }
                    }

                    // 顾问自购
                    for (var __ = user_IsFxAdviser && course.CanEvltReward; __; __ = !__)
                    {
                        var conditions = _config.GetSection("AppSettings:EvltReward:CourseBonus:fxhead:self").GetChildren();
                        var condition = conditions.FirstOrDefault(_ => MathInterval.Parse(_.Key).Contains(price));
                        if (condition == null) break;
                        if (c1count < 1) break;

                        ls_FxAdviserNotUsedTgEvltRewards ??= await GetFxAdviserNotUsedTgEvltRewards(userId, payTime.AddSeconds(1), c1count);
                        if (ls_FxAdviserNotUsedTgEvltRewards.Count < 1) break;

                        // for update
                        for (var i = 0; i < courseOrderProdItemDto.BuyCount; i++)
                        {
                            if (ls_FxAdviserNotUsedTgEvltRewards.Count < 1) break;
                            var dto = ls_FxAdviserNotUsedTgEvltRewards[0];
                            ls_FxAdviserNotUsedTgEvltRewards.RemoveAt(0);
                            ls_EvaluationReward.Add(dto);
                            dto.UserId = userId;
                            dto.IsValid = true;
                            dto.Used = false;
                            dto.OrgId = courseOrderProdItemDto.OrgInfo.Id;
                            dto.CourseId = courseOrderProdItemDto.Id;
                            dto.GoodsId = courseOrderProdItemDto.GoodsId;
                            dto.OrderId = order.OrderId;
                            dto.Modifier = userId;
                            dto.ModifyDateTime = DateTime.Now;
                            //dto.Reward = decimal.Parse(condition.Value);
                        }
                    }
                }
                // 好物
                else if (courseOrderProdItemDto.ProdType == CourseTypeEnum.Goodthing.ToInt())
                {
                    for (var __ = true; __; __ = !__)
                    {
                        var conditions = _config.GetSection("AppSettings:EvltReward:GoodThingBonus").GetChildren();
                        var condition = conditions.FirstOrDefault(_ => MathInterval.Parse(_.Key).Contains(price));
                        if (condition == null) break;

                        // for insert
                        for (var i = 0; i < courseOrderProdItemDto.BuyCount; i++)
                        {
                            var dto = new EvaluationReward { IsNewbuy = null };
                            ls_EvaluationReward.Add(dto);
                            dto.UserId = userId;
                            /* dto.Id = Guid.NewGuid(); //后续填id */                            
                            dto.IsValid = true;
                            dto.Used = false;
                            dto.OrgId = courseOrderProdItemDto.OrgInfo.Id;
                            dto.CourseId = courseOrderProdItemDto.Id;
                            dto.GoodsId = courseOrderProdItemDto.GoodsId;
                            dto.OrderId = order.OrderId;
                            dto.Creator = dto.Modifier = userId;
                            dto.CreateTime = payTime.AddSeconds(1);
                            dto.ModifyDateTime = DateTime.Now;
                            //dto.Reward = decimal.Parse(condition.Value);
                        }
                    }
                }
            }

            // add or update
            if (ls_EvaluationReward.Count > 0)
            {
                var ls4up = ls_EvaluationReward.Where(_ => _.Id != default).ToArray();
                var ls4add = ls_EvaluationReward.Where(_ => _.Id == default).Select(_ => 
                {
                    _.Id = Guid.NewGuid();
                    return _;
                }).ToArray();

                _orgUnitOfWork.BeginTransaction();
                try
                {
                    if (ls4add.Length > 0)
                    {
                        LogDebug(order.UserId, "do add", ls4add);
                        await _orgUnitOfWork.DbConnection.InsertAsync(ls4add, _orgUnitOfWork.DbTransaction);
                    }
                    if (ls4up.Length > 0)
                    {
                        LogDebug(order.UserId, "do up", ls4up);
                        await _orgUnitOfWork.DbConnection.UpdateAsync(ls4up, _orgUnitOfWork.DbTransaction);
                    }
                    _orgUnitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    _orgUnitOfWork.SafeRollback();
                    LogError(userId, "购买成功以后对应的种草会有获奖机会", cmd, ex);
                }
            }
            return default;
        }

        /// <summary>
        /// 顾问的未使用的推广订单带来的种草机会
        /// </summary>
        private async Task<List<EvaluationReward>> GetFxAdviserNotUsedTgEvltRewards(Guid userId, DateTime naf, int count)
        {
            var sql = $@"
select top {count} er.* from EvaluationReward er
where er.IsValid=1 and er.userid=@userId and er.CreateTime<@naf
and er.CourseId is null and er.TgOrderDetialId is not null
order by er.CreateTime asc
";
            var ls = await _orgUnitOfWork.QueryAsync<EvaluationReward>(sql, new { userId, naf });
            return ls.AsList();
        }

        void LogError(Guid userid, string errdesc, object obj, Exception ex, int errcode = 500)
        {
            LogError(userid, errdesc, obj?.ToJsonString(camelCase: true), ex, errcode);
        }

        void LogError(Guid userid, string errdesc, string paramsStr, Exception ex, int errcode = 500)
        {
            if (_log != null)
            {
                var msg = _log.GetNLogMsg(nameof(PresetEvltRewardChangesCmdHandler))
                    .SetUserId(userid)
                    .SetParams(paramsStr)
                    .SetLevel("错误")
                    .SetError(ex, errdesc, errcode);
                msg.Properties["Class"] = nameof(PresetEvltRewardChangesCmdHandler);                
                _log.Error(msg);
            }
        }

        void LogDebug(Guid userid, string msg, object args)
        {
            if (_log != null)
            {
                var m = _log.GetNLogMsg(nameof(PresetEvltRewardChangesCmdHandler))
                    .SetUserId(userid)
                    .SetParams(args)
                    .SetContent(msg)
                    .SetLevel("debug");
                m.Properties["Class"] = nameof(PresetEvltRewardChangesCmdHandler);
                _log.Info(m);
            }
        }
    }
}
