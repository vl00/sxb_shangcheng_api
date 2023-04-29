using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class SensitiveKeywordCmdResult
    {
        /// <summary>是否通过</summary>
        public bool Pass { get; set; }
        /// <summary>默认不通过时的消息</summary>
        public string? NotpassMessage { get; set; }

        /// <summary>
        /// 不通过时被*替换后的文本
        /// </summary>
        public string? FilteredTxt { get; set; }
        /// <summary>
        /// 不通过时被*替换后的文本s
        /// </summary>
        public string[]? FilteredTxts { get; set; }

        public JToken SrcData { get; set; } = default!;
    }

#nullable disable
}
