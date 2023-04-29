using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iSchool.Organization.Appliaction.Queries.Models
{
    public class AdvanceOrderDetail
    {
        /// <summary>
        /// order.detail.id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        public decimal OriTotalAmount { get; set; }

        /// <summary>
        /// 应付
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 折后实付
        /// </summary>
        public decimal Payment { get; set; }

        /// <summary>
        /// 折扣金额
        /// </summary>
        public decimal Discount => OriTotalAmount - Payment;

        /// <summary>
        /// json course value
        /// </summary>
        public string Ctn { get; set; }

        /// <summary>
        /// 属性列表
        /// </summary>
        public IEnumerable<string> PropItemNames => GetJsonValues(Ctn, "propItemNames");

        /// <summary>
        /// {"id":"7d2addef-4449-49ef-a7e9-cefa6e4ec117","no":413,"title":"翻斗乐园plus","subtitle":"new","prodType":2,
        /// "banner":"https://cos.sxkid.com/images/org/eval/3d6a02a24db8450983aadfbb7f6d9308/3d6a02a24db8450983aadfbb7f6d9308_s.png",
        /// "authentication":true,"orgId":"05bd4259-37a9-46bb-9818-f967f43d87f8","orgNo":434,"orgName":"智能学具品牌",
        /// "orgLogo":"https://cos.sxkid.com/images/org/eval/56fe4ae652824582b8319463060f9ef1/56fe4ae652824582b8319463060f9ef1.png",
        /// "orgDesc":"我来了哈","orgSubdesc":"我来了啦","goodsId":"52d1854a-cb76-442f-9d92-136e39265c57",
        /// "propItemNames":["胡英俊"],"propItemIds":["605542cf-d70f-496b-ad8e-cecb8c5bf147"],"isNewUserExclusive":false,"_Ver":"v4",
        /// "_FxHeaducode":null,"_prebindFxHead_ok":null,"_RwInviteActivity":null}
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetJsonValues(string json, string key)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<JObject>(json);

                if (obj != null && obj.ContainsKey(key))
                {
                    return obj.GetValue(key).Values<string>();
                }
                return Enumerable.Empty<string>();
            }
            finally { }
        }

    }
}