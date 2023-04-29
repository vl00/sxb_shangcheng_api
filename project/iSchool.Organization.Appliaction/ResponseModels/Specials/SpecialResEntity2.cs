using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 单个专题页
    /// </summary>
    public class SpecialResEntity2
    {
        /// <summary>专题id</summary>
        public Guid Id { get; set; }
        /// <summary>标题</summary>
        public string Title { get; set; }
        /// <summary>副标题</summary>
        public string SubTitle { get; set; }
        /// <summary>专题的图片/海报</summary>
        public string Banner { get; set; }
        /// <summary>专题类型(1=小专题 2=大专题)</summary>
        public byte SpecialType { get; set; } = (byte)SpecialTypeEnum.SmallSpecial;
        /// <summary>小专题集合</summary>
        public IEnumerable<SmallSpecialItem> SmallSpecialItems { get; set; }
        /// <summary>评测分页info</summary>
        public LoadMoreResult<EvaluationItemDto> PageInfo { get; set; }
        
    }

    /// <summary>
    /// 小专题
    /// </summary>
    public class SmallSpecialItem
    {
        /// <summary>小专题id</summary>
        public Guid Id { get; set; }
        /// <summary>小专题短Id</summary>
        public string Id_s { get; set; }
        /// <summary>标题</summary>
        public string Title { get; set; }
        /// <summary>副标题</summary>
        public string SubTitle { get; set; }
    }

}
