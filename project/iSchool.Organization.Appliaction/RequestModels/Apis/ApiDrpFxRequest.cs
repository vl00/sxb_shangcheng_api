using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    public interface IAAA : iSchool.Domain.Repository.Interfaces.IDependency { }
    public class AAA : IAAA { }

    public class ApiDrpFxRequest : IRequest<ApiDrpFxResponse>
    {
        [JsonIgnore]
        public object Ctn { get; set; } = default!;
        
        public BecomSecondCmd? BecomSecond
        {
            get => Ctn as BecomSecondCmd;
            set => Ctn ??= value!;
        }
        /// <summary>成为下线</summary>
        public class BecomSecondCmd
        {
            public Guid UserId { get; set; }
            /// <summary>上级.default表示预锁</summary>
            public Guid HeadUserId { get; set; }
            /// <summary>课程名.用于拉新后发短信等.</summary>
            public string? CourseName { get; set; }
            /// <summary>是否强制跳过锁粉进行绑定</summary>
            public int ForceChangePrefan { get; set; } = 0;
        }

        public AddFxOrderCmd? AddFxOrder
        {
            get => Ctn as AddFxOrderCmd;
            set => Ctn ??= value!;
        }
        /// <summary>录入大课</summary>
        public class AddFxOrderCmd : IRequest<ApiDrpFxResponse.AddFxOrderCmdResult?>
        {
            /// <summary>买课人</summary>
            public Guid UserId { get; set; }
            /// <summary>商品Id 如: 课程Id</summary>
            public Guid ObjectId { get; set; }
            /// <summary>商品名称 如: 课程名称</summary>
            public string ObjectName { get; set; } = default!;
            /// <summary>商品图片</summary>
            public string ObjectImgUrl { get; set; } = default!;
            /// <summary>商品(对象)扩展信息</summary>
            public JToken? ObjectExtensions { get; set; }
            /// <summary>购买数量</summary>
            public decimal Number { get; set; } = 1;
            /// <summary>实际支付金额</summary>
            public decimal PayAmount { get; set; }
            /// <summary>实际支付优惠金额</summary>
            public decimal PayDisccountAmout { get; set; } = 0;
            /// <summary>支付时间</summary>
            public DateTime? PayTime { get; set; }
            /// <summary>奖金锁定截止日期</summary>
            public DateTime? BonusLockEndTime { get; set; }
            /// <summary>上线独享奖励</summary>
            public List<BonusItemDto>? BonusItems { get; set; }

            /// <summary>分销单类型(商品类型) 1 课程购买小课 2 课程购买大课</summary>
            public int OrderType => 1;
            /// <summary>平台其他系统订单</summary>
            public Guid OrderId { get; set; }
            /// <summary>平台其他系统订单号</summary>
            public string RelationOrderNo { get; set; } = default!;

            public string? _FxHeaducode { get; set; }

            /// <summary>是否1.6新小程序</summary>
            public bool IsMp { get; set; }
            /// <summary>商品是否隐形上架</summary>
            public int IsInvisibleOnline { get; set; } = 0;
            /// <summary>
            /// 商品类型 1=网课 2=好物
            /// </summary>
            public int CourseType { get; set; }

            /// <summary>only for log</summary>
            public Domain.CourseDrpInfo? _CourseDrpInfo { get; set; }
            /// <summary>only for log</summary>
            public Domain.CourseGoodDrpInfo? _CourseGoodDrpInfo { get; set; }
        }

        /// <summary>活动期间新下线可能成为顾问</summary>
        public class BecomHeadUserInHdCmd
        {
            public Guid UserId { get; set; }
        }

        public GetConsultantRateQry? GetConsultantRate
        {
            get => Ctn as GetConsultantRateQry;
            set => Ctn ??= value!;
        }
        /// <summary>获取用户是顾问时的系数</summary>
        public class GetConsultantRateQry : IRequest<ApiDrpFxResponse.GetConsultantRateQryResult?>
        { 
            public Guid UserId { get; set; }
        }

        /// <summary>奖金项目 返现/分佣</summary>
        public class BonusItemDto
        {
            /// <summary>
            /// 提成类型 1 自购返现(直推奖励) 2 上级分佣(间推奖励) 3 上线独享 4 平级奖励
            /// </summary>
            public int Type { get; set; }

            /// <summary>提成金额</summary>
            public decimal Amount { get; set; }
            /// <summary>提成比例, 百分比是100分制</summary>           
            public decimal Rate { get; set; }
            /// <summary>
            /// 提成比例类型 1 百分比(100分制, 可>100%) 2 元
            /// </summary>           
            public int RateType { get; set; }
        }

        #region 确认收货确定佣金有效
        /// <summary>确认收货确定佣金有效</summary>
        public class OrgOrderSettleCmd
        {
            public Guid UserId { get; set; }
            public IEnumerable<OrgOrderSettleCmd_param> Param { get; set; } = default!;
        }
        public class OrgOrderSettleCmd_param
        {
            /// <summary>(子)订单id</summary>
            public Guid _OrderId { get; set; }
            /// <summary>订单详情id</summary>
            public Guid OrderDetailId { get; set; }
            public DateTime BonusLockEndTime { get; set; }
        }
        #endregion 确认收货确定佣金有效
    }

    public class ApiDrpFxRequest2 : IRequest<ApiDrpFxResponse>
    {
        public ApiDrpFxRequest.AddFxOrderCmd? AddFxOrder { get; set; }
    }

#nullable disable
}
