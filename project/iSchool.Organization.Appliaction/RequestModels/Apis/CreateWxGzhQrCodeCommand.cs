using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 创建wx公众号二维码cmd
    /// </summary>
    public class CreateWxGzhQrCodeCommand : IRequest<string?>
    {
        /// <summary>公众号AppName</summary>
        public string GzhAppName { get; set; } = default!;
        /// <summary>缓存(如redis)key, 用于wx回调时使用自定义参数</summary>
        public string CacheKey { get; set; } = default!;
        /// <summary>二维码过期秒数</summary>
        public int Expsec { get; set; } = 60 * 60 * 24 * 30;

        /// <summary>url获取公众号的accesstoken</summary>
        public string? AccessTokenApiUrl { get; set; } = default!; //= "https://wx.sxkid.com/api/accesstoken/gettoken?app={0}";
        /// <summary>url生成二维码post路径</summary>
        public string CreateQRCodeUrl { get; set; } = "https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={0}";
        /// <summary>url获取二维码路径</summary>
        public string GetQRCodeUrl { get; set; } = "https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={0}";
    }

#nullable disable
}
