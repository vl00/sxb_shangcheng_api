using iSchool.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain
{
    #region WX
    public interface IWXUnitOfWork : IUnitOfWork, IDisposable
    {
    }
    #endregion
    #region Openid_WX
    public interface IOpenid_WXUnitOfWork : IUnitOfWork, IDisposable
    {
    }
    #endregion
}
