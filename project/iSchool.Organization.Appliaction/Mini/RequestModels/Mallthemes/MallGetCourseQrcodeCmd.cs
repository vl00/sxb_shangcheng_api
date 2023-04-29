using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 点击弹出商品详情小程序码
    /// </summary>
    public class MallGetCourseQrcodeCmd : IRequest<MallGetCourseQrcodeCmdResult>
    {
        /// <summary>商品spu短id</summary>
        public string Id { get; set; } = default!;
    }

#nullable disable
}
