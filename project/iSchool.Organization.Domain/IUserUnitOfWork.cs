using iSchool.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain
{
    public interface IUserUnitOfWork : IUnitOfWork, IDisposable
    {
    }

}
