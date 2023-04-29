using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 分页查询某专题下的评测列表
    /// </summary>
    public class SpecialLoadMoreEvaluationsQuery : IRequest<LoadMoreResult<EvaluationItemDto>>
    {
        /// <summary>
        /// 专题id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 排序类型 1=最热 2=最新
        /// </summary>
        public int OrderBy { get; set; } = 1;
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        #region 大专题增加请求参数
        /// <summary>专题类型(默认小专题  1:小专题；2：大专题)</summary>
        public byte SpecialType { get; set; } = (byte)SpecialTypeEnum.SmallSpecial;

        /// <summary>小专题Id </summary>
        public Guid SmallId { get; set; } = default;
        #endregion

    }
}
