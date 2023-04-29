using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Infrastructure.Extensions;
namespace iSchool.Organization.Appliaction.ViewModels.Coupon
{

    /// <summary>
    /// 商品/非SKU
    /// </summary>
    public class Goods
    {
        public Guid Id { get; set; }

        public string NoStr
        {
            get
            {
                return UrlShortIdUtil.Long2Base32(No);
            }
        }

        public string Title { get; set; }

        public List<string> Banner
        {
            get
            {
                if (string.IsNullOrEmpty(this.BannersJsn))
                    return new List<string>();
                else
                    return JsonConvert.DeserializeObject<List<string>>(this.BannersJsn);
            }
        }

        public List<string> BannerThumbnail
        {
            get
            {
                if (string.IsNullOrEmpty(this.BannerThumbnailsJsn))
                    return new List<string>();
                else
                    return JsonConvert.DeserializeObject<List<string>>(this.BannerThumbnailsJsn);
            }
        }

        public decimal? Price { get; set; }

        public decimal? OriginPrice { get; set; }
        [JsonIgnore]
        private string BannersJsn { get; set; }
        [JsonIgnore]
        private string BannerThumbnailsJsn { get; set; }
        [JsonIgnore]
        private long No { get; set; }
        [JsonIgnore]
        private int? Minage { get; set; }
        [JsonIgnore]
        private int? Maxage { get; set; }
        [JsonIgnore]
        private SubjectEnum? Subject { get; set; }
        [JsonIgnore]
        private bool NewUserExclusive { get; set; }
        [JsonIgnore]
        private bool CanNewUserReward { get; set; }
        [JsonIgnore]
        private bool LimitedTimeOffer { get; set; }

        public IEnumerable<string> Tags
        {
            get
            {
                //年龄标签
                if (this.Minage != null && this.Maxage != null)
                {
                    yield return $"{this.Minage}-{this.Maxage}岁";
                }
                else if (this.Minage != null && this.Maxage == null)
                {
                    yield return $"大于{this.Minage}岁";
                }
                else if (this.Maxage != null && this.Minage == null)
                {
                    yield return $"小于{this.Maxage}岁";
                }

                //科目标签
                if (this.Subject != null)
                    yield return EnumUtil.GetDesc((this.Subject.Value));

                //低价体验
                if (this.Price != null && this.Price <= 10)
                    yield return "低价体验";
                if (this.NewUserExclusive)
                    yield return "新人专享";
                if (this.CanNewUserReward)
                    yield return "新人立返";
                if (this.LimitedTimeOffer)
                    yield return "限时补贴";
            }
        }
    }
}
