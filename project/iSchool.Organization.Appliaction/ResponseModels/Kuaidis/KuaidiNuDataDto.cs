using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 快递单数据dto
    /// </summary>
    public class KuaidiNuDataDto
    {
        /// <summary>
        /// 0=没错误 <br/>
        /// 其他值=第三方接口返回错误 <br/>
        /// </summary>
        public int Errcode { get; set; }
        /// <summary>错误信息</summary>
        public string? Errmsg { get; set; }

        /// <summary>id</summary>
        public Guid? Id { get; set; }        
        /// <summary>运单号</summary>
        public string Nu { get; set; } = default!;
        
        /// <summary>
        /// 成功时,快递轨迹s<br/>
        /// 错误时为null.
        /// </summary>
        public IEnumerable<KuaidiNuDataItemDto>? Items { get; set; }

        /// <summary>
        /// 接口数据来源类型 <br/>
        /// </summary>
        public int SrcType { get; set; }
#if DEBUG
        /// <summary>
        /// 第三方接口数据 <br/>      
        /// 正式接口此字段为null
        /// </summary>      
        public JToken? SrcResult { get; set; }
#else
        public JToken? SrcResult 
        {
            get => null;
            set => _ = value;
        }
#endif

        /// <summary>后端更新时间</summary>
        public DateTime? UpTime { get; set; }
        /// <summary>true=已收货</summary>
        public bool IsCompleted { get; set; }
        /// <summary>快递公司名</summary>
        public string? CompanyName { get; set; }
        /// <summary>快递公司code</summary>
        public string? CompanyCode { get; set; }
    }

    public class KuaidiNuDataItemDto
    {
        /// <summary>时间（格式 `yyyy-MM-dd HH:mm:ss`）</summary>
        public string Time { get; set; } = default!;
        /// <summary>轨迹</summary>
        public string Desc { get; set; } = default!;
    }

#nullable disable
}
