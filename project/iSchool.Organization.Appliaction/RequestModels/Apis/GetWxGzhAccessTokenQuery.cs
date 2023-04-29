using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.wx;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// get wx公众号 AccessToken
    /// </summary>
    public class GetWxGzhAccessTokenQuery : IRequest<AccessTokenApiData>
    {
        /// <summary>公众号AppName</summary>
        public string GzhAppName { get; set; } = default!;
       
        /// <summary>url获取公众号的accesstoken</summary>
        public string AccessTokenApiUrl { get; set; } = null!; //= "https://wx.sxkid.com/api/accesstoken/gettoken?app={0}";
    }

#nullable disable
}
