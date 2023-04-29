using iSchool.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.SeedWork
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
    }
}
