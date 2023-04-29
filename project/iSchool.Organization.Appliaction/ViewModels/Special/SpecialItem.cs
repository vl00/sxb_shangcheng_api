using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Special
{
    /// <summary>
    /// 专题列表Item返回实体
    /// </summary>
    public class SpecialItem
    {

        /// <summary>
        /// 专题Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        /// 专题名称(标题)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 专题副标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 测评数量(专题中上架的评测总数)
        /// </summary>
        public int? EvltCount { get; set; }

        /// <summary>
        /// 专题状态（1:上架;2:下架;）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 专题类型（1：小专题；2：大专题；）
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 分享标题
        /// </summary>
        public string ShareTitle { get; set; }

        /// <summary>
        /// 分享副标题
        /// </summary>
        public string ShareSubTitle { get; set; }

        /// <summary>
        /// 专题海报
        /// </summary>
        public string Banner { get; set; }

        /// <summary>
        /// 专题Url
        /// </summary>
        public string SpecialUrl { get; set; }

        /// <summary>
        /// 专题NO
        /// </summary>
        public byte No { get; set; }
    }
}
