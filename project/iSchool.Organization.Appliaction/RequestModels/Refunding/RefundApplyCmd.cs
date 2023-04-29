using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    public class RefundApplyCmd : IRequest<RefundApplyCmdResult>
    {
        /// <summary>订单详情id</summary>
        public Guid OrderDetailId { get; set; }
        /// <summary>退款类型 1=退款, 2=退货, 3=极速退款</summary>
        public int RefundType { get; set; }

        /// <summary>退款数量</summary>
        public int RefundCount { get; set; } // >=1

        /// <summary>
        /// 退款理由选择：<br/>
        /// 仅退款时  1.不想要了、2.商品信息拍错（属性/颜色等）、3.地址/电话信息填写错误、4.拍多了、5.协商一致退款、6.缺货、7.发货速度不满意、8.其他。
        /// <br/><br/>
        /// 退货时    11.颜色/尺寸/参数不符、12.商品瑕疵、13.质量问题、14.少件/漏发、15.其他。
        /// </summary> 
        public byte? Cause { get; set; }
        /// <summary>
        /// 退货方式 1=快递寄回
        /// </summary>
        public byte? ReturnMode { get; set; } // =1

        /// <summary>补充描述</summary>
        public string? Desc { get; set; } 
        /// <summary>凭证图片urls</summary>
        public string[]? Vouchers { get; set; }
        /// <summary>凭证图片缩略图urls</summary>
        public string[]? Vouchers_s { get; set; }
    }

#nullable disable
}
