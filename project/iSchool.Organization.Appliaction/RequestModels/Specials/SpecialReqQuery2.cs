using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 单个专题页\大专题页
    /// </summary>
    public class SpecialReqQuery2 : IRequest<ResponseModels.SpecialResEntity2>
    {
        #region 大专题增加请求参数
        /// <summary>专题类型(默认小专题  1:小专题；2：大专题)</summary>
        public byte SpecialType { get; set; } = (byte)SpecialTypeEnum.SmallSpecial;
        /// <summary>小专题短id(当SpecialType=2 并且 SmallShortId=null 则查大专题全部小专题的评测)</summary>
        public long? SmallShortId { get; set; }         
        #endregion

        /// <summary>专题短id</summary>
        public long No { get; set; }
        /// <summary>排序类型 1=最热 2=最新</summary>
        public int OrderBy { get; set; } = 1;
        /// <summary>页码</summary>
        public int PageIndex { get; set; } = 1;
    }
}
