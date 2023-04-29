using iSchool.Organization.Domain.Security;
using System;
using System.Collections.Generic;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction
{
    /// <summary>
    /// 我(已登录用户)
    /// </summary>
    public interface IPageMeInfo
    {
        IUserInfo? Me { get; set; }
    }
}
#nullable disable