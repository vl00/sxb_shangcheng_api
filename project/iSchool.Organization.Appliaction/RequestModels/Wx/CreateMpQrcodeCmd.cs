using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 生成微信小程序二维码
    /// </summary>
    public class CreateMpQrcodeCmd : IRequest<CreateMpQrcodeCmdResult>
    {
        /// <summary>服务号appName</summary>
        public string AppName { get; set; } = default!;
        /// <summary>小程序页面路径</summary>
        public string Page { get; set; } = default!;
        /// <summary>小程序页面路径参数？</summary>
        public string? Scene { get; set; }
    }

#nullable disable
}
