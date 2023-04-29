using Dapper.Contrib.Extensions;
using iSchool.Organization.Domain.Event.Coupon;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate
{
    [Dapper.Contrib.Extensions.Table("CouponReceive")]
    public class CouponReceive : Entity, IAggregateRoot
    {
        [Write(false)]
        public int Number { get; set; }

        public Guid CouponId { get; private set; }

        public Guid UserId { get; private set; }

        public DateTime GetTime { get; private set; }

        /// <summary>
        /// 有效开始时间
        /// </summary>
        public DateTime VaildStartTime { get; private set; }
        /// <summary>
        /// 有效结束时间
        /// </summary>
        public DateTime VaildEndTime { get; private set; }

        public DateTime? UsedTime { get; private set; }

        public CouponReceiveState Status { get; private set; }
        public Guid? OrderId { get; private set; }

        public CouponReceiveOriginType OriginType { get; private set; }
        public DateTime? ReadTime { get; private set; }
        public string Remark { get; private set; }

        public bool WillExpireMessageNotify { get; private set; }



        public CouponReceive(Guid id, Guid couponId
            , Guid userId, DateTime getTime, DateTime vaildStartTime, DateTime vaildEndTime
            , CouponReceiveState state = CouponReceiveState.WaitUse, DateTime? usedTime = null
            , Guid? orderId = null, CouponReceiveOriginType originType = CouponReceiveOriginType.SelfReceive , DateTime? readTime = null, string remark = null, int? number = null)
        {
            this.Id = id;
            this.Number = number.GetValueOrDefault();
            this.CouponId = couponId;
            this.UserId = userId;
            this.GetTime = getTime;
            this.VaildStartTime = vaildStartTime;
            this.VaildEndTime = vaildEndTime;
            this.UsedTime = usedTime;
            this.Status = state;
            this.OrderId = orderId;
            this.OriginType = originType;
            this.ReadTime = readTime;
            this.Remark = remark;
        }


        /// <summary>
        /// 发送即将过期消息通知事件
        /// </summary>
        /// <returns></returns>
        public bool SendWillExpireMsgNotifyEvent()
        {
            if (this.WillExpireMessageNotify || this.Status!= CouponReceiveState.WaitUse || this.VaildEndTime < DateTime.Now)
            {
                return false;
            }
            else {
                if (DateTime.Now.Date == this.VaildEndTime.Date.AddDays(-1))
                {
                    //过期的前一天
                    this.WillExpireMessageNotify = true;
                    this.AddDomainEvent(new CouponReceiveWillExpireDomainEvent(this));
                    return true;

                }
                else { 
                    return false;
                }
            }

        }

        public void SetUsedTime(DateTime dateTime)
        {
            this.UsedTime = dateTime;
        }

        public void SetStatus(CouponReceiveState state)
        {
            if (state == CouponReceiveState.PreUse)
            {
                if (this.Status == CouponReceiveState.WaitUse)
                {
                    this.Status =  CouponReceiveState.PreUse;
                }
                else
                {
                    throw new Exception(" Current State can not transfer to PreUse");
                }
            }else if (state == CouponReceiveState.Used)
            {
                if (this.Status == CouponReceiveState.PreUse)
                {
                    this.Status =  CouponReceiveState.Used;
                }
                else {
                    throw new Exception(" Current State can not transfer to Used");
                }
            }else  if(state == CouponReceiveState.WaitUse)
            {
                if (this.Status == CouponReceiveState.Used)
                {
                    throw new Exception(" Current State can not transfer to WaitUse");
                }
                else {
                    this.Status = CouponReceiveState.WaitUse;
                }
            }

        }

        public void SetOrderId(Guid? id)
        {
            this.OrderId = id;
        }


        /// <summary>
        /// 检查该券是否可用，不可用将抛出异常。
        /// </summary>
        public (bool res, string msg) CanUseCheck(Guid userId)
        {
            if (UserId != userId)
            {
                return (false, "无权使用该券");
            }

            if (this.VaildStartTime > DateTime.Now)
            {
                return (false, "该券未到使用时间。");
            }
            if (this.VaildEndTime < DateTime.Now)
            {
                return (false, "该券已过期。");
            }
            if (this.Status == CouponReceiveState.PreUse)
            {
                return (false, "该券已被预占用。");
            }
            if (this.Status == CouponReceiveState.Used)
            {
                return (false, "该券已使用。");
            }
            return (true, null);
        }



    }
}
