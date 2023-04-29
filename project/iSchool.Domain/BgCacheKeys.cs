using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain
{
    public static partial class BgCacheKeys
    {
        // bgdata:*

        /// <summary>锁</summary>
        public const string DgAy_import = "bgdata:dgay_import";
        /// <summary>导入结果</summary>
        public const string DgAy_import_result = "bgdata:dgay_import:{0}";

        /// <summary>用于删除前端caches</summary>
        public const string DgAyKeys = "DegreeAnalyze:*";

        /// <summary>导出结果</summary>
        public const string DgAy_export_result = "bgdata:dgay_export:{0}";
    }
}
