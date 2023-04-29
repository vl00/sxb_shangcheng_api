using iSchool.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain
{
    public interface IOrgUnitOfWork : IUnitOfWork, IDisposable
    {
    }

}
