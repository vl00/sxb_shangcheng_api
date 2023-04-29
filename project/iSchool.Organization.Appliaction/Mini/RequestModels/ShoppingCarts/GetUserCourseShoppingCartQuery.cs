using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// get用户购物车
    /// </summary>
    public class GetUserCourseShoppingCartQuery : IRequest<CourseShoppingCartDto>
    { 
        /// <summary>用户id</summary>
        public Guid UserId { get; set; }

        /// <summary>需要合并的临时项s</summary>
        public IEnumerable<CourseShoppingCartItem>? Temps { get; set; }
    }

    public class CourseShoppingCartItem
    {
        /// <summary>商品id</summary>
        public Guid GoodsId { get; set; }
        /// <summary>数量</summary>        
        public int Count { get; set; }
        /// <summary>是否选中</summary>
        public bool Selected { get; set; }
        /// <summary>时间戳</summary>
        [JsonConverter(typeof(DateTimeToTimestampJsonConverter))]
        public DateTime Time { get; set; }

        /// <summary>
        /// 前端传入的json对象`{}`额外数据.可null <br/>
        /// 例如 `{ fw: '', eid: '', surl: '' }`
        /// </summary>
        public JObject? Jo { get; set; }
    }

#nullable disable
}
