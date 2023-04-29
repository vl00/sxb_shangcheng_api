using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class GetCourseDrpFxInfoQueryHandler : IRequestHandler<GetCourseDrpFxInfoQuery, GetCourseDrpFxInfoDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;        
        IMapper _mapper;
        IConfiguration _config;
        IUserInfo me;

        public GetCourseDrpFxInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config, IUserInfo me,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;            
            this._redis = redis;            
            this._mapper = mapper;
            this._config = config;
            this.me = me;
        }

        public async Task<GetCourseDrpFxInfoDto> Handle(GetCourseDrpFxInfoQuery query, CancellationToken cancellation)
        {
            var result = new GetCourseDrpFxInfoDto { CourseId = query.CourseId };
            await default(ValueTask);

            var courseInfo = await _mediator.Send(new CourseBaseInfoQuery { CourseId = query.CourseId });
            result.Title = courseInfo.Title;

            // 课程推广奖励
            do
            {
                //积分奖励
                var integralInfo= await _mediator.Send(new GetCourseIntegralSimpleInfoQuery { CourseId = query.CourseId });
                result.IntegralInfo = integralInfo;
                var drpinfo = await _mediator.Send(new GetCourseFxSimpleInfoQuery { CourseId = query.CourseId });

                // sku佣金列表
                var rwls = new List<CourseDrpFxRewardLsItemDto>();
                await foreach (var item in GetRewardList(query.CourseId))
                {
                    // 自购返现数值为0 不在前端显示
                    if (item == null) continue; 
                    //if (item.CashbackMoney <= 0) continue; 
                    //
                    rwls.Add(item);
                }
                result.RewardList = rwls;

                result.ReceivingAfterDays = drpinfo?.ReceivingAfterDays ?? result.ReceivingAfterDays;
                result.Condition1ConsumedMoneys = int.TryParse(_config["AppSettings:drpfx_upgrade2headfx:condition1:consumedMoneys"], out var _c1cm) ? _c1cm : default;
            }
            while (false);

            // 大课信息                        
            {
                var sql = "select * from BigCourse where IsValid=1 and CourseId=@CourseId";
                result.BigCourseInfos = (await _orgUnitOfWork.QueryAsync<BigCourseDrpFxInfoDto>(sql, new { query.CourseId })).AsArray();
                foreach (var bigCourseInfo in result.BigCourseInfos)
                {
                    bigCourseInfo.CashbackTypeDesc = ((CashbackTypeEnum)bigCourseInfo.CashbackType).GetDesc();
                    bigCourseInfo.Desc = string.IsNullOrEmpty(bigCourseInfo.Desc) ? null : bigCourseInfo.Desc;

                    var big_CashbackValue = bigCourseInfo.CashbackValue ?? 0;
                    var big_CashbackType = bigCourseInfo.CashbackType;
                    bigCourseInfo.CashbackMoney = Math.Round((big_CashbackValue == 0 ? 0 :
                            big_CashbackType == (byte)CashbackTypeEnum.Percent ? (big_CashbackValue * bigCourseInfo.Price / 100m) :
                            big_CashbackValue), 2, MidpointRounding.ToZero);
                }
            }

            // 课程推广列表为空,前端不显示
            if (result.RewardList?.Any() != true) result.RewardList = null;


            // 返回数值没填,前端不显示该大课信息
            // 大课列表为空,前端不显示
            {
                var barr = result.BigCourseInfos.Where(_ => (_.CashbackValue ?? -1) >= 0).ToArray();
                result.BigCourseInfos = barr.Length > 0 ? barr : null;
                result.BigCourseInfo = barr.Length > 0 ? barr[0] : null;
            }

            // 判断是否高级顾问
            if (me.IsAuthenticated)
            {
                try
                {
                    var headInfo = ((await _mediator.Send(new ApiDrpFxRequest { Ctn = new ApiDrpFxRequest.GetConsultantRateQry { UserId = me.UserId } }))
                        .Result as ApiDrpFxResponse.GetConsultantRateQryResult);

                    if (headInfo?.IsHighConsultant == true)
                    {
                        result.IsHighHead = true;
                        if (result.RewardList != null)
                        {
                            foreach (var rewa in result.RewardList)
                                rewa.MgrMoney = fmt_money(rewa.CashbackMoney * Convert.ToDecimal(headInfo.Rate), 2);
                        }
                    }
                }
                catch { }
            }

            return result;
        }

        async IAsyncEnumerable<CourseDrpFxRewardLsItemDto> GetRewardList(Guid courseId)
        {
            var table = await _mediator.Send(new CourseGoodsPropsSmTableQuery { CourseId = courseId });
            foreach (var g in table)
            {
                var drpinfo = await _mediator.Send(new GetSkuFxSimpleInfoQuery { SkuId = g.GoodsId });
                var drpinfo_CashbackType = drpinfo?.CashbackType ?? 0;
                var drpinfo_CashbackValue = drpinfo?.CashbackValue ?? 0;
                if (drpinfo_CashbackValue <= 0)
                {
                    yield return null;
                }
                var money = 0m;
                switch ((CashbackTypeEnum)drpinfo_CashbackType)
                {
                    case CashbackTypeEnum.Percent:
                        {
                            //money = (g.Price * drpinfo_CashbackValue / 100m);

                            // 2021-10-29 虎叔叔应成都要求修改的
                            var costprice = g.Costprice ?? 0m;
                            var liyun = g.Price - costprice;
                            money = costprice == 0 || (liyun <= 0) ? 0 : (liyun) * 1 * drpinfo_CashbackValue / 100m;
                        }
                        break;
                    case CashbackTypeEnum.Yuan:
                        {
                            //money = drpinfo_CashbackValue;

                            // 2021-12-30 跟好方,沈叔叔确认 佣金设置为元的情况 没成本|成本为0|利润小于佣金设置 都是没佣金
                            var costprice = g.Costprice ?? 0m;
                            var liyun = g.Price - costprice;
                            money = costprice == 0 || liyun <= 0 || liyun < drpinfo_CashbackValue ? 0 : drpinfo_CashbackValue;
                        }
                        break;
                    default:
                        money = 0;
                        break;
                }
                yield return new CourseDrpFxRewardLsItemDto
                {
                    Id = g.GoodsId,
                    Name = string.Join('-', g.PropItems.Select(_ => _.PropItemName)),
                    Price = g.Price,

                    CashbackType = drpinfo_CashbackType,
                    CashbackTypeDesc = ((CashbackTypeEnum)drpinfo_CashbackType).GetDesc(),
                    CashbackValue = drpinfo_CashbackValue,
                    CashbackMoney = fmt_money(money, 2),
                };
            }
        }

        static decimal fmt_money(decimal v, int d)
        {
            if (d == 0) return decimal.Truncate(v);
            var str = v.ToString().AsSpan();
            var i = str.IndexOf('.');
            return i == -1 || (i + 1 + d) > str.Length ? v : decimal.Parse(str[..(i + 1 + d)]);
        }
    }
}
