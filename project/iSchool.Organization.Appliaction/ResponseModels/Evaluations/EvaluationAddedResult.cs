using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 发评测结果
    /// </summary>
    public class EvaluationAddedResult
    {
        /// <summary>评测id</summary>
        public Guid Id { get; set; }
        /// <summary>评测短id</summary>
        public string Id_s { get; set; } = default!;     
        
        /// <summary>专题id</summary>
        public Guid? SpecialId { get; set; }
        /// <summary>专题短id</summary>
        public string? SpecialId_s { get; set; }
        /// <summary>专题名称</summary>
        public string? SpecialName { get; set; }

        /// <inheritdoc cref="EvaluationAddedResult_Activity1"/>
        public EvaluationAddedResult_Activity1? Activity1 { get; set; }
        /// <inheritdoc cref="EvaluationAddedResult_NewActivity"/>
        public EvaluationAddedResult_NewActivity? Activity { get; set; }
    }

    /// <summary>活动1数据 (已弃用)</summary>
    public class EvaluationAddedResult_Activity1
    {
        /// <summary>活动码(推广)码</summary>
        public string Pcode { get; set; } = default!;
        /// <summary>二维码</summary>
        public string Qrcode { get; set; } = default!;
    }

    /// <summary>(新)活动数据</summary>
    public class EvaluationAddedResult_NewActivity
    {
        /// <summary>活动码(推广)码</summary>
        public string Code { get; set; } = default!;
        /// <summary>
        /// 账号状态<br/> 
        /// ```
        /// /// <summary>账号正常</summary>
        /// [Description("账号正常")]
        /// Normal = 0,
        /// /// <summary>手机号异常</summary>
        /// [Description("手机号异常")]
        /// MobileExcp = 1,
        /// ```
        /// </summary>
        public int Ustatus { get; set; }
        /// <summary>
        /// 活动状态<br/>
        /// 参考接口'/api/Activity/info'返回的'status'字段
        /// </summary>
        public int Status { get; set; }
        /// <summary>base64二维码</summary>
        public string? Qrcode { get; set; }
    }

#nullable disable
}
