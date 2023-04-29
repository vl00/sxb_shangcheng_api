using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>学制</summary>
    public enum EduSysType
    {
        /// <summary>九年制学校</summary>
        [Description("九年制学校")]
        Y9 = 9,
        /// <summary>十二年制学校</summary>
        [Description("十二年制学校")]
        Y12 = 12,
        /// <summary>十五年制学校</summary>
        [Description("十五年制学校")]
        Y15 = 15,
        /// <summary>十六年制学校</summary>
        [Description("十六年制学校")]
        Y16 = 16,
        /// <summary>初级中学</summary>
        [Description("初级中学")]
        JuniMidSch = 31,
        /// <summary>高级中学</summary>
        [Description("高级中学")]
        SeniMidSch = 32,
        /// <summary>完全中学</summary>
        [Description("完全中学")]
        FullMidSch = 33,
    }
}
