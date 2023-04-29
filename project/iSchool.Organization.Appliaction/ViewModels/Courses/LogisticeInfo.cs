using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Courses
{
    /// <summary>
    /// 物流信息返回实体
    /// </summary>
    public class LogisticeInfo
    {
        /// <summary>
        /// 兑换码
        /// </summary>
        public string DHCode { get; set; }

        /// <summary>
        /// 快递公司名称
        /// </summary>
        public string LogName { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string LogNumber { get; set; }

        /// <summary>
        /// 成功时,快递轨迹s<br/>
        /// 错误时为null.
        /// </summary>
        public List<LogisticeInfoItem> Items { get; set; }

        /// <summary>true=已收货</summary>
        public bool IsCompleted { get; set; }

    }

    public class LogisticeInfoItem 
    {
        /// <summary>时间（格式 `yyyy-MM-dd HH:mm:ss`）</summary>
        public DateTime Time { get; set; } = default!;
        /// <summary>轨迹</summary>
        public string Desc { get; set; } = default!;
    }

}
