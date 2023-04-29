using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class GG21Query : IRequest<GG21QryResult>
    {
        public AgesClass[] Ages { get; set; } = default!;

        public class AgesClass
        {
            /// <summary>最小年龄</summary>
            public int? MinAge { get; set; }
            /// <summary>最大年龄</summary>
            public int? MaxAge { get; set; }
        }

        /// <summary>
        /// 科目s <br/>
        /// `不传` = 全部 <br/>
        /// `199` = 其他 <br/>
        /// 多个用 `,` 区分
        /// </summary>
        public string? Subjs { get; set; }
        /// <summary>
        /// 价格类型 <br/>
        /// `0 或 不传` = 全部 <br/>
        /// `1` = 100元以上 <br/>
        /// `2` = 100元以下 <br/>
        /// </summary>
        public int Price { get; set; } = 0;
        /// <summary>
        /// 是否需要返回小程序二维码<br/>
        /// 测试默认false, 正式默认true
        /// </summary>
#if DEBUG
        public bool Mp { get; set; } = false;
#else
        public bool Mp { get; set; } = true;
#endif
    }
}

#nullable disable
