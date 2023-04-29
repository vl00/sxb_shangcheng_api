using iSchool.Organization.Appliaction.ResponseModels.Apis;
using MediatR;

namespace iSchool.Organization.Appliaction.RequestModels
{

#nullable disable


    /// <summary>
    /// 长链接转短链接
    /// </summary>
    public class LongUrlToShortUrlRequest : IRequest<LongUrlToShortUrlResult>
    {
        /// <summary>
        /// 源链接
        /// </summary>
        public string OriginUrl { get; set; }
    }

#nullable disable
}
